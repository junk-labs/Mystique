using System;

namespace Inscribe.Communication.Posting
{
    /// <summary>
    /// ツイート時にエラーではない追加事項が発生したことを通知します。
    /// </summary>
    [Serializable]
    public class TweetAnnotationException : Exception
    {
        public enum AnnotationKind
        {
            NearUnderControl
        }

        public AnnotationKind Kind { get; set; }

        public TweetAnnotationException(AnnotationKind kind)
        {
            Kind = kind;
        }
        public TweetAnnotationException(AnnotationKind kind, string message)
            : base(message)
        {
            Kind = kind;
        }

        public TweetAnnotationException(AnnotationKind kind, string message, Exception inner)
            : base(message, inner)
        {
            Kind = kind;
        }

        protected TweetAnnotationException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }
}
