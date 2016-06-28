namespace Logging.ExceptionSender
{
    using System;
    using System.IO;

    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;

    using RecurrentTasks;
    
    public abstract class ExceptionSenderTask : TaskBase<TaskRunStatus>
    {
        private static readonly TimeSpan ShortInterval = TimeSpan.FromMinutes(1);

        private static readonly TimeSpan LongInterval = TimeSpan.FromHours(1);

        private ExceptionSenderOptions options;

        private IHostingEnvironment hostingEnvironment;

        public ExceptionSenderTask(ILoggerFactory loggerFactory, IServiceScopeFactory serviceScopeFactory, IOptions<ExceptionSenderOptions> options, IHostingEnvironment hostingEnvironment)
            : base(loggerFactory, ShortInterval, serviceScopeFactory)
        {
            this.options = options.Value;
            this.hostingEnvironment = hostingEnvironment;
        }

        /// <summary>
        /// Append information about exception to send queue. Real send will perform later, in separate thread/task.
        /// </summary>
        public void LogException(Exception ex)
        {
            var baseDir = hostingEnvironment.ContentRootPath;

            var subfolderName = string.Format("{0}{1}", options.SubfolderPrefix, DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss-fff"));
            var subfolderPath = Path.Combine(baseDir, options.FolderName, subfolderName);

            Directory.CreateDirectory(subfolderPath);

            var path = Path.Combine(subfolderPath, options.ExceptionFileName);

            var errorText = string.Format("{2}: {0}\r\n\r\n{1}", ex.Message, ex.ToString(), ex.GetType().FullName);
            File.WriteAllText(path, errorText);
            Logger.LogInformation("Exception details saved to: {0}", path);

            path = Path.Combine(subfolderPath, options.LogFileName);

            var log = iflight.Logging.MemoryLogger.LogList;
            File.WriteAllLines(path, log);
            Logger.LogInformation("Exception log saved to: {0}", path);

            if (IsStarted)
            {
                TryRunImmediately();
                Logger.LogDebug("TryRunImmediately called");
            }
            else
            {
                Logger.LogWarning("Task not started - mail with exception data will not be sent");
            }
        }

        protected override void Run(IServiceProvider serviceProvider, TaskRunStatus runStatus)
        {
            var appEnv = serviceProvider.GetService<IHostingEnvironment>();

            var baseDir = Path.Combine(appEnv.ContentRootPath, options.FolderName);

            if (!Directory.Exists(baseDir))
            {
                Interval = LongInterval;
                Logger.LogInformation("Папка с логами ошибок не существует, нечего отправлять: {0}", baseDir);
                return;
            }

            var exceptions = Directory.GetDirectories(baseDir, options.SubfolderPrefix + "*", SearchOption.TopDirectoryOnly);

            if (exceptions.Length == 0)
            {
                Interval = LongInterval;
                Logger.LogInformation("Папка с логами ошибок пуста, нечего отправлять: {0} ", baseDir);
                return;
            }

            // есть что отправлять - значит "на случай ошибок" уменьшает интервал (до след. попытки)
            Interval = ShortInterval;

            var count = 0;
            foreach (string exception in exceptions)
            {
                var exceptionFilePath = Path.Combine(exception, options.ExceptionFileName);
                if (File.Exists(exceptionFilePath))
                {
                    var exceptionText = File.ReadAllText(exceptionFilePath);

                    var logFile = new FileInfo(Path.Combine(exception, options.LogFileName));

                    Send(serviceProvider, runStatus, exceptionText, logFile);

                    Directory.Delete(exception, true);
                    count++;
                }
            }
            Logger.LogInformation("Успешно отправлено {0} писем с логами ошибок.", count);
        }

        protected abstract void Send(IServiceProvider serviceProvider, TaskRunStatus runStatus, string text, FileInfo logFile);
    }
}

