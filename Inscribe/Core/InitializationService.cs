using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Inscribe.Core
{
    public static class InitializationService
    {
        public static InitializationStep CurrentStep { get; internal set; }
    }

    public enum InitializationStep
    {
        /// <summary>
        /// システムコアはまだ初期化されていない
        /// </summary>
        CoreHasNotInitialized,
        /// <summary>
        /// システムコアが初期化され、起動中
        /// </summary>
        CoreInitialized,
        /// <summary>
        /// 接続を待機中
        /// </summary>
        EstablishingConnection,
        /// <summary>
        /// 情報を取得中
        /// </summary>
        ReceivingInformations,
        /// <summary>
        /// 初期化処理がすべて完了し、安定状態
        /// </summary>
        Stable,
    }
}
