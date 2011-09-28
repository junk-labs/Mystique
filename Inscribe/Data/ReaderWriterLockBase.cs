using System;
using System.Threading;

namespace Inscribe.Data
{
    public abstract class ReaderWriterLockBase
    {
        public ReaderWriterLockBase() : this(LockRecursionPolicy.NoRecursion) { }

        public ReaderWriterLockBase(LockRecursionPolicy lrp)
        {
            this.readerWriterLock = new ReaderWriterLockSlim(lrp);
        }

        protected ReaderWriterLockSlim readerWriterLock;

        protected IDisposable ReaderLock()
        {
            return AcquireReaderLock(readerWriterLock);
        }

        protected IDisposable WriterLock()
        {
            return AcquireWriterLock(readerWriterLock);
        }

        protected IDisposable UpgradableReaderLock()
        {
            return AcquireUpgradableReaderLock(readerWriterLock);
        }

        protected static IDisposable AcquireReaderLock(ReaderWriterLockSlim self)
        {
            return FinallyBlock.Create(() => self.EnterReadLock(), () => self.ExitReadLock());
        }

        protected static IDisposable AcquireWriterLock(ReaderWriterLockSlim self)
        {
            return FinallyBlock.Create(() => self.EnterWriteLock(), () => self.ExitWriteLock());
        }

        protected static IDisposable AcquireUpgradableReaderLock(ReaderWriterLockSlim self)
        {
            return FinallyBlock.Create(() => self.EnterUpgradeableReadLock(), () => self.ExitUpgradeableReadLock());
        }

        protected static class FinallyBlock
        {
            public static FinallyBlock<object> Create(Action pre, Action post)
            {
                pre();

                return new FinallyBlock<object>(null, _ => post());
            }

            public static FinallyBlock<TFinal> Create<TFinal>(TFinal value, Action<TFinal> dispose)
            {
                return new FinallyBlock<TFinal>(value, dispose);
            }
        }

        protected class FinallyBlock<TFinal> : IDisposable
        {
            TFinal value;
            Action<TFinal> dispose;

            public FinallyBlock(TFinal value, Action<TFinal> dispose)
            {
                this.value = value;
                this.dispose = dispose;
            }

            public void Dispose()
            {
                dispose(value);
            }

            public static implicit operator TFinal(FinallyBlock<TFinal> self)
            {
                return self.value;
            }
        }
    }
}
