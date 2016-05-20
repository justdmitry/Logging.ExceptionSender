namespace Microsoft.AspNetCore.Builder
{
    using System;
    using Logging.ExceptionSender;

    public static class ExceptionSenderExtensions
    {
        public static IApplicationBuilder UseExceptionSender(this IApplicationBuilder app)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            return app.UseMiddleware<ExceptionSenderMiddleware>();
        }
    }
}