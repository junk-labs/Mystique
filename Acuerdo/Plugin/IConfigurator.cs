using System.Windows;

namespace Acuerdo.Plugin
{
    /// <summary>
    /// 設定UIの提供を行うクラスが実装するインターフェイス
    /// </summary>
    public interface IConfigurator
    {
        void Transition(Window owner);
    }
}
