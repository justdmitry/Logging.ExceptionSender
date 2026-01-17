namespace Logging.ExceptionSender.Sample
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using RecurrentTasks;

    public class FailingTask : IRunnable
    {
        public Task RunAsync(ITask currentTask, IServiceProvider scopeServiceProvider, CancellationToken cancellationToken)
        {
            throw new SampleException();
        }

        public class SampleException() : Exception("This is sample Task exception");
    }
}
