namespace Logging.ExceptionSender.Sample
{
    using System;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            this.Configuration = configuration;
        }

        public IConfiguration Configuration { get; set; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddTelegramExceptionSender(Configuration.GetSection("ExceptionSender"));
        }

        public void Configure(IApplicationBuilder app, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddMemory();

            app.UseDeveloperExceptionPage();

            app.UseExceptionSender();

            app.Run(async (context) =>
            {
                if (context.Request.Path == "/throw")
                {
                    throw new Exception("Test exception");
                }

                await context.Response.WriteAsync("Hello World! Request for /throw page to see exception sender in action.");
            });
        }
    }
}
