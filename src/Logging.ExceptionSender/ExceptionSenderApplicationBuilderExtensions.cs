namespace Microsoft.AspNetCore.Builder
{
    using System;
    using Logging.ExceptionSender;

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
    }
}