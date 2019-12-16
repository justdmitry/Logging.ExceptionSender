namespace Logging.ExceptionSender
{
    using System;
    using System.IO;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
#if NETCOREAPP2_1
    using Microsoft.AspNetCore.Hosting;
#else
    using Microsoft.Extensions.Hosting;
#endif
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;

    using RecurrentTasks;

    public class ExceptionSenderMailgunTask : ExceptionSenderTask
    {
        private ExceptionSenderMailgunOptions options;

        public ExceptionSenderMailgunTask(
            ILogger<ExceptionSenderMailgunTask> logger,
            IOptions<ExceptionSenderMailgunOptions> options,
#if NETCOREAPP2_1
            IHostingEnvironment hostEnvironment
#else
            IHostEnvironment hostEnvironment
#endif
            )
            : base(logger, options.Value, hostEnvironment)
        {
            this.options = options.Value;
        }

        protected override async Task SendAsync(ITask currentTask, string text, FileInfo logFile)
        {
            var mailSubject = text.Substring(0, text.IndexOf("\r\n"));

            using (var form = new MultipartFormDataContent())
            {
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
                    var response = await httpClient.PostAsync("messages", form);

                    response.EnsureSuccessStatusCode();
                }
            }
        }
    }
}

