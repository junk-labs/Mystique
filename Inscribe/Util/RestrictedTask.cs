using System.Collections.Generic;

namespace System.Threading.Tasks
{
    public abstract class RestrictedTaskDispatcher : IDisposable
    {
        private Thread workDispatchThread;
        private Semaphore workSemaphore;
        private ManualResetEvent workWaiter;

        public RestrictedTaskDispatcher(int maxParallelism)
        {
            workWaiter = new ManualResetEvent(false);
            workSemaphore = new Semaphore(maxParallelism, maxParallelism);
            workDispatchThread = new Thread(WorkDispatcher);
            workDispatchThread.Start();
        }

        private void WorkDispatcher()
        {
            while (!disposed)
            {
                if (workWaiter.WaitOne(5000))
                {
                    while (isRemainWork)
                    {
                        workWaiter.Reset();
                        workSemaphore.WaitOne();
                        var act = GetWorkOne();
                        if (act == null)
                            break;
                        Task.Factory.StartNew(() =>
                            {
                                try
                                {
                                    if (act != null)
                                        act();
                                }
                                finally
                                {
                                    try
                                    {
                                        workSemaphore.Release();
                                    }
                                    catch { }
                                }
                            });
                    }
                }
            }
        }

        protected abstract bool isRemainWork { get; }

        protected abstract Action GetWorkOne();

        protected void NotifyWorkPended()
        {
            workWaiter.Set();
        }

        public void Dispose()
        {
            this.Dispose(true);
        }

        bool disposed = false;
        protected virtual void Dispose(bool disposing)
        {
            System.Diagnostics.Debug.WriteLine("disposed");
            if (disposed) return;
            disposed = true;
            if (workDispatchThread != null)
            {
                var wdt = workDispatchThread;
                workDispatchThread = null;
                if (wdt != null)
                    wdt.Abort();
            }
            workSemaphore.Dispose();
            workWaiter.Dispose();
            GC.SuppressFinalize(this);
        }

        ~RestrictedTaskDispatcher()
        {
            this.Dispose(false);
        }
    }

    public class QueueTaskDispatcher : RestrictedTaskDispatcher
    {
        private Queue<Action> workQueue;

        public QueueTaskDispatcher(int maxParallelism)
            : base(maxParallelism)
        {
            workQueue = new Queue<Action>();
        }

        protected override bool isRemainWork
        {
            get { return workQueue.Count > 0; }
        }

        protected override Action GetWorkOne()
        {
            if (workQueue == null) return null;
            lock (workQueue)
            {
                return workQueue.Dequeue();
            }
        }

        public void Enqueue(Action act)
        {
            if (act == null)
                throw new ArgumentNullException("act");
            lock (workQueue)
            {
                workQueue.Enqueue(act);
            }
            NotifyWorkPended();
        }
    }

    public class StackTaskDispatcher : RestrictedTaskDispatcher
    {
        private Stack<Action> workStack;

        public StackTaskDispatcher(int maxParallelism)
            : base(maxParallelism)
        {
            workStack = new Stack<Action>();
        }

        protected override bool isRemainWork
        {
            get { return workStack.Count > 0; }
        }

        protected override Action GetWorkOne()
        {
            if (workStack == null) return null;
            lock (workStack)
            {
                return workStack.Pop();
            }
        }

        public void Push(Action act)
        {
            if (act == null)
                throw new ArgumentNullException("act");
            lock (workStack)
            {
                workStack.Push(act);
            }
            NotifyWorkPended();
        }
    }
}
