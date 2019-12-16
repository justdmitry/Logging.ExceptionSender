namespace Logging.ExceptionSender
{
    using System;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
#if NETCOREAPP2_1
    using Microsoft.AspNetCore.Hosting;
#else
    using Microsoft.Extensions.Hosting;
#endif
    using Microsoft.Extensions.Logging;
    using RecurrentTasks;

    public abstract class ExceptionSenderTask : IRunnable
    {
        private static readonly TimeSpan ShortInterval = TimeSpan.FromMinutes(1);

        private static readonly TimeSpan LongInterval = TimeSpan.FromHours(1);

        private readonly ILogger logger;

        private readonly ExceptionSenderOptions options;

        private readonly string contentRootPath;

        public ExceptionSenderTask(
            ILogger logger,
            ExceptionSenderOptions options,
#if NETCOREAPP2_1
            IHostingEnvironment hostEnvironment
#else
            IHostEnvironment hostEnvironment
#endif
            )
        {
            this.logger = logger;
            this.options = options;
            this.contentRootPath = hostEnvironment?.ContentRootPath ?? throw new ArgumentNullException(nameof(hostEnvironment));
        }

        public async Task RunAsync(ITask currentTask, IServiceProvider scopeServiceProvider, CancellationToken cancellationToken)
        {
            currentTask = currentTask ?? throw new ArgumentNullException(nameof(currentTask));

            var baseDir = Path.Combine(contentRootPath, options.FolderName);

            if (!Directory.Exists(baseDir))
            {
                currentTask.Options.Interval = LongInterval;
                logger.LogInformation("Папка с логами ошибок не существует, нечего отправлять: {0}", baseDir);
                return;
            }

            var exceptions = Directory.GetDirectories(baseDir, options.SubfolderPrefix + "*", SearchOption.TopDirectoryOnly);

            if (exceptions.Length == 0)
            {
                currentTask.Options.Interval = LongInterval;
                logger.LogInformation("Папка с логами ошибок пуста, нечего отправлять: {0} ", baseDir);
                return;
            }

            // есть что отправлять - значит "на случай ошибок" уменьшает интервал (до след. попытки)
            currentTask.Options.Interval = ShortInterval;

            var count = 0;
            foreach (string exception in exceptions)
            {
                var exceptionFilePath = Path.Combine(exception, options.ExceptionFileName);
                if (File.Exists(exceptionFilePath))
                {
                    using (var fs = File.OpenText(exceptionFilePath))
                    {
                        var exceptionText = await fs.ReadToEndAsync().ConfigureAwait(false);

                        var logFile = new FileInfo(Path.Combine(exception, options.LogFileName));

                        await SendAsync(currentTask, exceptionText, logFile).ConfigureAwait(false);
                    }

                    Directory.Delete(exception, true);
                    count++;
                }
            }
            logger.LogInformation("Успешно отправлено {0} писем с логами ошибок.", count);
        }

        protected abstract Task SendAsync(ITask currentTask, string text, FileInfo logFile);
    }
}

