using System;

namespace Inscribe.Communication.Posting
{
    [Serializable]
    public class TweetFailedException : Exception
    {
        public enum TweetErrorKind
        {
            Controlled,
            Duplicated,
            Timeout,
            CommonFailed
        }

        public TweetErrorKind ErrorKind { get; private set; }

        public TweetFailedException(TweetErrorKind kind)
        {
            this.ErrorKind = kind;
        }
        public TweetFailedException(TweetErrorKind kind, string message)
            : base(message)
        {
            this.ErrorKind = kind;
        }
        public TweetFailedException(TweetErrorKind kind, string message, Exception inner)
            : base(message, inner)
        {
            this.ErrorKind = kind;
        }

        protected TweetFailedException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context)
        {
            this.ErrorKind = TweetErrorKind.CommonFailed;
        }
    }
}
