﻿namespace Logging.ExceptionSender
{
    using System;
    using System.IO;
    using System.Net;
    using System.Net.Http;

    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;

    using RecurrentTasks;
    
    public class ExceptionSenderMailgunTask : ExceptionSenderTask
    {
        private ExceptionSenderMailgunOptions options;

        public ExceptionSenderMailgunTask(ILoggerFactory loggerFactory, IServiceScopeFactory serviceScopeFactory, IOptions<ExceptionSenderMailgunOptions> options)
            : base(loggerFactory, serviceScopeFactory, options)
        {
            this.options = options.Value;
        }

        protected override void Send(IServiceProvider serviceProvider, TaskRunStatus runStatus, string text, FileInfo logFile)
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
