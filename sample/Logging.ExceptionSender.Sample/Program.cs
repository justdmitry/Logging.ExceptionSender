namespace Logging.ExceptionSender.Sample
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    public static class Program
    {
        public static async Task Main()
        {
            var builder = WebApplication.CreateBuilder();

            // Step 1: Configure required sender (and fill parameters in appsettings file).
            //// builder.Services.AddMailgunExceptionSender(builder.Configuration.GetSection("ExceptionSender"));
            builder.Services.AddTelegramExceptionSender(builder.Configuration.GetSection("ExceptionSender"));

            // Step 2a: Append .WithExceptionSender() to task configuration.
            builder.Services.AddTask<FailingTask>(o => o.AutoStart(TimeSpan.FromSeconds(60), TimeSpan.FromSeconds(10)).WithExceptionSender());

            var app = builder.Build();

            app.UseDeveloperExceptionPage();

            // Step 2b: Append .UseExceptionSender() to aspnet request pipeline.
            app.UseExceptionSender();

            // Step 3: Enable MemoryLogger with desired log level.
            var loggerFactory = app.Services.GetRequiredService<ILoggerFactory>();
            loggerFactory.AddMemory(LogLevel.Debug);

            app.Run(async (context) =>
            {
                if (context.Request.Path == "/throw")
                {
                    throw new Exception("This is a test exception in ASP.NET pipeline.");
                }

                await context.Response.WriteAsync("<h1>Hello World!</h1> <p>Go to <a href='/throw'>/throw page</a> to see exception sender in action for web projects, and/or wait ~15sec to see failing background task action (not displayed on page, check 'log' folder).</p>");
            });

            await app.RunAsync();
        }
    }
}
