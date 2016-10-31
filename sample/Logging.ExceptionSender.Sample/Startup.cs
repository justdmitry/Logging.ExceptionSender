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
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables();

            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; set; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<ExceptionSenderOptions>(Configuration.GetSection("ExceptionSender"));
            services.Configure<ExceptionSenderMailgunOptions>(Configuration.GetSection("ExceptionSender"));
            services.AddExceptionSender<ExceptionSenderMailgunTask>();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole();
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
