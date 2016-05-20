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

        public ExceptionSenderTask(ILoggerFactory loggerFactory, IServiceScopeFactory serviceScopeFactory, IOptions<ExceptionSenderOptions> options)
            : base(loggerFactory, ShortInterval, serviceScopeFactory)
        {
            this.options = options.Value;
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

