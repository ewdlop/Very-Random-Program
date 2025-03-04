using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;

namespace 亂七八糟.Threading
{
    /// <summary>
    /// <inheritdoc/>
    /// <see href="https://claude.ai/share/9d37543d-ab76-4259-9e99-5183d5b8a1c3"/>
    /// <see href="https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.taskscheduler?view=net-9.0"/>
    /// </summary>
    public class PriorityValueTaskScheduler : TaskScheduler, IDisposable, IAsyncDisposable
    {
        protected bool _disposed;
        protected Dictionary<ThreadPriority, SemaphoreSlim> _semaphoreSlims;
        protected readonly Dictionary<ThreadPriority, Channel<(ThreadPriority threadPriority, ValueTask task)>> _taskQueues;
        protected readonly Dictionary<ThreadPriority, ChannelWriter<(ThreadPriority threadPriority, ValueTask task)>> _taskQueueWriters;
        protected readonly Dictionary<ThreadPriority, ChannelReader<(ThreadPriority threadPriority, ValueTask task)>> _taskQueueReaders;
        protected readonly ConcurrentDictionary<Task, (ThreadPriority threadPriority, ValueTask task)> _taskToPriorityMap = new ConcurrentDictionary<Task, (ThreadPriority threadPriority, ValueTask task)>();
        protected readonly Dictionary<ThreadPriority, Thread> _threads = new Dictionary<ThreadPriority, Thread>(threadPriorities.Length);
        protected readonly BoundedChannelOptions _boundedChannelOptions;
        protected static readonly ThreadPriority[] threadPriorities =
        [
            ThreadPriority.Highest,
            ThreadPriority.AboveNormal,
            ThreadPriority.Normal,
            ThreadPriority.BelowNormal,
            ThreadPriority.Lowest,
        ];

        public PriorityValueTaskScheduler(BoundedChannelOptions boundedChannelOptions,
            Dictionary<ThreadPriority, bool> isBackgroundDictionary,
            Dictionary<ThreadPriority, Action<(ThreadPriority, ValueTask)>>? itemDroppedDictionary = null)
        {
            _boundedChannelOptions = boundedChannelOptions;
            _semaphoreSlims = new Dictionary<ThreadPriority, SemaphoreSlim>(threadPriorities.Length);
            _taskQueues = new Dictionary<ThreadPriority, Channel<(ThreadPriority threadPriority, ValueTask task)>>(threadPriorities.Length);
            _taskQueueWriters = new Dictionary<ThreadPriority, ChannelWriter<(ThreadPriority threadPriority, ValueTask task)>>(threadPriorities.Length);
            _taskQueueReaders = new Dictionary<ThreadPriority, ChannelReader<(ThreadPriority threadPriority, ValueTask task)>>(threadPriorities.Length);
            
            foreach (ThreadPriority threadPriority in threadPriorities)
            {
                _semaphoreSlims[threadPriority] = new SemaphoreSlim(1,1);
                _taskQueues[threadPriority] = Channel.CreateBounded<(ThreadPriority threadPriority, ValueTask task)>(_boundedChannelOptions, itemDroppedDictionary?[threadPriority]);
                _taskQueueWriters[threadPriority] = _taskQueues[threadPriority].Writer;
                _taskQueueReaders[threadPriority] = _taskQueues[threadPriority].Reader;
                Thread thread = new Thread(() =>
                {
                    WorkerThreadFunction(threadPriority);
                })
                {
                    Name = $"PriorityValueTaskScheduler-{Enum.GetName(threadPriority)}",
                    IsBackground = isBackgroundDictionary[threadPriority],
                    Priority = threadPriority,
                    //CurrentCulture = Thread.CurrentThread.CurrentCulture,
                };
                _threads.Add(threadPriority, thread);
                thread.Start();
            }
        }

        protected void WorkerThreadFunction(ThreadPriority workerThreadPriority)
        {
            while (!_disposed)
            {
                try
                {
                    if (_taskQueueReaders[workerThreadPriority].TryRead(out (ThreadPriority threadPriority, ValueTask task) workItem))
                    {
                        if (workerThreadPriority != workItem.threadPriority)
                        {
                            //Console.WriteLine($"Thread priority mismatch: {threadPriority} != {workItem.threadPriority}");
                            //move to the correct queue?
                            _taskQueueWriters[workItem.threadPriority].TryWrite(workItem);
                            //swap thread priority?
                            //Thread.CurrentThread.Priority = workItem.threadPriority;
                        }
                        TryExecuteTask(workItem.task.AsTask());
                    }
                    else
                    {
                        //work for other threads?
                        foreach (ThreadPriority priority in threadPriorities.Where(threadPriority => threadPriority < workerThreadPriority))
                        {
                            if (_taskQueueReaders[priority].TryRead(out (ThreadPriority threadPriority, ValueTask task) lowerPriorityWorkItem))
                            {
                                TryExecuteTask(lowerPriorityWorkItem.task.AsTask());
                            }
                        }
                    }
                    

                    //if (_taskQueueReaders[threadPriority].TryRead(out (ThreadPriority threadPriority, ValueTask task) workItem))
                    //{
                    //    if(threadPriority != workItem.threadPriority)
                    //    {
                    //        //Console.WriteLine($"Thread priority mismatch: {threadPriority} != {workItem.threadPriority}");
                    //        //move to the correct queue?
                    //        _taskQueueWriters[workItem.threadPriority].TryWrite(workItem);
                    //        //swap thread priority?
                    //        //Thread.CurrentThread.Priority = workItem.threadPriority;
                    //    }
                    //    TryExecuteTask(workItem.task.AsTask());
                    //}
                    //else
                    //{
                    //    //work for other threads?
                    //    //Thread.Sleep(1);
                    //}
                }
                catch (InvalidOperationException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error in worker thread: {ex.Message}");
                }
            }
        }

