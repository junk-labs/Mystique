using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text.RegularExpressions;
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
using Inscribe.Filter.Filters.Text;

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
            get { return new Version(0, 6, 0, 0); }
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
                        CheckFollower(e.EventDescription.SourceUser, e.EventDescription.TargetUser.TwitterUser.ScreenName);
                        break;
                }
            });
        }

        private void CheckFollower(UserViewModel userViewModel, string receiver)
        {
            try
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
                    ShowR4SCandidateDialog(userViewModel, receiver, checkresult.Description);
                }
            }
            catch (Exception ex)
            {
                ExceptionStorage.Register(ex, ExceptionCategory.TwitterError, "MapleMagicでの照合に失敗しました。", () => CheckFollower(userViewModel, receiver));
            }
        }

        public void ShowR4SCandidateDialog(UserViewModel userViewModel, string receiver, string description)
        {
            DispatcherHelper.BeginInvoke(() =>
            {
                var r4svm = new R4SDialogViewModel(userViewModel, receiver, description);
                var r4s = new R4SDialog();
                r4s.DataContext = r4svm;
                r4s.Owner = Application.Current.MainWindow;
                r4s.ShowDialog();
            });
        }

        public IEnumerable<AutoSpamRule> GetRules()
        {
            yield return new AutoSpamRule("Bioにひらがなが含まれない", (u, t) =>
                !String.IsNullOrEmpty(u.TwitterUser.Bio) && !Regex.IsMatch(u.TwitterUser.Bio, @".\p{IsHiragana}", RegexOptions.Compiled));
            yield return new AutoSpamRule("ツイートが3件以下", (u, t) => u.TwitterUser.Tweets < 3);
            yield return new AutoSpamRule("お気に入りが0個", (u, t) => u.TwitterUser.Favorites == 0);
            yield return new AutoSpamRule("直近50ツイートの8割以上がtwittbot.netからの投稿", (u, t) =>
            {
                var tla = t.OfType<TwitterStatus>().ToArray();
                return tla.Select(s => s.Source)
                    .Where(s => s.Contains("twittbot.net")).Count() > tla.Count() * 0.8;
            });
            yield return new AutoSpamRule("直近50ツイートの8割以上がtwitterfeedからの投稿", (u, t) =>
            {
                var tla = t.OfType<TwitterStatus>().ToArray();
                return tla.Select(s => s.Source)
                    .Where(s => s.Contains("twitterfeed")).Count() > tla.Count() * 0.8;
            });
            yield return new AutoSpamRule("直近50ツイートの8割以上にURLが含まれる", (u, t) =>
            {
                var tla = t.OfType<TwitterStatus>().ToArray();
                return tla.Select(s => s.Text)
                    .Where(s => RegularExpressions.UrlRegex.IsMatch(s)).Count() > tla.Count() * 0.8;
            });
            yield return new AutoSpamRule("直近50ツイートの8割以上に\"RT\"が含まれる", (u, t) =>
            {
                var tla = t.OfType<TwitterStatus>().ToArray();
                return tla.Select(s => s.Text)
                    .Where(s => s.Contains("RT")).Count() > tla.Count() * 0.8;
            });
            yield return new AutoSpamRule("直近50ツイート全てに@mentionが含まれる", (u, t) =>
                 t.OfType<TwitterStatus>().Select(s => s.Text).Where(s => !s.Contains("@")).Count() == 0);
            var spamtools = new string[] { "burgertweet", "http://www.twisuke.com/" };
            yield return new AutoSpamRule("知られているスパムツールからのツイートが含まれる", (u, t) =>
                t.OfType<TwitterStatus>().Select(s => s.Source).Any(s => spamtools.Any(tool => s.ContainsIgnoreCase(tool))));
            yield return new AutoSpamRule("botっぽいツールを使っている", (u, t) =>
                t.OfType<TwitterStatus>().Select(s => s.Source).Any(s => !s.ContainsIgnoreCase("tweetbot") && s.ContainsIgnoreCase("bot")));
            yield return new AutoSpamRule("直近50ツイートのうち、goo.gl/amazon/amzn.toを含むツイートが1割以上含まれる", (u, t) =>
            {
                var tla = t.OfType<TwitterStatus>().ToArray();
                return tla.Select(s => s.Text)
                    .Where(s => s.ContainsIgnoreCase("goo.gl")).Count() > tla.Count() * 0.1;
            });
            yield return new AutoSpamRule("直近50ツイートの1割以上に #followmejp などのフォロー募集系タグを使っている", (u, t) =>
            {
                var tla = t.OfType<TwitterStatus>().ToArray();
                return tla.Select(s => s.Text)
                    .Where(s =>
                        s.ContainsIgnoreCase("#followmejp") ||
                        s.ContainsIgnoreCase("#sougofollow") ||
                        s.ContainsIgnoreCase("#followdaibosyu") ||
                        s.ContainsIgnoreCase("#followdaiboshu") ||
                        s.ContainsIgnoreCase("#goen") ||
                        s.ContainsIgnoreCase("#autofollow"))
                        .Count() > tla.Count() * 0.1;
            });
            yield return new AutoSpamRule("Krileでまだ受信したことのないクライアントからのツイートが含まれる", (u, t) =>
                t.OfType<TwitterStatus>()
                    .Select(tw => tw.Source)
                    .Distinct()
                    .Select(s => new FilterVia(s))
                    .Any(f => TweetStorage.GetAll(tw => f.Filter(tw.Status))
                        .Count() == 0));
        }

        public IConfigurator ConfigurationInterface
        {
            get { return null; }
        }
    }

    public static class StringExtension
    {
        public static bool ContainsIgnoreCase(this String haystack, String needle)
        {
            return haystack.IndexOf(needle, StringComparison.CurrentCultureIgnoreCase) != -1;
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
