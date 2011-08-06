using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Livet.Messaging;
using System.Windows;

namespace Inscribe.ViewModels.Behaviors.Messaging
{
    /*
     * 戻り値のある相互作用メッセージはResponsiveInteractionMessage<T>を継承して作成します。
     * Tは戻り値の型です。
     * 戻り値のない相互作用メッセージはInteractionMessageを継承して作成します。
     */
    public class SetListSelectionMessage : ResponsiveInteractionMessage<string>
    {
        //Viewでメッセージインスタンスを生成する時のためのコンストラクタ
        public SetListSelectionMessage()
        {
        }

        //ViewModelからMessenger経由での発信目的でメッセージインスタンスを生成するためのコンストラクタ
        public SetListSelectionMessage(string messageKey, ListSelectionKind kind = ListSelectionKind.Deselect)
            : base(messageKey)
        {
            this.ListSelectionKind = kind;
        }

        /*
         * メッセージに保持させたい情報をプロパティとして定義してください。
         * Viewでバインド可能なプロパティにするために依存関係プロパティとして定義する事をお勧めします。
         * 通常依存関係プロパティはコードスニペット propdpを使用して定義します。
         * もし普通のプロパティとして定義したい場合はコードスニペット propを使用して定義します。
         */

        public ListSelectionKind ListSelectionKind
        {
            get { return (ListSelectionKind)GetValue(ListSelectionKindProperty); }
            set { SetValue(ListSelectionKindProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ListSelectionKindProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ListSelectionKindProperty =
            DependencyProperty.Register("ListSelectionKind", typeof(ListSelectionKind), typeof(SetListSelectionMessage), new UIPropertyMetadata(ListSelectionKind.Deselect));

        /// <summary>
        /// 派生クラスでは必ずオーバーライドしてください。Freezableオブジェクトとして必要な実装です。<br/>
        /// 通常このメソッドは、自身の新しいインスタンスを返すように実装します。
        /// </summary>
        /// <returns>自身の新しいインスタンス</returns>
        protected override System.Windows.Freezable CreateInstanceCore()
        {
            return new SetListSelectionMessage(this.MessageKey, this.ListSelectionKind);
        }
    }

    public enum ListSelectionKind
    {
        Deselect,
        SelectFirst,
        SelectLast,
        SelectBelow,
        SelectAbove,
        SelectAboveAndNull,
    }
}
