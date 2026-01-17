namespace Logging.ExceptionSender
{
    using System;
    using Microsoft.Extensions.DependencyInjection;
    using RecurrentTasks;

    public static class TaskOptionsExtensions
    {
        public static TaskOptions WithExceptionSender(this TaskOptions taskOptions)
        {
            ArgumentNullException.ThrowIfNull(taskOptions);

            taskOptions.AfterRunFail += (sp, task, ex) =>
            {
                var exceptionAccumulator = sp.GetRequiredService<IExceptionAccumulator>();
                return exceptionAccumulator.SaveException(ex);
            };

            return taskOptions;
        }
    }
}
