namespace Logging.ExceptionSender
{
    public class ExceptionSenderOptions
    {
        /// <summary>
        /// Folder, where exception logs are stored
        /// </summary>
        /// <remarks>
        /// Default: <value>log</value>
        /// </remarks>
        public string FolderName { get; set; } = "log";

        /// <summary>
        /// Prefix for each separate exception subfolder
        /// </summary>
        /// <remarks>
        /// Default: <value>exception_</value>
        /// </remarks>
        public string SubfolderPrefix { get; set; } = "exception_";

        /// <summary>
        /// File name for exception stacktrace
        /// </summary>
        /// <remarks>
        /// Default: <value>exception.txt</value>
        /// </remarks>
        public string ExceptionFileName { get; set; } = "exception.txt";

        /// <summary>
        /// File name for log records (before exception)
        /// </summary>
        /// <remarks>
        /// Default: <value>log.txt</value>
        /// </remarks>
        public string LogFileName { get; set; } = "log.txt";

        /// <summary>
        /// Maximum size (in bytes) for log file (before exception). Set to 0 (or negative) to disable size limit.
        /// </summary>
        /// <remarks>
        /// Default: <value>10000000</value> (approx. 10 MB)
        /// </remarks>
        public int LogFileMaxSize { get; set; } = 10_000_000;
    }
}
