
namespace Acuerdo.Plugin
{
    /// <summary>
    /// Krile2 プラグインインターフェイス
    /// </summary>
    public interface IPlugin
    {
        /// <summary>
        /// プラグインの名前を取得します。
        /// </summary>
        string Name { get; }

        /// <summary>
        /// プラグイン バージョンを取得します。
        /// </summary>
        double Version { get; }

        /// <summary>
        /// プラグインがロードされたことを通知します。
        /// </summary>
        void Loaded();

        /// <summary>
        /// 設定UIプロバイダインターフェイスを取得します。
        /// </summary>
        /// <returns></returns>
        IConfigurator ConfigurationInterface { get; }

    }
}
