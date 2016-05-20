namespace Logging.ExceptionSender
{
    public class ExceptionSenderMailgunOptions : ExceptionSenderOptions
    {
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
        public string From { get; set; }

        /// <summary>
        /// Target (admin) email(s) ('to' in messages)
        /// </summary>
        public string[] To { get; set; }
    }
}
