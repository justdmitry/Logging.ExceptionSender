using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RecurrentTasks;

namespace Logging.ExceptionSender
{
    public class ExceptionSenderTelegramTask : ExceptionSenderTask
    {
        private readonly ILogger logger;
        private readonly ExceptionSenderTelegramOptions options;
        private readonly HttpClient httpClient;
        private readonly string appName;

        public ExceptionSenderTelegramTask(
            ILogger<ExceptionSenderTelegramTask> logger,
            IOptions<ExceptionSenderTelegramOptions> options,
            IHostEnvironment hostEnvironment,
            HttpClient httpClient)
            : base(logger, options.Value, hostEnvironment)
        {
            this.logger = logger;
            this.options = options.Value;
            this.httpClient = httpClient;

            var appAssembly = Assembly.GetEntryAssembly();
            appName = appAssembly.GetCustomAttribute<AssemblyTitleAttribute>()?.Title ?? appAssembly.GetName().Name ?? "Unknown :(";
        }

        protected override async Task SendAsync(ITask currentTask, string text, FileInfo logFile)
        {
            if (string.IsNullOrEmpty(text))
            {
                throw new ArgumentNullException(nameof(text));
            }

            if (string.IsNullOrEmpty(options.ChatId))
            {
                throw new InvalidOperationException("ChatId is empty");
            }

            if (string.IsNullOrEmpty(options.BotToken))
            {
                throw new InvalidOperationException("BotToken is empty");
            }

            var toDispose = new List<IDisposable>();

            var addDisposableContent = new Func<HttpContent, HttpContent>(c =>
            {
                toDispose.Add(c);
                return c;
            });

            var caption = appName + "\r\n" + text[..text.IndexOf("\r\n", StringComparison.Ordinal)];

            using var form = new MultipartFormDataContent();
            form.Add(addDisposableContent(new StringContent(options.ChatId)), "chat_id");
            form.Add(addDisposableContent(new StringContent(caption)), "caption");
            if (options.MessageThreadId.HasValue)
            {
                form.Add(addDisposableContent(new StringContent(options.MessageThreadId.Value.ToString(System.Globalization.CultureInfo.InvariantCulture))), "message_thread_id");
            }

            if (logFile != null && logFile.Exists)
            {
                form.Add(addDisposableContent(new StreamContent(logFile.OpenRead())), "document", logFile.Name);
            }

            var uri = new Uri($"https://api.telegram.org/bot{options.BotToken}/sendDocument");

            try
            {
                using var response = await httpClient.PostAsync(uri, form).ConfigureAwait(false);
                var responseText = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                if (!response.IsSuccessStatusCode)
                {
                    logger.LogError("Non-successful response: {Text}", responseText);
                }

                // And throw
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
