namespace Logging.ExceptionSender
{
    using System;
    using System.IO;
    using System.Net;
    using System.Net.Http;

    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;

    using RecurrentTasks;
    
    public class ExceptionSenderMailgunTask : ExceptionSenderTask
    {
        private ExceptionSenderMailgunOptions options;

        public ExceptionSenderMailgunTask(
            ILogger<ExceptionSenderMailgunTask> logger, 
            IOptions<ExceptionSenderMailgunOptions> options, 
            IHostingEnvironment hostingEnvironment)
            : base(logger, options.Value, hostingEnvironment)
        {
            this.options = options.Value;
        }

        protected override void Send(ITask currentTask, string text, FileInfo logFile)
        {
            var mailSubject = text.Substring(0, text.IndexOf("\r\n"));

            var form = new MultipartFormDataContent();
            form.Add(new StringContent(options.From), "from");
            foreach (var to in options.To)
            {
                form.Add(new StringContent(to), "to");
            }
            form.Add(new StringContent(mailSubject), "subject");
            form.Add(new StringContent(text), "text");

            if (logFile != null && logFile.Exists)
            {
                form.Add(new StreamContent(logFile.OpenRead()), "attachment", logFile.Name);
            }

            var credentials = new NetworkCredential("api", options.MailgunApiKey);
            var handler = new HttpClientHandler { Credentials = credentials };
            using (var httpClient = new HttpClient(handler))
            {
                httpClient.BaseAddress = new Uri(options.MailgunBaseUrl + options.MailgunDomain + "/");
                var response = httpClient.PostAsync("messages", form).Result;

                response.EnsureSuccessStatusCode();
            }
        }
    }
}