        // Schedule a task with a specific priority
        public virtual ValueTask ScheduleAction(Action action, ThreadPriority priority, CancellationToken cancellationToken)
        {
            Task task = new Task(action, cancellationToken);
            ValueTask valueTask = new ValueTask(task);
            QueueValueTask(valueTask, priority);
            return valueTask;
        }

        // Schedule a task with a specific priority
        public virtual ValueTask ScheduleTask(Task task, ThreadPriority priority)
        {
            ValueTask valueTask = new ValueTask(task);
            QueueValueTask(valueTask, priority);
            return valueTask;
        }

        // Schedule a value task with a specific priority
        public virtual ValueTask ScheduleTask(ValueTask valueTask, ThreadPriority priority)
        {
            QueueValueTask(valueTask, priority);
            return valueTask;
        }

        public virtual bool QueueValueTask(ValueTask task, ThreadPriority priority)
        {
            if (_disposed)
            {
                return false;
            }
            return _taskQueueWriters[priority].TryWrite((priority, task));
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns></returns>
        protected override IEnumerable<Task>? GetScheduledTasks() =>
            _taskQueueReaders.SelectMany(taskQueueReader =>
                taskQueueReader.Value.ReadAllAsync().ToBlockingEnumerable()
                .Select(task => task.task.AsTask()));

        protected override void QueueTask(Task task)
        {
            QueueTask(task, ThreadPriority.Normal);
        }

        protected virtual bool QueueTask(Task task, ThreadPriority threadPriority = ThreadPriority.Normal)
        {
            return QueueValueTask(new ValueTask(task), threadPriority);
        }

        protected virtual bool QueueTask(ValueTask task, ThreadPriority threadPriority = ThreadPriority.Normal)
        {
            return QueueValueTask(task, threadPriority);
        }

        protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
        {
            // Only execute inline if we're on a ThreadPool thread
            if (Thread.CurrentThread.IsThreadPoolThread)
            {
                // If previously queued, try to remove it from the queue
                if (taskWasPreviouslyQueued && !TryDequeue(task))
                    return false;

                // Execute the task
                return TryExecuteTask(task);
            }

            return false;
        }

        protected virtual bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued, TaskScheduler taskScheduler)
        {
            if (task.IsCompleted || task.IsCompletedSuccessfully)
            {
                return false;
            }

            if (taskScheduler.Id == Id)
            {
                if (taskWasPreviouslyQueued && !TryDequeue(task))
                    return false;

                // Execute the task
                return TryExecuteTask(task);
            }
            return false;
        }

        //protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
        //{
        //    // 1. Check if we're on an appropriate task for inline execution
        //    bool canExecuteInline = IsAppropriateTaskForInlineExecution(task);

        //    if (canExecuteInline)
        //    {
        //        // 2. If task was previously queued, try to remove it from the queue
        //        if (taskWasPreviouslyQueued)
        //        {
        //            if (!TryDequeue(task))
        //                return false;  // Couldn't remove from queue, so don't execute inline
        //        }

        //        // 3. Try to execute the task
        //        return TryExecuteTask(task);
        //    }

        //    return false;  // Can't execute inline
        //}

        protected virtual bool IsAppropriateThreadForInlineExecution(Thread thread)
        {
            // Implementation depends on your scheduler's threading model
            // Example 1: Only allow inline execution on threads created by this scheduler
            //return _threads.Contains(Thread.CurrentThread);
            // Example 2: Allow inline execution on any thread except the UI thread
            // return Thread.CurrentThread != _uiThread;

            // Example 3: Allow inline execution based on some runtime condition
            // return _allowInlineExecution && (_currentLoad < _maxLoad);

            // Example 4: Allow inline execution based on the name of the thread
            // return Thread.CurrentThread.Name == thread.Name;

            return false;
        }

