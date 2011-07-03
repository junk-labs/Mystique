using System;
using System.Collections.Generic;
using Inscribe.Model;
using Inscribe.Storage;
using Livet;
using System.Linq;

namespace Mystique.ViewModels.PartBlocks.InputBlock
{
    public class InputDescription : ViewModel
    {
        private long _inReplyToId = 0;
        public long InReplyToId
        {
            get { return this._inReplyToId; }
            set
            {
                this._inReplyToId = value;
                RaisePropertyChanged(() => InReplyToId);
                RaisePropertyChanged(() => IsInReplyToEnabled);
            }
        }

        public bool IsInReplyToEnabled
        {
            get { return this._inReplyToId != 0; }
        }

        public string InReplyToIdStatus
        {
            get
            {
                var tvm = TweetStorage.Get(this._inReplyToId);
                if (tvm == null)
                    return "[undefined]";
                else
                    return "@" + tvm.Status.User.ScreenName + ": " + tvm.Status.Text;
            }
        }

        private string _inputText = String.Empty;
        public string InputText
        {
            get { return this._inputText; }
            set
            {
                this._inputText = value;
                RaisePropertyChanged(() => InputText);
            }
        }

        private string _attachedImage = null;
        public string AttachedImage
        {
            get { return this._attachedImage; }
            set
            {
                this._attachedImage = value;
                RaisePropertyChanged(()=>AttachedImage);
                RaisePropertyChanged(() => IsAttachedImage);
            }
        }

        public bool IsAttachedImage
        {
            get { return this._attachedImage != null; }
        }

        public IEnumerable<TweetWorker> ReadyUpdate(InputBlockViewModel ibvm, IEnumerable<AccountInfo> infos)
        {
            return infos.Select(i => new TweetWorker(ibvm, i, InputText, InReplyToId, AttachedImage, null));
        }

    }
}
