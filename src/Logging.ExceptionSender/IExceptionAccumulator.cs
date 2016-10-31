namespace Logging.ExceptionSender
{
    using System;

    public interface IExceptionAccumulator
    {
        void SaveException(Exception ex);
    }
}
