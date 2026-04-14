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
            ArgumentNullException.ThrowIfNull(services);
            ArgumentNullException.ThrowIfNull(config);

            services.TryAddSingleton<IExceptionAccumulator, ExceptionAccumulator>();

            services.Configure<ExceptionSenderOptions>(config);

            services.TryAddTransient<ExceptionSenderTask, TSenderTask>();

            services.AddTask<ExceptionSenderTask>(o => o.AutoStart(TimeSpan.FromHours(1)));

            return services;
        }

        public static IServiceCollection AddMailgunExceptionSender(this IServiceCollection services, IConfigurationSection config, Action<IHttpClientBuilder>? additionalConfiguration = default)
        {
            services.Configure<ExceptionSenderMailgunOptions>(config);

            var options = config.Get<ExceptionSenderMailgunOptions>();
            var baseAddress = new Uri(options.MailgunBaseUrl, options.MailgunDomain + "/");

            var builder = services
                .AddHttpClient<ExceptionSenderTask, ExceptionSenderMailgunTask>(c => c.BaseAddress = baseAddress)
                .ConfigurePrimaryHttpMessageHandler(sp =>
                {
                    var options = sp.GetRequiredService<IOptions<ExceptionSenderMailgunOptions>>().Value;
                    return new HttpClientHandler { Credentials = new NetworkCredential("api", options.MailgunApiKey) };
                });
            additionalConfiguration?.Invoke(builder);

            return AddExceptionSender<ExceptionSenderMailgunTask>(services, config);
        }

        public static IServiceCollection AddTelegramExceptionSender(this IServiceCollection services, IConfigurationSection config, Action<IHttpClientBuilder>? additionalConfiguration = default)
        {
            services.Configure<ExceptionSenderTelegramOptions>(config);

            var builder = services.AddHttpClient<ExceptionSenderTask, ExceptionSenderTelegramTask>();
            additionalConfiguration?.Invoke(builder);

            return AddExceptionSender<ExceptionSenderTelegramTask>(services, config);
        }
    }
}
