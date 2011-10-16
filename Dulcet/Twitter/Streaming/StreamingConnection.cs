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

        private void StreamingThread()
        {
            try
            {
                using (var sr = new StreamReader(this.receiveStream))
                {
                    while (!sr.EndOfStream && !parentCore.IsDisposed && !disposed && !isTimeout)
                    {
                        timeoutCount = 0;
                        this.parentCore.EnqueueReceivedObject(this.Provider, sr.ReadLine());
                    }
                }
            }
            catch (ObjectDisposedException) { }
            catch (ThreadAbortException) { }
            catch (Exception e)
            {
                // ignore all errors when disposing
                if (disposed) return;
                parentCore.RaiseOnExceptionThrown(e);
            }
            finally
            {
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

        bool disposed = false;
        /// <summary>
        /// Finalize streaming connection
        /// </summary>
        /// <param name="disposing">Called from Dispose() method</param>
        protected virtual void Dispose(bool disposing)
        {
            if (this.disposed) return;
            try
            {
                this.disposed = true;
                usedRequest.Abort();
                var rs = this.receiveStream;
                this.receiveStream = null;
                if (rs != null)
                {
                    rs.Close();
                    rs.Dispose();
                }
                streamReceiver.Abort();
                streamReceiver = null;
            }
            finally
            {
                this.parentCore.UnregisterConnection(this);
                this.parentCore.RaiseOnDisconnected(this);
            }
        }
    }
}
