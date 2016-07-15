namespace RecurrentTasks
{
    using System;

    using Logging.ExceptionSender;

    public static class RecurrentTasksExtensions
    {
        public static T CatchExceptions<T>(this T task, ExceptionSenderTask exceptionSender) where T : ITask
        {
            task.AfterRunFail += (sender, e) => exceptionSender.LogException(e.Exception);
            return task;
        }
    }
}
