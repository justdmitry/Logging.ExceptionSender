namespace Logging.ExceptionSender
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Threading.Tasks;
#if NETCOREAPP2_1
    using Microsoft.AspNetCore.Hosting;
#else
    using Microsoft.Extensions.Hosting;
#endif
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;

    using RecurrentTasks;

    public class ExceptionAccumulator : IExceptionAccumulator
    {
        private readonly ILogger logger;

        private readonly ExceptionSenderOptions options;

        private readonly string contentRootPath;

        private readonly ITask exceptionSenderTask;

        public ExceptionAccumulator(
            ILogger<ExceptionAccumulator> logger,
            IOptions<ExceptionSenderOptions> options,
#if NETCOREAPP2_1
            IHostingEnvironment hostEnvironment,
#else
            IHostEnvironment hostEnvironment,
#endif
            ITask<ExceptionSenderTask> exceptionSenderTask)
        {
            this.logger = logger;
            this.options = options.Value;
            this.contentRootPath = hostEnvironment.ContentRootPath;
            this.exceptionSenderTask = exceptionSenderTask;
        }

        /// <summary>
        /// Append information about exception to send queue. Real send will perform later, in separate thread/task.
        /// </summary>
        public async Task SaveExceptionAsync(Exception ex)
        {
            ex = ex ?? throw new ArgumentNullException(nameof(ex));

            var baseDir = contentRootPath;

            var subfolderName = string.Format(CultureInfo.InvariantCulture, "{0}{1}", options.SubfolderPrefix, DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss-fff", CultureInfo.InvariantCulture));
            var subfolderPath = Path.Combine(baseDir, options.FolderName, subfolderName);

            Directory.CreateDirectory(subfolderPath);

            var path = Path.Combine(subfolderPath, options.ExceptionFileName);

            var errorText = string.Format(CultureInfo.InvariantCulture, "{2}: {0}\r\n\r\n{1}", ex.Message, ex.ToString(), ex.GetType().FullName);
            await File.WriteAllTextAsync(path, errorText).ConfigureAwait(false);
            logger.LogInformation($"Exception details saved to: {path}");

            path = Path.Combine(subfolderPath, options.LogFileName);

            var log = Logging.Memory.MemoryLogger.LogList;
            await File.WriteAllLinesAsync(path, log).ConfigureAwait(false);
            logger.LogInformation($"Exception log saved to: {path}");

            if (exceptionSenderTask.IsStarted)
            {
                exceptionSenderTask.TryRunImmediately();
                logger.LogDebug("TryRunImmediately called");
            }
            else
            {
                logger.LogWarning("Task not started - mail with exception data will not be sent");
            }
        }
    }
}
