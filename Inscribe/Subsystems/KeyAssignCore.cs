using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using Inscribe.Common;
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
        private static Dictionary<string, Action<string>> callbacks;

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
            callbacks = new Dictionary<string, Action<string>>();
            Setting.SettingValueChanged += (o, e) => ReloadAssign();
        }

        /// <summary>
        /// キーアサインの設定一覧を判別可能な形式で取得します。
        /// </summary>
        public static string GetKeyAssignMaps()
        {
            return callbacks.Keys.Select(k => k + " : " + LookupKeyFromId(k))
                .JoinString(Environment.NewLine);
        }

        /// <summary>
        /// キーアサイン オペレーションを登録します。
        /// </summary>
        /// <param name="id">キーアサインID</param>
        /// <param name="action">コールバックアクション</param>
        /// <exception cref="System.ArgumentNullException">同一のIDが登録済みです。</exception>
        public static void RegisterOperation(string id, Action action)
        {
            RegisterOperation(id, _ => action());
        }

        /// <summary>
        /// キーアサイン オペレーションを登録します。
        /// </summary>
        /// <param name="id">キーアサインID</param>
        /// <param name="action">コールバックアクション</param>
        /// <exception cref="System.ArgumentNullException">同一のIDが登録済みです。</exception>
        public static void RegisterOperation(string id, Action<string> action)
        {
            if (id == null)
                throw new ArgumentNullException("id");
            if (callbacks.ContainsKey(id))
                throw new ArgumentException("すでにIDが登録されています。");
            callbacks.Add(id, action);
        }

        /// <summary>
        /// キーアサインIDに関連付けられたキー一覧を取得します。
        /// </summary>
        /// <param name="id">キーアサインID</param>
        public static String LookupKeyFromId(string id)
        {
            if (assignDescription == null)
                return String.Empty;
            return assignDescription.AssignDatas.SelectMany(s => s.Item2)
                .Where(i => i.ActionId == id)
                .Select(s => KeyToString(s.Modifiers, s.Key)).JoinString(", ");
        }

        /// <summary>
        /// 現在の設定に合わせて、キーアサイン一覧を更新します。
        /// </summary>
        public static void ReloadAssign()
        {
            assignDescription = null;
            if (!String.IsNullOrEmpty(Setting.Instance.KeyAssignProperty.KeyAssignFile) &&
                Setting.Instance.KeyAssignProperty.KeyAssignFile != KeyAssignProperty.DefaultAssignFileName)
            {
                try
                {
                    assignDescription = AssignLoader.LoadAssign(KeyAssignHelper.GetPath(Setting.Instance.KeyAssignProperty.KeyAssignFile));
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
                    assignDescription = AssignLoader.LoadAssign(KeyAssignHelper.GetPath(KeyAssignProperty.DefaultAssignFileName));
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

        /// <summary>
        /// キー表現を判別可能な文字列に変換します。
        /// </summary>
        public static String KeyToString(ModifierKeys modkeys, Key key)
        {
            String ret = key.ToString();
            if (modkeys.HasFlag(ModifierKeys.Shift))
                ret = "Shift+" + ret;
            if (modkeys.HasFlag(ModifierKeys.Windows))
                ret = "Win+" + ret;
            if (modkeys.HasFlag(ModifierKeys.Alt))
                ret = "Alt+" + ret;
            if (modkeys.HasFlag(ModifierKeys.Control))
                ret = "Control+" + ret;
            return ret;
        }

        /// <summary>
        /// KeyDown Eventを通知します。
        /// </summary>
        public static void HandlePreviewEvent(KeyEventArgs e, AssignRegion region)
        {
            if (HandleEventSink(CheckIme(e), region, true, IsSourceFromTextBox(e)))
                e.Handled = true;
        }

        /// <summary>
        /// PreviewKeyDown Eventを通知します。
        /// </summary>
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
                // clear
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
                    .Select(a => new Tuple<string, string>(a.ActionId, a.Argument))
                    .Dispatch();
            }
            catch (Exception ex)
            {
                ExceptionStorage.Register(ex, ExceptionCategory.ConfigurationError,
                    "キーアサインを処理中にエラーが発生しました :" + ex.Message);
                return false;
            }
        }

        private static bool Dispatch(this IEnumerable<Tuple<string, string>> actions)
        {
            bool dispatched = false;
            foreach (var ad in actions)
            {
                Action<string> action;
                if (callbacks.TryGetValue(ad.Item1, out action))
                    action(ad.Item2);
                else
                    throw new ArgumentException("オペレーションID \"" + ad + "\" は見つかりませんでした。");
                dispatched = true;
            }
            return dispatched;
        }

        /// <summary>
        /// 指定されたキーアサインIDを持つアクションが登録されているか確認します。
        /// </summary>
        /// <param name="id">キーアサインID</param>
        public static bool ExistsAction(string id)
        {
            return callbacks.ContainsKey(id);
        }
    }
}
