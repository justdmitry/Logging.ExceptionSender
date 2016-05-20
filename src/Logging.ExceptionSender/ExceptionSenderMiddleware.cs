namespace Logging.ExceptionSender
{
    using System;
    using System.IO;
    using System.Threading.Tasks;

    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;

    using iflight.Logging;

    public class ExceptionSenderMiddleware
    {
        private readonly RequestDelegate nextMiddleware;
        private readonly ILogger logger;
        private readonly string appBasePath;
        private readonly ExceptionSenderOptions options;
        private readonly ExceptionSenderTask senderTask;

        public ExceptionSenderMiddleware(
            RequestDelegate next,
            IHostingEnvironment appEnv,
            ILogger<ExceptionSenderMiddleware> logger,
            IOptions<ExceptionSenderOptions> options,
            ExceptionSenderTask senderTask
            )
        {
            nextMiddleware = next;
            appBasePath = appEnv.ContentRootPath;
            this.options = options.Value;
            this.senderTask = senderTask;
            this.logger = logger;
            logger.LogDebug("Created.");
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await nextMiddleware(context);
            }
            catch (Exception ex)
            {
                logger.LogWarning("An unhandled exception has occurred, logging it: {0}", ex.Message);

                var subfolderName = string.Format("{0}{1}", options.SubfolderPrefix, DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss-fff"));
                var subfolderPath = Path.Combine(appBasePath, options.FolderName, subfolderName);

                Directory.CreateDirectory(subfolderPath);

                var path = Path.Combine(subfolderPath, options.ExceptionFileName);

                var errorText = string.Format("{2}: {0}\r\n{1}", ex.Message, ex.StackTrace, ex.GetType().FullName);
                using (StreamWriter stream = File.CreateText(path))
                {
                    stream.Write(errorText);
                }
                logger.LogInformation("Exception details saved to: {0}", path);

                path = Path.Combine(subfolderPath, options.LogFileName);

                var log = MemoryLogger.LogList;
                File.WriteAllLines(path, log);
                logger.LogInformation("Exception log saved to: {0}", path);

                if (senderTask.IsStarted)
                {
                    senderTask.TryRunImmediately();
                    logger.LogDebug("ExceptionSenderTask called");
                }
                else
                {
                    logger.LogWarning("ExceptionSenderTask not started - mail with exception data will not be sent");
                }

                throw;
            }
        }
    }
}
