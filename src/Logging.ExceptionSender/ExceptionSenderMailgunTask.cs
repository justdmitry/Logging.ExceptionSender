namespace Logging.ExceptionSender
{
    using System;
    using System.IO;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;

    using RecurrentTasks;

    public class ExceptionSenderMailgunTask(
        ILogger<ExceptionSenderMailgunTask> logger,
        IOptions<ExceptionSenderMailgunOptions> options,
        IHostEnvironment hostEnvironment,
        HttpClient httpClient)
        : ExceptionSenderTask(logger, options.Value, hostEnvironment)
    {
        protected override async Task SendAsync(ITask currentTask, string text, FileInfo logFile)
        {
            ArgumentException.ThrowIfNullOrEmpty(text);

            var mailSubject = text[..Math.Min(256, text.IndexOf("\r\n", StringComparison.Ordinal))];

            using var form = new MultipartFormDataContent();
            foreach (var to in options.Value.To)
            {
                form.Add(new StringContent(to), "to");
            }
            form.Add(new StringContent(options.Value.From), "from");
            form.Add(new StringContent(mailSubject), "subject");
            form.Add(new StringContent(text), "text");

            if (logFile != null && logFile.Exists)
            {
                form.Add(new StreamContent(logFile.OpenRead()), "attachment", logFile.Name);
            }

            using var response = await httpClient.PostAsync("messages", form).ConfigureAwait(false);

            var responseText = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                logger.LogError("Non-successful response: {Text}", responseText);
            }

            response.EnsureSuccessStatusCode();
        }
    }
}

