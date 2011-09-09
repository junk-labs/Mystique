using System;
using System.Collections.Generic;
using System.Linq;
using Livet;
using Mystique.Views.Behaviors.Messages;
using System.Threading;

namespace Inscribe.ViewModels.Common
{
    public class IntelliSenseTextBoxViewModel : ViewModel
    {
        public IEnumerable<char> SuggestTriggers
        {
            get { return new[] { '@', '#' }; }
        }

        public IEnumerable<char> Splitters
        {
            get { return new[] { '\r', '\n', ' ', '\t', ',', '.' }; }
        }

        private string _textBoxText = String.Empty;
        public string TextBoxText
        {
            get { return this._textBoxText; }
            set
            {
                this._textBoxText = value;
                RaisePropertyChanged(() => TextBoxText);
                OnTextChanged(EventArgs.Empty);
            }
        }

        #region TextChangedイベント
        public event EventHandler<EventArgs> TextChanged;
        private Notificator<EventArgs> _TextChangedEvent;
        public Notificator<EventArgs> TextChangedEvent
        {
            get
            {
                if (_TextChangedEvent == null) _TextChangedEvent = new Notificator<EventArgs>();
                return _TextChangedEvent;
            }
            set { _TextChangedEvent = value; }
        }

        protected void OnTextChanged(EventArgs e)
        {
            var threadSafeHandler = Interlocked.CompareExchange(ref TextChanged, null, null);
            if (threadSafeHandler != null) threadSafeHandler(this, e);
            TextChangedEvent.Raise(e);
        }
        #endregion

        #region ItemsOpeningイベント
        public event EventHandler<EventArgs> ItemsOpening;
        private Notificator<EventArgs> _ItemsOpeningEvent;
        public Notificator<EventArgs> ItemsOpeningEvent
        {
            get
            {
                if (_ItemsOpeningEvent == null) _ItemsOpeningEvent = new Notificator<EventArgs>();
                return _ItemsOpeningEvent;
            }
            set { _ItemsOpeningEvent = value; }
        }

        protected void OnItemsOpening(EventArgs e)
        {
            var threadSafeHandler = Interlocked.CompareExchange(ref ItemsOpening, null, null);
            if (threadSafeHandler != null) threadSafeHandler(this, e);
            ItemsOpeningEvent.Raise(e);
        }
        #endregion
      
        public void RaiseOnItemsOpening()
        {
            OnItemsOpening(EventArgs.Empty);
        }

        private IEnumerable<IntelliSenseItemViewModel> _items = null;
        public IEnumerable<IntelliSenseItemViewModel> Items
        {
            get
            {
                return this._items;
            }
            set
            {
                if (value == null)
                    this._items = new IntelliSenseItemViewModel[0];
                else
                    this._items = value;
                RaisePropertyChanged(() => Items);
                RaisePropertyChanged(() => FilteredItems);
            }
        }

        public IEnumerable<IntelliSenseItemViewModel> FilteredItems
        {
            get
            {
                if (String.IsNullOrEmpty(this.CurrentToken))
                {
                    return this._items;
                }
                else if (this._items == null)
                {
                    return new IntelliSenseItemViewModel[0];
                }
                else if (this._items
                    .Where(i => IntelliSenseTextBoxUtil.CheckContains(i.ItemText,
                        this.CurrentToken, this.SuggestTriggers))
                    .Count() == 0)
                {
                    // 同じトークンで始まるものだけ選択
                    return this._items.Where(s => s.ItemText.StartsWith(this.CurrentToken.Substring(0, 1)));
                }
                else
                {
                    return this._items
                        .Where(i => IntelliSenseTextBoxUtil.CheckContains(i.ItemText,
                            this.CurrentToken, this.SuggestTriggers));
                }
            }
        }

        private string _currentToken = String.Empty;
        public string CurrentToken
        {
            get { return this._currentToken; }
            set
            {
                if (value == null)
                    this._currentToken = String.Empty;
                else
                    this._currentToken = value;
                RaisePropertyChanged(() => FilteredItems);
            }
        }

        public void SetFocus()
        {
            this.Messenger.Raise(new Livet.Messaging.InteractionMessage("SetFocus"));
        }

        public void SetCaret(int selStart, int selLength = 0)
        {
            var cmsg = new TextBoxSetCaretMessage("SetCaret", selStart, selLength);
            this.Messenger.Raise(cmsg);
        }
    }

    public class IntelliSenseItemViewModel : ViewModel
    {
        public string ItemText { get; private set; }

        public Uri ItemImageUri { get; private set; }

        public bool IsImageEnabled
        {
            get { return this.ItemImageUri != null; }
        }

        public IntelliSenseItemViewModel(string itemText, Uri imageUri)
        {
            this.ItemText = itemText;
            this.ItemImageUri = imageUri;
        }
    }


    public static class IntelliSenseTextBoxUtil
    {
        public static bool CheckContains(string haystack, string needle, IEnumerable<char> triggers)
        {
            return CheckIndexOf(haystack, needle, triggers) >= 0;
        }

        public static int CheckIndexOf(string haystack, string needle, IEnumerable<char> triggers)
        {
            foreach (var trigger in triggers)
            {
                if (haystack[0] == trigger && needle[0] == trigger)
                {
                    return haystack.Substring(1).IndexOf(needle.Substring(1),
                        StringComparison.CurrentCultureIgnoreCase);
                }
            }
            return -1;
        }
    }
}
