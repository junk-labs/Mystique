using Inscribe.Configuration.Tabs;

namespace Inscribe.Core
{
    /// <summary>
    /// 現在の状態を表す状態変数のプロキシ
    /// </summary>
    public static class StateProxy
    {
        /// <summary>
        /// 現在アクティブになっているタブのプロパティ
        /// </summary>
        public static TabProperty CurrentTabProperty { get; set; }
    }
}
