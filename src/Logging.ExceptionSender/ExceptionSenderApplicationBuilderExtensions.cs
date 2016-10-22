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

            app.StartTask<ExceptionSenderTask>(TimeSpan.FromHours(1));

            return app;
        }

        public static void StartTaskWithExceptionSender<TRunnable>(this IApplicationBuilder app, TimeSpan interval)
            where TRunnable : IRunnable
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            var exceptionAccumulator = app.ApplicationServices.GetRequiredService<IExceptionAccumulator>();

            app.StartTask<TRunnable>(t => 
                {
                    t.Interval = interval;
                    t.AfterRunFail += (sender, e) => { exceptionAccumulator.SaveException(e.Exception); };
                });
        }
    }
}