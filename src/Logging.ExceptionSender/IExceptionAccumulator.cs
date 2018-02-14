namespace Logging.ExceptionSender
{
    using System;
    using System.Threading.Tasks;

    public interface IExceptionAccumulator
    {
        [Obsolete("Use SaveExceptionAsync(ex)")]
        void SaveException(Exception ex);

        Task SaveExceptionAsync(Exception ex);
    }
}
