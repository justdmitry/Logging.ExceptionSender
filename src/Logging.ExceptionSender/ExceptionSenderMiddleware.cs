namespace Logging.ExceptionSender
{
    using System;
    using System.IO;
    using System.Threading.Tasks;
    using Microsoft.AspNet.Builder;
    using Microsoft.AspNet.Http;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.OptionsModel;
    using Microsoft.Extensions.PlatformAbstractions;
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
            ILoggerFactory loggerFactory,
            IApplicationEnvironment appEnv,
            IOptions<ExceptionSenderOptions> options,
            ExceptionSenderTask senderTask
            )
        {
            nextMiddleware = next;
            appBasePath = appEnv.ApplicationBasePath;
            this.options = options.Value;
            this.senderTask = senderTask;
            logger = loggerFactory.CreateLogger<ExceptionSenderMiddleware>();
            logger.LogDebug("Created");
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await nextMiddleware(context);
            }
            catch (Exception ex)
            {
                logger.LogWarning("An unhandled exception has occurred: " + ex.Message);

                var subfolderName = string.Format("{0}{1}", options.SubfolderPrefix, DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss-fff"));
                var subfolderPath = Path.Combine(appBasePath, options.FolderName, subfolderName);

                Directory.CreateDirectory(subfolderPath);

                var path = Path.Combine(subfolderPath, options.ExceptionFileName);

                var errorText = string.Format("{0}\r\n{1}", ex.Message, ex.StackTrace);
                using (StreamWriter stream = File.CreateText(path))
                {
                    stream.Write(errorText);
                }
                logger.LogDebug("Exception details saved to: " + path);

                path = Path.Combine(subfolderPath, options.LogFileName);

                var log = MemoryLogger.LogList;
                File.WriteAllLines(path, log);
                logger.LogDebug("Exception log saved to: " + path);

                if (senderTask.IsStarted)
                {
                    senderTask.TryRunImmediately();
                    logger.LogDebug("ExceptionSenderTask called");
                }
                else
                {
                    logger.LogWarning("ExceptionSenderTask nostarted - mail with exception data will not be sent");
                }

                throw;
            }
        }
    }
}
