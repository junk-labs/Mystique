using System;
using System.Collections.Generic;
using System.Linq;

namespace Acuerdo.Injection
{
    public sealed class InjectionPort<T, TResult> : IInjectionPort<T, TResult>
    {
        private readonly Func<T, TResult> finalCallback;

        private LinkedList<InjectDelegate<T, TResult>> callFirst;
        private LinkedList<InjectDelegate<T, TResult>> callMiddle;
        private LinkedList<InjectDelegate<T, TResult>> callLast;

        public InjectionPort(Func<T, TResult> finalCallback)
        {
            this.finalCallback = finalCallback;
            this.callFirst = new LinkedList<InjectDelegate<T, TResult>>();
            this.callMiddle = new LinkedList<InjectDelegate<T, TResult>>();
            this.callLast = new LinkedList<InjectDelegate<T, TResult>>();
        }

        /// <summary>
        /// インジェクションを追加します。
        /// </summary>
        /// <param name="injectionAction">インジェクション関数を指定します。</param>
        public void Injection(InjectDelegate<T, TResult> injectAction)
        {
            this.Injection(injectAction, InjectionMode.NoConsider);
        }

        /// <summary>
        /// インジェクションを追加します。
        /// </summary>
        /// <param name="injectionAction">インジェクション関数を指定します。</param>
        /// <param name="mode">インジェクションの方針を決定します。</param>
        public void Injection(InjectDelegate<T, TResult> injectAction, InjectionMode mode)
        {
            switch (mode)
            {
                case InjectionMode.InjectionFirst:
                    this.callFirst.AddFirst(injectAction);
                    break;
                case InjectionMode.InjectionLast:
                    this.callLast.AddLast(injectAction);
                    break;
                case InjectionMode.NoConsider:
                default:
                    this.callMiddle.AddLast(injectAction);
                    break;
            }
        }

        /// <summary>
        /// 外部に公開するインターフェイスを取得します。
        /// </summary>
        public IInjectionPort<T, TResult> GetInterface()
        {
            return this;
        }

        /// <summary>
        /// インジェクションチェーンを巡回します。
        /// </summary>
        public TResult Execute(T argument)
        {
            var callChain = this.callFirst.Concat(this.callMiddle).Concat(this.callLast).Reverse().ToArray();
            if (callChain.Length == 0)
            {
                return this.finalCallback(argument);
            }
            else
            {
                var prevCall = finalCallback;
                foreach (var call in callChain)
                {
                    prevCall = arg => call(arg, prevCall, finalCallback);
                }
                return prevCall(argument);
            }
        }
    }
}
