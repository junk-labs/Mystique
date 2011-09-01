using System;

namespace Inscribe.Data
{
    /// <summary>
    /// 外部に対してReaderWriterLockを提供します。<para />
    /// 使い方に気を付けてね☆
    /// </summary>
    public class ReaderWriterLockWrap : ReaderWriterLockBase
    {
        public IDisposable GetReaderLock()
        {
            return ReaderLock();
        }

        public IDisposable GetWriterLock()
        {
            return WriterLock();
        }
    }
}
