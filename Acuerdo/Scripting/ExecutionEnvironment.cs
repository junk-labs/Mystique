using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Acuerdo.Scripting
{
    /// <summary>
    /// スクリプトの実行環境を定義します。
    /// </summary>
    public sealed class ExecutionEnvironment : IDisposable
    {
        private bool isDisposed = false;

        ~ExecutionEnvironment()
        {
            if (!this.isDisposed)
                this.Dispose(false);
        }

        public void Dispose()
        {
            this.isDisposed = true;
            this.Dispose(true);
        }

        private void Dispose(bool disposing)
        {
        }
    }
}
