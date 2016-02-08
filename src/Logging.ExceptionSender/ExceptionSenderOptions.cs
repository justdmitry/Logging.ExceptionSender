namespace Logging.ExceptionSender
{
    using System.Net.Mail;

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
        /// File name log records before exception
        /// </summary>
        /// <remarks>
        /// Default: <value>log.txt</value>
        /// </remarks>
        public string LogFileName { get; set; } = "log.txt";

        /// <summary>
        /// Domain for sending reports via MailGun.com
        /// </summary>
        public string MailgunDomain { get; set; }

        /// <summary>
        /// ApiKey for sending reports via MailGun.com
        /// </summary>
        public string MailgunApiKey { get; set; }

        /// <summary>
        /// Base url for sending reports via MailGun.com
        /// </summary>
        /// <remarks>
        /// Already set to <value>https://api.mailgun.net/v3/</value>, no need to change.
        /// </remarks>
        public string MailgunBaseUrl { get; set; } = "https://api.mailgun.net/v3/";

        /// <summary>
        /// Sender email ('from' in messages)
        /// </summary>
        public MailAddress From { get; set; }

        /// <summary>
        /// Target (admin) email(s) ('to' in messages)
        /// </summary>
        public MailAddress[] To { get; set; }
    }
}
