using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using Inscribe.Configuration;
using Inscribe.Data;
using Livet;

namespace Inscribe.Storage
{
    public static class ExceptionStorage
    {

        #region ExceptionUpdatedイベント

        public static event EventHandler<EventArgs> ExceptionUpdated;
        private static Notificator<EventArgs> _ExceptionUpdatedEvent;
        public static Notificator<EventArgs> ExceptionUpdatedEvent
        {
            get
            {
                if (_ExceptionUpdatedEvent == null) _ExceptionUpdatedEvent = new Notificator<EventArgs>();
                return _ExceptionUpdatedEvent;
            }
            set { _ExceptionUpdatedEvent = value; }
        }

        private static void OnExceptionUpdated(EventArgs e)
        {
            var threadSafeHandler = Interlocked.CompareExchange(ref ExceptionUpdated, null, null);
            if (threadSafeHandler != null) threadSafeHandler(null, e);
            ExceptionUpdatedEvent.Raise(e);
        }

        #endregion
      

        private static SafeLinkedList<ExceptionDescription> exceptions = new SafeLinkedList<ExceptionDescription>();

        /// <summary>
        /// 例外を登録します。
        /// </summary>
        public static void Register(Exception excp, ExceptionCategory category, string message = null, Action retry = null)
        {
            if (excp == null)
                throw new ArgumentNullException("excp");
            WebException wex;
            if (Setting.IsInitialized && Setting.Instance.ExperienceProperty.IgnoreTimeoutError &&
                (wex = excp as WebException) != null && wex.Status == WebExceptionStatus.Timeout)
                return;
            exceptions.AddLast(new ExceptionDescription(excp, category, message ?? excp.Message, retry));
            OnExceptionUpdated(EventArgs.Empty);
        }

        public static void Remove(ExceptionDescription description)
        {
            exceptions.Remove(description);
            OnExceptionUpdated(EventArgs.Empty);
        }

        public static IEnumerable<ExceptionDescription> Exceptions
        {
            get { return exceptions; }
        }
    }

    /// <summary>
    /// 例外デスクリプタ
    /// </summary>
    public class ExceptionDescription
    {
        /// <summary>
        /// 例外本体
        /// </summary>
        public Exception Exception { get; private set; }

        /// <summary>
        /// 例外カテゴリ
        /// </summary>
        public ExceptionCategory Category { get; private set; }

        /// <summary>
        /// 例外メッセージ(追加)
        /// </summary>
        public string Message { get; private set; }

        /// <summary>
        /// 再試行を指定された場合に実行するアクションデリゲート
        /// </summary>
        public Action RetryAction { get; private set; }

        public ExceptionDescription(Exception exception, ExceptionCategory category, string message, Action retry)
        {
            this.Exception = exception;
            this.Category = category;
            this.Message = message;
            this.RetryAction = retry;
        }
    }

    public enum ExceptionCategory
    {
        /// <summary>
        /// ソフトウェアの内部で予備的に設けられたException
        /// </summary>
        AssertionFailed,
        /// <summary>
        /// ソフトウェアの誤動作を示すException
        /// </summary>
        InternalError,
        /// <summary>
        /// プラグインに起因するエラー(プラグインに起因するプラグインローダーエラーを含みます。)
        /// </summary>
        PluginError,
        /// <summary>
        /// 設定値に瑕疵がある場合のエラー
        /// </summary>
        ConfigurationError,
        /// <summary>
        /// ユーザー操作に起因するエラー
        /// </summary>
        UserError,
        /// <summary>
        /// Twitterとの通信状況によって生じたエラー
        /// </summary>
        TwitterError,
    }
}
