namespace Microsoft.AspNetCore.Builder
{
    using System;
    using Logging.ExceptionSender;
    using Microsoft.Extensions.DependencyInjection;
    using RecurrentTasks;

    public static class ExceptionSenderApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseExceptionSender(this IApplicationBuilder app)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            app.UseMiddleware<ExceptionSenderMiddleware>();

            return app;
        }

        public static TaskOptions WithExceptionSender(this TaskOptions taskOptions)
        {
            if (taskOptions == null)
            {
                throw new ArgumentNullException(nameof(taskOptions));
            }

            taskOptions.AfterRunFail += (sp, task, ex) =>
            {
                var exceptionAccumulator = sp.GetRequiredService<IExceptionAccumulator>();
                return exceptionAccumulator.SaveExceptionAsync(ex);
            };

            return taskOptions;
        }
    }
}