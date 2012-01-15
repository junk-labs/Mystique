using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Acuerdo.Plugin;
using Dulcet.Twitter;
using Dulcet.Twitter.Rest;
using Inscribe.Common;
using Inscribe.Storage;
using Inscribe.Text;
using Inscribe.ViewModels.PartBlocks.MainBlock;
using Livet;

namespace MapleMagic
{
    [Export(typeof(IPlugin))]
    public class EntryPoint : IPlugin
    {
        public string Name
        {
            get { return "Maple Magic plugin"; }
        }

        public Version Version
        {
            get { return new Version(0, 0, 0, 0); }
        }

        public void Loaded()
        {
            EventStorage.EventRegistered += new EventHandler<EventDescriptionEventArgs>(EventStorage_EventRegistered);
        }

        public static T SafeExec<T>(Func<T> import)
        {
            try
            {
                return import();
            }
            catch (Exception ex)
            {
                ExceptionStorage.Register(ex, ExceptionCategory.PluginError);
                return default(T);
            }
        }

        void EventStorage_EventRegistered(object sender, EventDescriptionEventArgs e)
        {
            Task.Factory.StartNew(() =>
            {
                switch (e.EventDescription.Kind)
                {
                    case EventKind.Follow:
                        CheckFollower(e.EventDescription.SourceUser);
                        break;
                }
            });
        }

        private void CheckFollower(UserViewModel userViewModel)
        {
            // 自分からのフォローなら無視
            if (AccountStorage.Contains(userViewModel.TwitterUser.NumericId) ||
                AccountStorage.Contains(userViewModel.TwitterUser.ScreenName))
                return;
            // 自分がフォローしている相手なら無視
            if (TwitterHelper.IsFollowingAny(userViewModel))
                return;
            // 直近ツイートを取得
            var ai = AccountStorage.GetRandom(i => i.IsFollowedBy(userViewModel.TwitterUser.NumericId), true);
            if (ai == null) return;
            var tl = ai.GetUserTimeline(userId: userViewModel.TwitterUser.NumericId, count: 50);
            if (tl == null) return;
            var checkresult = GetRules()
                .Where(r => r.Check(userViewModel, tl))
                .FirstOrDefault();
            if (checkresult != null)
            {
                ShowR4SCandidateDialog(userViewModel, checkresult.Description);
            }
        }

        public void ShowR4SCandidateDialog(UserViewModel userViewModel, string description)
        {
            DispatcherHelper.BeginInvoke(() =>
            {
                var r4svm = new R4SDialogViewModel(userViewModel, description);
                var r4s = new R4SDialog();
                r4s.DataContext = r4svm;
                r4s.Owner = Application.Current.MainWindow;
                r4s.ShowDialog();
            });
        }

        public IEnumerable<AutoSpamRule> GetRules()
        {
            // 直近のツイートが全て twittbot.net からの投稿、もしくは
            // 直近のツイートの8割以上にURLがある、もしくは
            // 直近のツイートの8割以上に"RT"がある
            yield return new AutoSpamRule("直近50ツイートの8割以上がtwittbot.netからの投稿", (u, t) =>
            {
                var tla = t.OfType<TwitterStatus>().ToArray();
                return tla.Select(s => s.Source)
                    .Where(s => s.Contains("twittbot.net")).Count() * 100 / tla.Count() >= 80;
            });
            yield return new AutoSpamRule("直近50ツイートの8割以上にURLが含まれる", (u, t) =>
            {
                var tla = t.OfType<TwitterStatus>().ToArray();
                return tla.Select(s => s.Text)
                    .Where(s => RegularExpressions.UrlRegex.IsMatch(s)).Count() * 100 / tla.Count() >= 80;
            });
            yield return new AutoSpamRule("直近50ツイートの8割以上に\"RT\"が含まれる", (u, t) =>
            {
                var tla = t.OfType<TwitterStatus>().ToArray();
                return tla.Select(s => s.Text)
                    .Where(s => s.Contains("RT")).Count() * 100 / tla.Count() >= 80;
            });
        }

        public IConfigurator ConfigurationInterface
        {
            get { return null; }
        }
    }

    // funcはtrueならスパム判定
    public class AutoSpamRule
    {
        public string Description { get; set; }

        public Func<UserViewModel, IEnumerable<TwitterStatusBase>, bool> Check { get; set; }

        public AutoSpamRule(string desc, Func<UserViewModel, IEnumerable<TwitterStatusBase>, bool> func)
        {
            this.Description = desc;
            this.Check = func;
        }
    }
}
