namespace Logging.ExceptionSender
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;

    using RecurrentTasks;

    public class ExceptionSenderMailgunTask : ExceptionSenderTask
    {
        private readonly ExceptionSenderMailgunOptions options;

        private readonly HttpClient httpClient;

        public ExceptionSenderMailgunTask(
            ILogger<ExceptionSenderMailgunTask> logger,
            IOptions<ExceptionSenderMailgunOptions> options,
            IHostEnvironment hostEnvironment,
            HttpClient httpClient)
            : base(logger, options?.Value, hostEnvironment)
        {
            this.options = options.Value;
            this.httpClient = httpClient;
        }

        protected override async Task SendAsync(ITask currentTask, string text, FileInfo logFile)
        {
            if (string.IsNullOrEmpty(text))
            {
                throw new ArgumentNullException(nameof(text));
            }

            var toDispose = new List<IDisposable>();

            var addDisposableContent = new Func<HttpContent, HttpContent>(c =>
            {
                toDispose.Add(c);
                return c;
            });

            var mailSubject = text[..Math.Min(256, text.IndexOf("\r\n", StringComparison.Ordinal))];

            using var form = new MultipartFormDataContent();
            foreach (var to in options.To)
            {
                form.Add(addDisposableContent(new StringContent(to)), "to");
            }
            form.Add(addDisposableContent(new StringContent(options.From)), "from");
            form.Add(addDisposableContent(new StringContent(mailSubject)), "subject");
            form.Add(addDisposableContent(new StringContent(text)), "text");

            if (logFile != null && logFile.Exists)
            {
                form.Add(addDisposableContent(new StreamContent(logFile.OpenRead())), "attachment", logFile.Name);
            }

            try
            {
#pragma warning disable CA2234 // Pass system uri objects instead of strings // Let HttpClient to build full url
                var response = await httpClient.PostAsync("messages", form).ConfigureAwait(false);
#pragma warning restore CA2234 // Pass system uri objects instead of strings
                response.EnsureSuccessStatusCode();
            }
            finally
            {
                foreach (var d in toDispose)
                {
                    d.Dispose();
                }
            }
        }
    }
}

