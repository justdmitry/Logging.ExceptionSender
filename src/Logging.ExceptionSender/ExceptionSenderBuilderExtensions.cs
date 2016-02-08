namespace Microsoft.AspNet.Builder
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

        public static IApplicationBuilder UseExceptionSender(this IApplicationBuilder app, Action<ExceptionSenderOptions> configureSender)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }
            if (configureSender == null)
            {
                throw new ArgumentNullException(nameof(configureSender));
            }

            var options = new ExceptionSenderOptions();
            configureSender(options);

            return app.UseMiddleware<ExceptionSenderMiddleware>(options);
        }

        public static IApplicationBuilder UseExceptionSender(this IApplicationBuilder app, ExceptionSenderOptions options)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            return app.UseMiddleware<ExceptionSenderMiddleware>(new object[] { options });
        }

    }
}