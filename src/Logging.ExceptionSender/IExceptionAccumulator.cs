namespace Logging.ExceptionSender
{
    using System;
    using System.Threading.Tasks;

    public interface IExceptionAccumulator
    {
        Task SaveExceptionAsync(Exception ex);
    }
}
