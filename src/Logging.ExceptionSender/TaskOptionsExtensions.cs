namespace Logging.ExceptionSender
{
    using System;
    using Microsoft.Extensions.DependencyInjection;
    using RecurrentTasks;

    public static class TaskOptionsExtensions
    {
        public static TaskOptions WithExceptionSender(this TaskOptions taskOptions)
        {
            if (taskOptions == null)
            {
                throw new ArgumentNullException(nameof(taskOptions));
            }

            taskOptions.AfterRunFail += (sp, task, ex) =>
            {
                var exceptionAccumulator = sp.GetRequiredService<IExceptionAccumulator>();
                return exceptionAccumulator.SaveExceptionAsync(ex);
            };

            return taskOptions;
        }
    }
}
