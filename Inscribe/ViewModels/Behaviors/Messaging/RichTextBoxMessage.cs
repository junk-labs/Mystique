using Livet.Messaging;

namespace Inscribe.ViewModels.Behaviors.Messaging
{
    public class RichTextBoxMessage : InteractionMessage
    {
        public RichTextBoxMessage(string messageKey, RichTextActionType actionType) : base(messageKey)
        {
            this.ActionType = actionType;
        }

        protected override System.Windows.Freezable CreateInstanceCore()
        {
            return new RichTextBoxMessage(this.MessageKey, this.ActionType);
        }

        public RichTextActionType ActionType { get; set; }
    }

    public enum RichTextActionType
    {
        Copy,
        SelectAll
    }
}