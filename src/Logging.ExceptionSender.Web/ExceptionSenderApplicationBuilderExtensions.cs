namespace Microsoft.AspNetCore.Builder
{
    using System;
    using Logging.ExceptionSender;

    public static class ExceptionSenderApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseExceptionSender(this IApplicationBuilder app)
        {
            ArgumentNullException.ThrowIfNull(app);

            app.UseMiddleware<ExceptionSenderMiddleware>();

            return app;
        }
    }
}