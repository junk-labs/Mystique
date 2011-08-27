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
    public class SetListSelectionMessage : InteractionMessage
    {
        //Viewでメッセージインスタンスを生成する時のためのコンストラクタ
        public SetListSelectionMessage()
        {
        }

        //ViewModelからMessenger経由での発信目的でメッセージインスタンスを生成するためのコンストラクタ
        public SetListSelectionMessage(string messageKey, ListSelectionKind kind, object initSelectedItem)
            : base(messageKey)
        {
            this.ListSelectionKind = kind;
            this.InitialSelectedItem = initSelectedItem;
        }

        public ListSelectionKind ListSelectionKind
        {
            get { return (ListSelectionKind)GetValue(ListSelectionKindProperty); }
            set { SetValue(ListSelectionKindProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ListSelectionKindProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ListSelectionKindProperty =
            DependencyProperty.Register("ListSelectionKind", typeof(ListSelectionKind), typeof(SetListSelectionMessage), new UIPropertyMetadata(ListSelectionKind.Deselect));

        public object InitialSelectedItem
        {
            get { return (object)GetValue(InitialSelectedItemProperty); }
            set { SetValue(InitialSelectedItemProperty, value); }
        }

        // Using a DependencyProperty as the backing store for InitialSelectedItem.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty InitialSelectedItemProperty =
            DependencyProperty.Register("InitialSelectedItem", typeof(object), typeof(SetListSelectionMessage), new UIPropertyMetadata(null));

        /// <summary>
        /// 派生クラスでは必ずオーバーライドしてください。Freezableオブジェクトとして必要な実装です。<br/>
        /// 通常このメソッドは、自身の新しいインスタンスを返すように実装します。
        /// </summary>
        /// <returns>自身の新しいインスタンス</returns>
        protected override System.Windows.Freezable CreateInstanceCore()
        {
            return new SetListSelectionMessage(this.MessageKey, this.ListSelectionKind, this.InitialSelectedItem);
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
