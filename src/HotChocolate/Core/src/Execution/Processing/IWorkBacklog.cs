using System;
using System.Threading.Tasks;

namespace HotChocolate.Execution.Processing
{
    /// <summary>
    /// The task backlog of the execution engine stores <see cref="IExecutionTask"/>
    /// without any guaranteed order.
    /// </summary>
    internal interface IWorkBacklog
    {
        /// <summary>
        /// Signals that the work queue is filling up more quickly than work is dequeued.
        /// </summary>
        event EventHandler<EventArgs>? BackPressureLimitExceeded;

        /// <summary>
        /// A task that can be awaited to wait for the completion of the current work backlog.
        /// </summary>
        Task Completion { get; }

        /// <summary>
        /// Defines if the backlog is empty and has no more tasks.
        /// </summary>
        bool IsEmpty { get; }

        /// <summary>
        /// Defines if the backlog has running tasks.
        /// </summary>
        bool HasRunningTasks { get; }

        /// <summary>
        /// Tries to get some work from the backlog.
        /// </summary>
        /// <param name="buffer">
        /// The task buffer that shall be filled with work.
        /// </param>
        /// <param name="main">
        /// Defines if the main processor is asking for work.
        /// </param>
        /// <returns>
        /// Returns the amount of work that was put into the buffer.
        /// </returns>
        int TryTake(IExecutionTask?[] buffer);

        /// <summary>
        /// Registers work with the task backlog.
        /// </summary>
        void Register(IExecutionTask task);

        /// <summary>
        /// Registers work with the task backlog.
        /// </summary>
        void Register(IExecutionTask?[] tasks, int length);

        /// <summary>
        /// Complete a task
        /// </summary>
        void Complete(IExecutionTask task);

        /// <summary>
        /// Signal that a processor wants to complete.
        /// Depending on the state the <see cref="IWorkBacklog"/> might deny closing the processor.
        /// </summary>
        /// <returns>
        /// Returns a boolean indicating if a processor can close.
        /// </returns>
        Task<bool> TryCompleteProcessor();
    }
}
