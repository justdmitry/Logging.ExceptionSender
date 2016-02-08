namespace Logging.ExceptionSender
{
    using System;
    using System.IO;
    using Microsoft.Extensions.PlatformAbstractions;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.OptionsModel;
    using RestSharp;
    using RecurrentTasks;

    public class ExceptionSenderTask : TaskBase<TaskRunStatus>
    {
        private static readonly TimeSpan ShortInterval = TimeSpan.FromMinutes(1);

        private static readonly TimeSpan LongInterval = TimeSpan.FromHours(1);

        private ExceptionSenderOptions options;

        public ExceptionSenderTask(ILoggerFactory loggerFactory, IServiceScopeFactory serviceScopeFactory, IOptions<ExceptionSenderOptions> options)
            : base(loggerFactory, ShortInterval, serviceScopeFactory)
        {
            this.options = options.Value;
        }

        protected override void Run(IServiceProvider serviceProvider, TaskRunStatus runStatis)
        {
            var appEnv = serviceProvider.GetService<IApplicationEnvironment>();

            var baseDir = Path.Combine(appEnv.ApplicationBasePath, options.FolderName);

            if (!Directory.Exists(baseDir))
            {
                Interval = LongInterval;
                Logger.LogVerbose("Папка с логами ошибок не существует, нечего отправлять: " + baseDir);
                return;
            }

            var exceptions = Directory.GetDirectories(baseDir, options.SubfolderPrefix + "*", SearchOption.TopDirectoryOnly);

            if (exceptions.Length == 0)
            {
                Interval = LongInterval;
                Logger.LogVerbose("Папка с логами ошибок пуста, нечего отправлять: " + baseDir);
                return;
            }

            // есть что отправлять - значит 
            Interval = ShortInterval;

            // создаем отправителя
            var restClient = new RestClient();
            restClient.BaseUrl = new Uri(options.MailgunBaseUrl + options.MailgunDomain);
            restClient.Authenticator = new HttpBasicAuthenticator("api", options.MailgunApiKey);

            var count = 0;
            foreach (string exception in exceptions)
            {
                string mailSubject = "";
                string mailBody = "";
                string exceptionFilePath = Path.Combine(exception, options.ExceptionFileName);
                if (File.Exists(exceptionFilePath))
                {
                    string exceptionStrings = File.ReadAllText(exceptionFilePath);
                    mailSubject = exceptionStrings.Substring(0, exceptionStrings.IndexOf("\r\n"));
                    mailBody = exceptionStrings;

                    RestRequest request = new RestRequest();
                    request.Resource = "messages";
                    request.AddParameter("from", options.From.ToString());
                    foreach (var to in options.To)
                    {
                        request.AddParameter("to", to.ToString());
                    }
                    request.AddParameter("subject", mailSubject);
                    request.AddParameter("text", mailBody);

                    string logFilePath = Path.Combine(exception, options.LogFileName);
                    if (File.Exists(logFilePath))
                    {
                        byte[] log = File.ReadAllBytes(logFilePath);
                        request.AddFile("attachment", log, options.LogFileName);
                    }
                    request.Method = Method.POST;
                    var response = restClient.Execute<MailgunResponse>(request);

                    if (response.ResponseStatus != ResponseStatus.Completed)
                    {
                        throw new ApplicationException($"Не удалось отправить письмо. Ответ сервера {response.ResponseStatus}, текст: \n {response.Content}");
                    }
                    else
                    {
                        Directory.Delete(exception, true);
                        count++;
                    }
                }
            }
            Logger.LogVerbose($"Успешно отправлено {count} писем с логами ошибок.");
        }

        private class MailgunResponse
        {
            public string Message { get; set; }

            public string Id { get; set; }
        }
    }
}

