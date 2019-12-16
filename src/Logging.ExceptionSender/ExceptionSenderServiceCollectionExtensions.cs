namespace Microsoft.Extensions.DependencyInjection
{
    using System;
    using System.Net;
    using System.Net.Http;
    using global::Logging.ExceptionSender;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Options;

    public static class ExceptionSenderServiceCollectionExtensions
    {
        private static IServiceCollection AddExceptionSender<TSenderTask>(this IServiceCollection services, IConfigurationSection config)
            where TSenderTask : ExceptionSenderTask
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            services.AddSingleton<IExceptionAccumulator, ExceptionAccumulator>();

            services.Configure<ExceptionSenderOptions>(config);

            services.AddTransient<ExceptionSenderTask, TSenderTask>();

            services.AddTask<ExceptionSenderTask>(o => o.AutoStart(TimeSpan.FromHours(1)));

            return services;
        }

        public static IServiceCollection AddMailgunExceptionSender(this IServiceCollection services, IConfigurationSection config)
        {
            AddExceptionSender<ExceptionSenderMailgunTask>(services, config);

            services.Configure<ExceptionSenderMailgunOptions>(config);

            services
                .AddHttpClient<ExceptionSenderTask, ExceptionSenderMailgunTask>(
                (sp, c) =>
                {
                    var options = sp.GetRequiredService<IOptions<ExceptionSenderMailgunOptions>>().Value;
                    c.BaseAddress = new Uri(options.MailgunBaseUrl, options.MailgunDomain + "/");
                })
                .ConfigurePrimaryHttpMessageHandler(sp => {
                    var options = sp.GetRequiredService<IOptions<ExceptionSenderMailgunOptions>>().Value;
                    return new HttpClientHandler { Credentials = new NetworkCredential("api", options.MailgunApiKey) };
                });

            return services;
        }
    }
}
