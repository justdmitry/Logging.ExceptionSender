namespace Microsoft.Extensions.DependencyInjection
{
    using System;

    using global::Logging.ExceptionSender;

    public static class ExceptionSenderServiceCollectionExtensions
    {
        public static IServiceCollection AddExceptionSender<TExceptionSender>(this IServiceCollection services)
            where TExceptionSender : ExceptionSenderTask
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services.AddSingleton<IExceptionAccumulator, ExceptionAccumulator>();
            services.AddTransient<ExceptionSenderTask, TExceptionSender>();
            services.AddTask<ExceptionSenderTask>();

            return services;
        }
    }
}
