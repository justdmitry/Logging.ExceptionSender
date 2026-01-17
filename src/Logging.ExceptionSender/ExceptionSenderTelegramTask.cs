namespace Logging.ExceptionSender
{
    using System;
    using System.IO;
    using System.Net.Http;
    using System.Reflection;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using RecurrentTasks;

    public class ExceptionSenderTelegramTask(
        ILogger<ExceptionSenderTelegramTask> logger,
        IOptions<ExceptionSenderTelegramOptions> options,
        IHostEnvironment hostEnvironment,
        HttpClient httpClient)
        : ExceptionSenderTask(logger, options.Value, hostEnvironment)
    {
        private readonly ExceptionSenderTelegramOptions options = options.Value;
        private string? appName;

        protected override async Task SendAsync(ITask currentTask, string text, FileInfo logFile)
        {
            ArgumentException.ThrowIfNullOrEmpty(text);
            ArgumentException.ThrowIfNullOrEmpty(options.ChatId);
            ArgumentException.ThrowIfNullOrEmpty(options.BotToken);

            if (appName == null)
            {
                var appAssembly = Assembly.GetEntryAssembly();
                appName = appAssembly?.GetCustomAttribute<AssemblyTitleAttribute>()?.Title ?? appAssembly?.GetName().Name ?? "Unnamed";
                appName += options.AppNameSuffix;
            }

            var caption = appName + "\r\n" + text[..Math.Min(2048, text.IndexOf("\r\n", StringComparison.Ordinal))];

            using var form = new MultipartFormDataContent();
            form.Add(new StringContent(options.ChatId), "chat_id");
            form.Add(new StringContent(caption), "caption");
            if (options.MessageThreadId.HasValue)
            {
                form.Add(new StringContent(options.MessageThreadId.Value.ToString(System.Globalization.CultureInfo.InvariantCulture)), "message_thread_id");
            }

            if (logFile != null && logFile.Exists)
            {
                form.Add(new StreamContent(logFile.OpenRead()), "document", logFile.Name);
            }

            var uri = new Uri($"https://api.telegram.org/bot{options.BotToken}/sendDocument");

            using var response = await httpClient.PostAsync(uri, form).ConfigureAwait(false);

            var responseText = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                logger.LogError("Non-successful response: {Text}", responseText);
            }

            response.EnsureSuccessStatusCode();
        }
    }
}
