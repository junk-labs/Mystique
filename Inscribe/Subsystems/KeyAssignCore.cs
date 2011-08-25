using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using Inscribe.Configuration;
using Inscribe.Configuration.Settings;
using Inscribe.Storage;
using Inscribe.Subsystems.KeyAssign;
using Livet;

namespace Inscribe.Subsystems
{
    /// <summary>
    /// キーアサインの管理を行います。
    /// </summary>
    public static class KeyAssignCore
    {
        private static Dictionary<string, Action> callbacks;

        private static AssignDescription assignDescription = null;

        #region KeyAssignUpdatedイベント

        public static event EventHandler<EventArgs> KeyAssignUpdated;
        private static Notificator<EventArgs> _KeyAssignUpdatedEvent;
        public static Notificator<EventArgs> KeyAssignUpdatedEvent
        {
            get
            {
                if (_KeyAssignUpdatedEvent == null) _KeyAssignUpdatedEvent = new Notificator<EventArgs>();
                return _KeyAssignUpdatedEvent;
            }
            set { _KeyAssignUpdatedEvent = value; }
        }

        private static void OnKeyAssignUpdated(EventArgs e)
        {
            var threadSafeHandler = Interlocked.CompareExchange(ref KeyAssignUpdated, null, null);
            if (threadSafeHandler != null) threadSafeHandler(null, e);
            KeyAssignUpdatedEvent.Raise(e);
        }

        #endregion

        static KeyAssignCore()
        {
            callbacks = new Dictionary<string, Action>();
        }

        public static string GetKeyAssignMaps()
        {
            return callbacks.Keys.Select(k => k + " : " + LookupKeyFromId(k))
                .JoinString(Environment.NewLine);
        }

        public static void RegisterOperation(string id, Action callback)
        {
            if (id == null)
                throw new ArgumentNullException("id");
            if (callbacks.ContainsKey(id))
                throw new ArgumentException("すでにIDが登録されています。");
            callbacks.Add(id, callback);
        }

        public static String GetPath(string fileName)
        {
            return Path.Combine(Path.GetDirectoryName(Define.ExeFilePath), Define.KeyAssignDirectory, fileName);
        }

        public static String LookupKeyFromId(string id)
        {
            if (assignDescription == null)
                return String.Empty;
            return assignDescription.AssignDatas.SelectMany(s => s.Item2)
                .Where(i => i.ActionId == id)
                .Select(s => KeyToString(s.Modifiers, s.Key)).JoinString(", ");
        }

        public static String KeyToString(ModifierKeys modkeys, Key key)
        {
            String ret = key.ToString();
            if (modkeys.HasFlag(ModifierKeys.Shift))
                ret = "Shift+" + ret;
            if(modkeys.HasFlag(ModifierKeys.Windows))
                ret = "Win+" + ret;
            if(modkeys.HasFlag(ModifierKeys.Alt))
                ret = "Alt+" + ret;
            if(modkeys.HasFlag(ModifierKeys.Control))
                ret = "Control+" + ret;
            return ret;
        }

        public static void ReloadAssign()
        {
            assignDescription = null;
            if (!String.IsNullOrEmpty(Setting.Instance.KeyAssignProperty.KeyAssignFile) &&
                Setting.Instance.KeyAssignProperty.KeyAssignFile != KeyAssignProperty.DefaultAssignFileName)
            {
                try
                {
                    assignDescription = AssignLoader.LoadAssign(GetPath(Setting.Instance.KeyAssignProperty.KeyAssignFile));
                }
                catch (Exception e)
                {
                    ExceptionStorage.Register(e, ExceptionCategory.ConfigurationError,
                        "キーアサインファイルを読み込めませんでした: " + Setting.Instance.KeyAssignProperty.KeyAssignFile,
                        ReloadAssign);
                    assignDescription = null;
                }
            }
            if (assignDescription == null)
            {
                try
                {
                    assignDescription = AssignLoader.LoadAssign(GetPath(KeyAssignProperty.DefaultAssignFileName));
                }
                catch (Exception e)
                {
                    ExceptionStorage.Register(e, ExceptionCategory.InternalError,
                        "デフォルト キーアサインファイルが破損しています。Krileを再インストールしてください。",
                        ReloadAssign);
                }
            }
            OnKeyAssignUpdated(EventArgs.Empty);
        }

        public static void HandlePreviewEvent(KeyEventArgs e, AssignRegion region)
        {
            if (HandleEventSink(CheckIme(e), region, true, IsSourceFromTextBox(e)))
                e.Handled = true;
        }

        public static void HandleEvent(KeyEventArgs e, AssignRegion region)
        {
            if (HandleEventSink(CheckIme(e), region, false, IsSourceFromTextBox(e)))
                e.Handled = true;
        }

        private static bool IsSourceFromTextBox(KeyEventArgs e)
        {
            return e.OriginalSource is TextBoxBase;
        }

        private static Key CheckIme(KeyEventArgs e)
        {
            if (e.Key == Key.ImeProcessed && e.OriginalSource is TextBoxBase)
            {
                return Key.None;
            }
            else if (e.Key == Key.ImeProcessed)
            {
                InputMethod.Current.ImeState = InputMethodState.Off;
                return e.ImeProcessedKey;
            }
            else
            {
                return e.Key;
            }
        }

        private static bool HandleEventSink(Key key, AssignRegion region, bool preview, bool isSourceFromTextBox)
        {
            var modifier = Keyboard.Modifiers;
            System.Diagnostics.Debug.WriteLine(modifier.ToString() + " " + key.ToString() + " / " + region.ToString() + " ? " + preview.ToString());
            if (assignDescription == null) return false;
            try
            {
                return assignDescription.AssignDatas
                    .First(a => a.Item1 == region)
                    .Item2
                    .Where(a => a.Key == key && a.Modifiers == modifier &&
                        (!preview || a.LookInPreview) && (!isSourceFromTextBox || a.HandleInTextBox))
                    .Select(a => a.ActionId)
                    .Dispatch();
            }
            catch (Exception ex)
            {
                ExceptionStorage.Register(ex, ExceptionCategory.ConfigurationError,
                    "キーアサインを処理中にエラーが発生しました :" + ex.Message);
                return false;
            }
        }

        private static bool Dispatch(this IEnumerable<string> actionIds)
        {
            bool dispatched = false;
            foreach (var id in actionIds)
            {
                Action action;
                if (callbacks.TryGetValue(id, out action))
                    action();
                else
                    throw new ArgumentException("オペレーションID \"" + id + "\" は見つかりませんでした。");
                dispatched = true;
            }
            return dispatched;
        }

        internal static bool ExistsAction(string action)
        {
            return callbacks.ContainsKey(action);
        }
    }
}
