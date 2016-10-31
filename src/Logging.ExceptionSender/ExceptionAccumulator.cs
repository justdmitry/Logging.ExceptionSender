﻿namespace Logging.ExceptionSender
{
    using System;
    using System.IO;

    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;

    using RecurrentTasks;

    public class ExceptionAccumulator : IExceptionAccumulator
    {
        private ILogger logger;

        private ExceptionSenderOptions options;

        private string contentRootPath;

        private ITask exceptionSenderTask;

        public ExceptionAccumulator(
            ILogger<ExceptionAccumulator> logger, 
            IOptions<ExceptionSenderOptions> options, 
            IHostingEnvironment hostingEnvironment,
            ITask<ExceptionSenderTask> exceptionSenderTask) 
        {
            this.logger = logger;
            this.options = options.Value;
            this.contentRootPath = hostingEnvironment.ContentRootPath;
            this.exceptionSenderTask = exceptionSenderTask;
        }

        /// <summary>
        /// Append information about exception to send queue. Real send will perform later, in separate thread/task.
        /// </summary>
        public void SaveException(Exception ex)
        {
            var baseDir = contentRootPath;

            var subfolderName = string.Format("{0}{1}", options.SubfolderPrefix, DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss-fff"));
            var subfolderPath = Path.Combine(baseDir, options.FolderName, subfolderName);

            Directory.CreateDirectory(subfolderPath);

            var path = Path.Combine(subfolderPath, options.ExceptionFileName);

            var errorText = string.Format("{2}: {0}\r\n\r\n{1}", ex.Message, ex.ToString(), ex.GetType().FullName);
            File.WriteAllText(path, errorText);
            logger.LogInformation("Exception details saved to: {0}", path);

            path = Path.Combine(subfolderPath, options.LogFileName);

            var log = iflight.Logging.MemoryLogger.LogList;
            File.WriteAllLines(path, log);
            logger.LogInformation("Exception log saved to: {0}", path);

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