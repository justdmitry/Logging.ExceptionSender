namespace Logging.ExceptionSender
{
    using System;

    public class ExceptionSenderMailgunOptions : ExceptionSenderOptions
    {
        /// <summary>
        /// Domain for sending reports via MailGun.com
        /// </summary>
        public required string MailgunDomain { get; set; }

        /// <summary>
        /// ApiKey for sending reports via MailGun.com
        /// </summary>
        public required string MailgunApiKey { get; set; }

        /// <summary>
        /// Base url for sending reports via MailGun.com
        /// </summary>
        /// <remarks>
        /// Already set to <value>https://api.mailgun.net/v3/</value>, no need to change.
        /// </remarks>
        public required Uri MailgunBaseUrl { get; set; } = new("https://api.mailgun.net/v3/");

        /// <summary>
        /// Sender email ('from' in messages)
        /// </summary>
        public required string From { get; set; }

        /// <summary>
        /// Target (admin) email(s) ('to' in messages)
        /// </summary>
        public required string[] To { get; set; }
    }
}