        protected virtual bool IsAppropriateTaskForInlineExecution(Task task)
        {
            //return !task.CreationOptions.HasFlag(TaskCreationOptions.RunContinuationsAsynchronously);
            return false;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    foreach (ThreadPriority threadPriority in threadPriorities)
                    {
                        foreach(bool result in TryCompleteAsync(threadPriority).ToBlockingEnumerable())
                        {
                        }
                        TryJoinPriorityThreadsAsync(threadPriority).Wait();
                    }
                }
                Interlocked.Exchange(ref _disposed, true);
            }
        }

        protected async IAsyncEnumerable<bool> TryCompleteAsync(ThreadPriority threadPrioritiy)
        {
            if(_semaphoreSlims[threadPrioritiy].CurrentCount == 0)
            {
                yield return false;
            }
            try
            {
                await _semaphoreSlims[threadPrioritiy].WaitAsync();
                await _taskQueueReaders[threadPrioritiy].Completion;
                while (!_taskQueueWriters[threadPrioritiy].TryComplete())
                {
                    yield return false;
                }
                yield return true;
            }
            finally
            {
                _semaphoreSlims[threadPrioritiy].Release();
            }
        }


        //protected async Task<IEnumerable<bool?>> TryCompleteAsync()
        //{
        //    await foreach (bool? result in TryCompleteOrDefaultAsync())
        //    {
        //        yield return result;
        //    }
        //}

        //protected async IAsyncEnumerable<bool?> TryCompleteOrDefaultAsync()
        //{
        //    if (_taskQueueReaders.Count == 0)
        //    {
        //        yield return default;
        //    }
        //    if (_taskQueueWriters.Count == 0)
        //    {
        //        yield return default;
        //    }
        //    if(_semaphore.CurrentCount == 0)
        //    {
        //        yield return default;
        //    }
        //    try
        //    {
        //        await _semaphore.WaitAsync();
        //        await Task.WhenAll(_taskQueueReaders.Values.Select(taskQueueReader => taskQueueReader.Completion));
        //        foreach (KeyValuePair<ThreadPriority, ChannelWriter<(ThreadPriority threadPriority, ValueTask task)>> taskQueueWriter in _taskQueueWriters)
        //        {
        //            while (!taskQueueWriter.Value.TryComplete()) {
        //                yield return false;
        //            }
        //        }
        //        yield return true;
        //    }
        //    finally
        //    {
        //        _semaphore.Release();
        //    }
        //}

        //protected async IAsyncEnumerable<bool?> TryCompleteOrDefaultAsync()
        //{
        //    if (_taskQueueReaders.Count == 0)
        //    {
        //        yield return default;
        //    }
        //    if (_taskQueueWriters.Count == 0)
        //    {
        //        yield return default;
        //    }
        //    if (_semaphore.CurrentCount == 0)
        //    {
        //        yield return default;
        //    }
        //    try
        //    {
        //        await _semaphore.WaitAsync();
        //        await Task.WhenAll(_taskQueueReaders.Values.Select(taskQueueReader => taskQueueReader.Completion));
        //        foreach (KeyValuePair<ThreadPriority, ChannelWriter<(ThreadPriority threadPriority, ValueTask task)>> taskQueueWriter in _taskQueueWriters)
        //        {
        //            while (!taskQueueWriter.Value.TryComplete())
        //            {
        //                yield return false;
        //            }
        //        }
        //        yield return true;
        //    }
        //    finally
        //    {
        //        _semaphore.Release();
        //    }
        //}

        protected virtual async ValueTask DisposeOrDefaultAsync(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    foreach(ThreadPriority threadPriority in threadPriorities)
                    {
                        await foreach (bool result in TryCompleteAsync(threadPriority))
                        {

                        }
                        await TryJoinPriorityThreadsOrDefaultAsync(threadPriority);
                    }
                    Interlocked.Exchange(ref _disposed, true);
                }
            }
            return;
        }

        protected virtual bool TryJoinThread(Thread thread)
        {
            if (thread.ThreadState == ThreadState.Running)
            {
                return thread.Join(TimeSpan.MaxValue);
            }
            return thread.Join(TimeSpan.MaxValue);
        }

        protected virtual async Task<bool?> TryJoinPriorityThreadsAsync(ThreadPriority threadPriority)
        {
            if (!_disposed)
            {
                if (_semaphoreSlims[threadPriority].CurrentCount == 0)
                {
                    return default;
                }
                try
                {
                    if (_semaphoreSlims[threadPriority].CurrentCount == 0)
                    {
                        return default;
                    }
                    await _semaphoreSlims[threadPriority].WaitAsync();
                }
                finally
                {
                    _semaphoreSlims[threadPriority].Release();
                }
            }
            return null;
        }

        protected virtual async ValueTask<bool?> TryJoinPriorityThreadsOrDefaultAsync(ThreadPriority threadPriority)
        {
            if (!_disposed)
            {

                if (_semaphoreSlims[threadPriority].CurrentCount == 0)
                {
                    return default;
                }
                try
                {
                    await _semaphoreSlims[threadPriority].WaitAsync();
                    return TryJoinThread(_threads[threadPriority]);
                }
                finally
                {
                    _semaphoreSlims[threadPriority].Release();
                }
            }
            return null;
        }



        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        public async ValueTask DisposeAsync()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            await DisposeOrDefaultAsync(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
