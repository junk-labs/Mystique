using Livet.Messaging;

namespace Mystique.Views.Behaviors.Messages
{
    public class TextBoxSetCaretMessage : InteractionMessage
    {
        public TextBoxSetCaretMessage(string messageKey, int selectionStart, int selectionLength = 0)
            : base(messageKey)
        {
            this._selectionStart = selectionStart;
            this._selectionLength = selectionLength;
        }

        protected override System.Windows.Freezable CreateInstanceCore()
        {
            return new TextBoxSetCaretMessage(this.MessageKey, this._selectionStart, this._selectionLength);
        }

        private int _selectionStart;
        public int SelectionStart
        {
            get { return this._selectionStart; }
        }

        private int _selectionLength;
        public int SelectionLength
        {
            get { return this._selectionLength; }
        }
    }
}
