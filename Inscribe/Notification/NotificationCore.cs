using System;
using System.IO;
using System.Media;
using System.Threading.Tasks;
using System.Windows;
using Inscribe.Configuration;
using Inscribe.Storage;
using Inscribe.ViewModels;
using Inscribe.ViewModels.PartBlocks.MainBlock;
using Livet;
using Livet.Messaging;
using Inscribe.ViewModels.PartBlocks.MainBlock.TimelineChild;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace Inscribe.Notification
{
    /// <summary>
    /// ポップアップ通知制御サブシステム
    /// </summary>
    public static class NotificationCore
    {
        public static void Initialize()
        {
            EventStorage.EventRegistered += new EventHandler<EventDescriptionEventArgs>(EventStorage_EventRegistered);
        }

        private static void EventStorage_EventRegistered(object sender, EventDescriptionEventArgs e)
        {
            switch (e.EventDescription.Kind)
            {
                case EventKind.Favorite:
                    IssueNotification(
                        e.EventDescription.SourceUser,
                        e.EventDescription.TargetUser,
                        e.EventDescription.TargetTweet.Text, EventKind.Favorite);
                    break;
                case EventKind.Follow:
                    IssueNotification(
                        e.EventDescription.SourceUser,
                        e.EventDescription.TargetUser,
                        e.EventDescription.SourceUser.TwitterUser.Bio,
                        EventKind.Follow);
                    break;
                case EventKind.Mention:
                    IssueNotification(
                        e.EventDescription.SourceUser,
                        e.EventDescription.TargetUser,
                        e.EventDescription.TargetTweet.Text,
                        EventKind.Mention);
                    break;
                case EventKind.Retweet:
                    IssueNotification(
                        e.EventDescription.SourceUser,
                        e.EventDescription.TargetUser,
                        e.EventDescription.TargetTweet.Text,
                        EventKind.Retweet);
                    break;
                case EventKind.Unfavorite:
                    IssueNotification(
                        e.EventDescription.SourceUser,
                        e.EventDescription.TargetUser,
                        e.EventDescription.TargetTweet.Text, EventKind.Unfavorite);
                    break;
            }
        }

        private static object waitingsLocker = new object();

        private static Dictionary<TweetViewModel, string> waitings = new Dictionary<TweetViewModel, string>();

        public static void RegisterNotify(TweetViewModel tweet)
        {
            lock (waitingsLocker)
            {
                waitings.Add(tweet, null);
            }
        }

        public static void QueueNotify(TweetViewModel tweet, string soundSource = null)
        {
            if (tweet.CreatedAt < DateTime.Now.Subtract(TimeSpan.FromMinutes(10)))
                return;
            if (soundSource == null)
                soundSource = String.Empty;
            lock (waitingsLocker)
            {
                if (waitings.ContainsKey(tweet))
                    waitings[tweet] = soundSource;
            }
        }

        public static void DispatchNotify(TweetViewModel tweet)
        {
            if (tweet.CreatedAt < DateTime.Now.Subtract(TimeSpan.FromMinutes(10)))
                return;
            lock (waitingsLocker)
            {
                if (waitings.ContainsKey(tweet))
                {
                    var notify = waitings[tweet];
                    waitings.Remove(tweet);
                    if (notify != null)
                        IssueNotification(UserStorage.Get(tweet.Status.User), null, tweet.Status.Text, EventKind.Undefined, notify);
                }
            }
        }

        /// <summary>
        /// 通知を発行する
        /// </summary>
        private static void IssueNotification(UserViewModel source, UserViewModel target, string text, EventKind eventKind, string soundPath = null)
        {
            if (Setting.Instance.StateProperty.IsInSilentMode) return;
            if (Setting.Instance.NotificationProperty.WindowNotificationStrategy != Configuration.Settings.NotificationStrategy.Disabled)
            {
                DispatcherHelper.BeginInvoke(() =>
                {
                    if (Setting.Instance.NotificationProperty.WindowNotificationStrategy == Configuration.Settings.NotificationStrategy.OnlyInactive &&
                        Application.Current.MainWindow.IsActive)
                        return;
                    var nvm = new NotificationViewModel(
                        Core.KernelService.MainWindowViewModel,
                        source,
                        target,
                        text,
                        eventKind);
                    Core.KernelService.MainWindowViewModel.Messenger.Raise(
                        new TransitionMessage(nvm, TransitionMode.Normal, "Notification"));
                });
            }
            if (Setting.Instance.NotificationProperty.SoundNotificationStrategy != Configuration.Settings.NotificationStrategy.Disabled)
            {
                DispatcherHelper.BeginInvoke(() =>
                {
                    if (Setting.Instance.NotificationProperty.SoundNotificationStrategy == Configuration.Settings.NotificationStrategy.OnlyInactive &&
                        Application.Current.MainWindow.IsActive)
                        return;
                    PlaySound(eventKind, soundPath);
                }, System.Windows.Threading.DispatcherPriority.Background);
            }
        }

        private static bool isPlaying = false;
        private static void PlaySound(EventKind eventKind, string overrideSoundPath)
        {
            string path = Path.GetDirectoryName(Define.GetExeFilePath());
            path = Path.Combine(path, Define.MediaDirectory);
            switch (eventKind)
            {
                case EventKind.Mention:
                    path = Path.Combine(path, Define.MentionWavFile);
                    break;
                case EventKind.DirectMessage:
                    path = Path.Combine(path, Define.DirectMessageWavFile);
                    break;
                case EventKind.Undefined:
                    path = Path.Combine(path, Define.NewReceiveWavFile);
                    break;
                case EventKind.Follow:
                case EventKind.Favorite:
                case EventKind.Retweet:
                case EventKind.Unfavorite:
                    path = Path.Combine(path, Define.EventWavFile);
                    break;
                default:
                    return;
            }
            if (!String.IsNullOrEmpty(overrideSoundPath) &&
                File.Exists(overrideSoundPath))
            {
                path = overrideSoundPath;
            }
            if (File.Exists(path))
            {
                Task.Factory.StartNew(() =>
                {
                    if (isPlaying) return;
                    isPlaying = true;
                    using (var soundplayer = new SoundPlayer(path))
                    {
                        soundplayer.PlaySync();
                    }
                    isPlaying = false;
                });
            }
        }
    }
}