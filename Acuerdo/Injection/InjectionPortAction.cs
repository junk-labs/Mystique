using System;
using System.Collections.Generic;
using System.Linq;

namespace Acuerdo.Injection
{
    public sealed class InjectionPort<T> : IInjectionPort<T>
    {
        private readonly Action<T> finalCallback;

        private LinkedList<InjectDelegate<T>> callFirst;
        private LinkedList<InjectDelegate<T>> callMiddle;
        private LinkedList<InjectDelegate<T>> callLast;

        public InjectionPort(Action<T> finalCallback)
        {
            this.finalCallback = finalCallback;
            this.callFirst = new LinkedList<InjectDelegate<T>>();
            this.callMiddle = new LinkedList<InjectDelegate<T>>();
            this.callLast = new LinkedList<InjectDelegate<T>>();
        }

        /// <summary>
        /// インジェクションを追加します。
        /// </summary>
        /// <param name="injectionAction">インジェクション関数を指定します。</param>
        public void Injection(InjectDelegate<T> injectAction)
        {
            this.Injection(injectAction, InjectionMode.NoConsider);
        }

        /// <summary>
        /// インジェクションを追加します。
        /// </summary>
        /// <param name="injectionAction">インジェクション関数を指定します。</param>
        /// <param name="mode">インジェクションの方針を決定します。</param>
        public void Injection(InjectDelegate<T> injectAction, InjectionMode mode)
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
        public IInjectionPort<T> GetInterface()
        {
            return this;
        }

        /// <summary>
        /// インジェクションチェーンを巡回します。
        /// </summary>
        public void Execute(T argument)
        {
            var callChain = this.callFirst.Concat(this.callMiddle).Concat(this.callLast).Reverse().ToArray();
            if (callChain.Length == 0)
            {
                this.finalCallback(argument);
            }
            else
            {
                var prevCall = finalCallback;
                foreach (var call in callChain)
                {
                    prevCall = arg => call(arg, prevCall, finalCallback);
                }
                prevCall(argument);
            }
        }
    }
}
