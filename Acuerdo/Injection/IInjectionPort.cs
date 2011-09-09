using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Acuerdo.Injection
{
    public interface IInjectionPort<T, TResult>
    {
        /// <summary>
        /// インジェクションを追加します。
        /// </summary>
        /// <param name="injectionAction">インジェクション関数を指定します。</param>
        void Injection(InjectDelegate<T, TResult> injectAction);

        /// <summary>
        /// インジェクションを追加します。
        /// </summary>
        /// <param name="injectionAction">インジェクション関数を指定します。</param>
        /// <param name="mode">インジェクションの方針を決定します。</param>
        void Injection(InjectDelegate<T, TResult> injectAction, InjectionMode mode);
    }

    public interface IInjectionPort<T>
    {
        /// <summary>
        /// インジェクションを追加します。
        /// </summary>
        /// <param name="injectionAction">インジェクション関数を指定します。</param>
        void Injection(InjectDelegate<T> injectAction);

        /// <summary>
        /// インジェクションを追加します。
        /// </summary>
        /// <param name="injectionAction">インジェクション関数を指定します。</param>
        /// <param name="mode">インジェクションの方針を決定します。</param>
        void Injection(InjectDelegate<T> injectAction, InjectionMode mode);
    }

    public delegate TResult InjectDelegate<T, TResult>(T argument, Func<T, TResult> next, Func<T, TResult> last);

    public delegate void InjectDelegate<T>(T argument, Action<T> next, Action<T> last);

    public enum InjectionResult
    {
        /// <summary>
        /// 次のインジェクションを探索します。
        /// </summary>
        PassThru,
        /// <summary>
        /// 既定のコールバックをすぐに呼び出します。
        /// </summary>
        Handled,
        /// <summary>
        /// 処理をインタラプトします。
        /// </summary>
        Hijack,
    }

    public enum InjectionMode
    {
        /// <summary>
        /// インジェクション順を考慮しません。
        /// </summary>
        NoConsider,
        /// <summary>
        /// なるべく早く呼び出されるようにします。
        /// </summary>
        InjectionFirst,
        /// <summary>
        /// なるべく遅く呼び出されるようにします。
        /// </summary>
        InjectionLast,
    }
}
