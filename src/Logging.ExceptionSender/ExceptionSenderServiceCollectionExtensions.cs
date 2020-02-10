namespace Microsoft.Extensions.DependencyInjection
{
    using System;
    using System.Net;
    using System.Net.Http;
    using global::Logging.ExceptionSender;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection.Extensions;
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

            services.TryAddSingleton<IExceptionAccumulator, ExceptionAccumulator>();

            services.Configure<ExceptionSenderOptions>(config);

            services.TryAddTransient<ExceptionSenderTask, TSenderTask>();

            services.AddTask<ExceptionSenderTask>(o => o.AutoStart(TimeSpan.FromHours(1)));

            return services;
        }

        public static IServiceCollection AddMailgunExceptionSender(this IServiceCollection services, IConfigurationSection config)
        {
            services.Configure<ExceptionSenderMailgunOptions>(config);

            var options = config.Get<ExceptionSenderMailgunOptions>();
            var baseAddress = new Uri(options.MailgunBaseUrl, options.MailgunDomain + "/");

            services
                .AddHttpClient<ExceptionSenderTask, ExceptionSenderMailgunTask>(c => c.BaseAddress = baseAddress)
                .ConfigurePrimaryHttpMessageHandler(sp => {
                    var options = sp.GetRequiredService<IOptions<ExceptionSenderMailgunOptions>>().Value;
                    return new HttpClientHandler { Credentials = new NetworkCredential("api", options.MailgunApiKey) };
                });

            return AddExceptionSender<ExceptionSenderMailgunTask>(services, config);
        }
    }
}
