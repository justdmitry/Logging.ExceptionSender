namespace Logging.ExceptionSender
{
    using System;
    using System.Threading.Tasks;

    using Microsoft.AspNetCore.Http;

    public class ExceptionSenderMiddleware(RequestDelegate next, IExceptionAccumulator exceptionAccumulator)
    {
        public async Task Invoke(HttpContext context)
        {
            try
            {
                await next(context).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                await exceptionAccumulator.SaveException(ex).ConfigureAwait(false);
                throw;
            }
        }
    }
}
