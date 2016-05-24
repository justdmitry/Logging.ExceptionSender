namespace Logging.ExceptionSender
{
    using System;
    using System.Threading.Tasks;

    using Microsoft.AspNetCore.Http;

    public class ExceptionSenderMiddleware
    {
        private readonly RequestDelegate nextMiddleware;
        private readonly ExceptionSenderTask senderTask;

        public ExceptionSenderMiddleware(RequestDelegate next, ExceptionSenderTask senderTask)
        {
            nextMiddleware = next;
            this.senderTask = senderTask;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await nextMiddleware(context);
            }
            catch (Exception ex)
            {
                senderTask.LogException(ex);
                throw;
            }
        }
    }
}
