using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using Dulcet.Twitter.Credential;

namespace Dulcet.Twitter.Streaming
{
    /// <summary>
    /// Streams streaming connection
    /// </summary>
    public class StreamingConnection : IDisposable
    {
        /// <summary>
        /// Parent streaming core
        /// </summary>
        protected readonly StreamingCore parentCore;

        /// <summary>
        /// Current credential provider
        /// </summary>
        public CredentialProvider Provider;

        private Stream receiveStream;
        private Thread streamReceiver;
        private Timer timeoutTimer;
        private int timeoutCount = 0;
        private int timeoutValue;
        private bool isTimeout = false;

        private readonly HttpWebRequest usedRequest;

        internal StreamingConnection(StreamingCore core, CredentialProvider provider, HttpWebRequest usedRequest, Stream strm, int timeoutSec)
        {
            if (core == null)
                throw new ArgumentNullException("core");
            if (provider == null)
                throw new ArgumentNullException("provider");
            if (usedRequest == null)
                throw new ArgumentException("usedRequest");
            if (strm == null)
                throw new ArgumentNullException("strm");
            this.timeoutValue = timeoutSec;
            this.parentCore = core;
            this.Provider = provider;
            this.receiveStream = strm;
            this.usedRequest = usedRequest;
            this.streamReceiver = new Thread(StreamingThread);
            this.streamReceiver.Start();
            // タイムアウト用タイマー
            timeoutTimer = new Timer(TimeoutCountUp, null, 1000, 1000);
        }

        private void TimeoutCountUp(object o)
        {
            timeoutCount++;
            if (timeoutCount >= timeoutValue && !isTimeout)
            {
                isTimeout = true;
                this.Dispose();
            }
        }

        private static int STGC = 0;
        private void StreamingThread()
        {
                var cno = Interlocked.Increment(ref STGC);
            try
            {
                using (var sr = new StreamReader(this.receiveStream))
                {
                    while (!sr.EndOfStream && !parentCore.IsDisposed && !disposed && !isTimeout)
                    {
                        timeoutCount = 0;
                        String cline = sr.ReadLine();
                        System.Diagnostics.Debug.WriteLine("thread: " + cno + " ( " + Provider.ToString() + " ) / " + cline);
                        this.parentCore.EnqueueReceivedObject(this.Provider, cline);
                        // sr.ReadLine());
                    }
                }
            }
            catch (ObjectDisposedException) { }
            catch (ThreadAbortException) { }
            catch (Exception e)
            {
                // ignore all errors when disposing
                if (!disposed)
                {
                    parentCore.RaiseOnExceptionThrown(e);
                }
            }
            finally
            {
                System.Diagnostics.Debug.WriteLine("thread: ***DISCONNECTED*** " + cno + " ( " + Provider.ToString() + " ) ");
                FinalizeStream();
            }
        }

        public bool IsAlive
        {
            get { return !this.disposed; }
        }

        /// <summary>
        /// Finalize streaming connection
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Finalizer
        /// </summary>
        ~StreamingConnection()
        {
            this.Dispose(false);
        }

        private volatile bool disposed = false;
        /// <summary>
        /// Finalize streaming connection
        /// </summary>
        /// <param name="disposing">Called from Dispose() method</param>
        protected virtual void Dispose(bool disposing)
        {
            if (this.disposed) return;
            System.Diagnostics.Debug.WriteLine("Disposing:" + Provider.ToString());
            this.disposed = true;
            FinalizeStream();
        }

        private object finalizeLockObject = new object();
        private volatile bool finalized = false;
        private void FinalizeStream()
        {
            lock (finalizeLockObject)
            {
                if (finalized) return;
                finalized = true;
            }
            usedRequest.Abort();
            try
            {
                var rs = this.receiveStream;
                this.receiveStream = null;
                if (rs != null)
                {
                    rs.Close();
                    rs.Dispose();
                }
            }
            catch { }
            finally
            {
                this.parentCore.UnregisterConnection(this);
                this.parentCore.RaiseOnDisconnected(this);
            }
        }
    }
}
