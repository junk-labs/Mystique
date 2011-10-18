using System.Windows;
using Livet.Messaging;

namespace Inscribe.ViewModels.Behaviors.Messaging
{
    public class SetListSelectionFromIndexMessage : InteractionMessage
    {
        public SetListSelectionFromIndexMessage() { }

        public SetListSelectionFromIndexMessage(string messageKey, int index, object initSelectedItem)
            :base(messageKey)
        {
            this.Index = index;
            this.InitialSelectedItem = initSelectedItem;
        }



        public int Index
        {
            get { return (int)GetValue(IndexProperty); }
            set { SetValue(IndexProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Index.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IndexProperty =
            DependencyProperty.Register("Index", typeof(int), typeof(SetListSelectionFromIndexMessage), new UIPropertyMetadata(-1));



        public object InitialSelectedItem
        {
            get { return (object)GetValue(InitialSelectedItemProperty); }
            set { SetValue(InitialSelectedItemProperty, value); }
        }

        // Using a DependencyProperty as the backing store for InitialSelectedItem.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty InitialSelectedItemProperty =
            DependencyProperty.Register("InitialSelectedItem", typeof(object), typeof(SetListSelectionFromIndexMessage), new UIPropertyMetadata(null));

        protected override Freezable CreateInstanceCore()
        {
            return new SetListSelectionFromIndexMessage(this.MessageKey, this.Index, this.InitialSelectedItem);
        }
    }
}
