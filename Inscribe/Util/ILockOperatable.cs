using System;

namespace Inscribe.Util
{
    /// <summary>
    /// 内部にロック機構を持ち、また、そのロックを受けた上でのオペレーションを<para />
    /// 実行可能なクラスが実装するインターフェイスです。
    /// </summary>
    public interface ILockOperatable
    {
        /// <summary>
        /// 内部ロックされた動作を実行します。<para />
        /// operation内部でロック動作をするとデッドロックします。
        /// </summary>
        void LockOperate(Delegate operation, params object[] arguments);

        /// <summary>
        /// 内部ロックされた動作を実行します。<para />
        /// operation内部でロック動作をするとデッドロックします。
        /// </summary>
        T LockOperate<T>(Delegate operation, params object[] arguments);

    }

    public static class LockOperateSugar
    {
        /// <summary>
        /// 内部ロックされた動作を実行します。<para />
        /// operation内部でロック動作をするとデッドロックします。
        /// </summary>
        public static void LockOperate(this ILockOperatable target, Action operation)
        {
            target.LockOperate(operation, (object[])null);
        }

        /// <summary>
        /// 内部ロックされた動作を実行します。<para />
        /// operation内部でロック動作をするとデッドロックします。
        /// </summary>
        public static void LockOperate<T>(this ILockOperatable target, Action<T> operation, T argument)
        {
            target.LockOperate(operation, ((object)argument));
        }

        /// <summary>
        /// 内部ロックされた動作を実行します。<para />
        /// operation内部でロック動作をするとデッドロックします。
        /// </summary>
        public static T LockOperate<T>(this ILockOperatable target, Func<T> operation)
        {
            return target.LockOperate<T>(operation, (object[])null);
        }
    }
}