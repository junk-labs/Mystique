using System;
using System.Net;

namespace Dulcet.Network
{
    public class WebResult<T>
    {
        public Uri Target { get; private set; }

        private bool _succeeded = false;
        /// <summary>
        /// オペレーションが成功したかを取得します。<para />
        /// trueの時、データはNULLではないことを保証します。
        /// </summary>
        public bool Succeeded
        {
            get { return this._succeeded && Data != null; }
            private set { this._succeeded = value; }
        }

        public T Data { get; private set; }

        public Exception ThrownException { get; private set; }

        public HttpStatusCode StatusCode { get; private set; }

        public WebResult(Uri target, bool suceeded, HttpStatusCode code, T data, Exception thrown)
        {
            this.Target = target;
            this.Succeeded = suceeded;
            this.StatusCode = code;
            this.Data = data;
            this.ThrownException = thrown;
        }

        public WebResult(Uri target, HttpStatusCode code, T data)
            : this(target, true, code, data, null) { }

        public WebResult(Uri target, Exception thrown, HttpStatusCode code = (HttpStatusCode)0)
            : this(target, false, code, default(T), thrown) { }
    }
}
