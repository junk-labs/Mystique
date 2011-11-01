using System;
using System.Collections.Generic;
using System.Linq;
using Dulcet.Twitter;
using Inscribe.Authentication;
using Inscribe.Communication.Posting;
using Inscribe.Configuration.Settings;
using Inscribe.Core;
using Inscribe.Storage;
using Inscribe.ViewModels.PartBlocks.MainBlock.TimelineChild;
using Inscribe.Configuration;

namespace Inscribe.Subsystems
{
    public static class MouseAssignCore
    {
        /// <summary>
        /// アクションを実行します。
        /// </summary>
        /// <param name="target">対象ターゲット</param>
        /// <param name="action">アクション。Nullの場合は何も実行されません。</param>
        public static void ExecuteAction<T>(TweetViewModel target, ActionDescription<T> action)
        {
            if (action == null) return;
            ExecuteAction(target, (dynamic)action.Action, action.ActionArgs);
        }

        public static void ExecuteAction(TweetViewModel target, ReplyMouseActionCandidates actionKind, string argument)
        {
            switch (actionKind)
            {
                case ReplyMouseActionCandidates.Reply:
                case ReplyMouseActionCandidates.ReplyFromSpecificAccount:
                    KernelService.MainWindowViewModel.InputBlockViewModel.SetOpenText(true, true);
                    KernelService.MainWindowViewModel.InputBlockViewModel.SetInReplyTo(target);
                    if (actionKind == ReplyMouseActionCandidates.ReplyFromSpecificAccount &&
                        !String.IsNullOrEmpty(argument))
                        KernelService.MainWindowViewModel.InputBlockViewModel.OverrideTarget(
                            GetAccountInfos(argument));
                    break;
                case ReplyMouseActionCandidates.ReplyImmediately:
                    if (String.IsNullOrEmpty(argument)) return;
                    var status = target.Status;
                    if (!Setting.Instance.InputExperienceProperty.OfficialRetweetInReplyToRetweeter && status is TwitterStatus &&
                        ((TwitterStatus)status).RetweetedOriginal != null)
                        status = ((TwitterStatus)status).RetweetedOriginal;
                    PostImmediate(GetAccountInfos(),
                        String.Format(argument, "@" + status.User.ScreenName),
                        status.Id);
                    break;
            }
        }

        public static void ExecuteAction(TweetViewModel target, FavMouseActionCandidates actionKind, string argument)
        {
            if (!target.CanFavorite) return;
            var cais = GetAccountInfos();
            var pais = GetAccountInfos(argument);
            switch (actionKind)
            {
                case FavMouseActionCandidates.FavToggle:
                    if (cais.Select(ai => ai.UserViewModel)
                        .All(u => target.FavoredUsers.Contains(u)))
                        PostOffice.UnfavTweet(cais, target);
                    else
                        PostOffice.FavTweet(cais, target);
                    break;
                case FavMouseActionCandidates.FavAdd:
                    PostOffice.FavTweet(cais, target);
                    break;
                case FavMouseActionCandidates.FavRemove:
                    PostOffice.UnfavTweet(cais, target);
                    break;
                case FavMouseActionCandidates.FavSelect:
                    var favored = AccountStorage.Accounts
                        .Where(a => target.FavoredUsers.Contains(a.UserViewModel)).ToArray();
                    KernelService.MainWindowViewModel.SelectUser(
                        ViewModels.PartBlocks.ModalParts.SelectionKind.Favorite,
                        favored,
                        u =>
                        {
                            PostOffice.FavTweet(u.Except(favored), target);
                            PostOffice.UnfavTweet(favored.Except(u), target);
                        });
                    break;
                case FavMouseActionCandidates.FavToggleWithSpecificAccount:
                    if (pais.Select(ai => ai.UserViewModel)
                        .All(u => target.FavoredUsers.Contains(u)))
                        PostOffice.UnfavTweet(pais, target);
                    else
                        PostOffice.FavTweet(pais, target);
                    break;
                case FavMouseActionCandidates.FavAddWithSpecificAccount:
                    PostOffice.FavTweet(pais, target);
                    break;
                case FavMouseActionCandidates.FavRemoveWithSpecificAccount:
                    PostOffice.UnfavTweet(pais, target);
                    break;
                case FavMouseActionCandidates.FavAddWithAllAccount:
                    PostOffice.FavTweet(AccountStorage.Accounts, target);
                    break;
                case FavMouseActionCandidates.FavRemoveWithAllAccount:
                    PostOffice.UnfavTweet(AccountStorage.Accounts, target);
                    break;
            }
        }

        public static void ExecuteAction(TweetViewModel target, RetweetMouseActionCandidates actionKind, string argument)
        {
            if (target.Status is TwitterDirectMessage) return;
            var cais = GetAccountInfos();
            var pais = GetAccountInfos(argument);
            switch (actionKind)
            {
                case RetweetMouseActionCandidates.RetweetToggle:
                    if (cais.Select(ai => ai.UserViewModel)
                        .All(u => target.RetweetedUsers.Contains(u)))
                        PostOffice.Unretweet(cais, target);
                    else
                        PostOffice.Retweet(cais, target);
                    break;
                case RetweetMouseActionCandidates.RetweetCreate:
                    PostOffice.Retweet(cais, target);
                    break;
                case RetweetMouseActionCandidates.RetweetDelete:
                    PostOffice.Unretweet(cais, target);
                    break;
                case RetweetMouseActionCandidates.RetweetSelect:
                    var retweeted = AccountStorage.Accounts.Where(a => target.RetweetedUsers.Contains(a.UserViewModel)).ToArray();
                    KernelService.MainWindowViewModel.SelectUser(ViewModels.PartBlocks.ModalParts.SelectionKind.Retweet,
                        retweeted,
                        u =>
                        {
                            PostOffice.Retweet(u.Except(retweeted), target);
                            PostOffice.Unretweet(retweeted.Except(u), target);
                        });
                    break;
                case RetweetMouseActionCandidates.RetweetToggleWithSpecificAccount:
                    if (pais.Select(ai => ai.UserViewModel)
                        .All(u => target.RetweetedUsers.Contains(u)))
                        PostOffice.Unretweet(pais, target);
                    else
                        PostOffice.Retweet(pais, target);
                    break;
                case RetweetMouseActionCandidates.RetweetCreateWithSpecificAccount:
                    PostOffice.Retweet(pais, target);
                    break;
                case RetweetMouseActionCandidates.RetweetDeleteWithSpecificAccount:
                    PostOffice.Unretweet(pais, target);
                    break;
                case RetweetMouseActionCandidates.RetweetCreateWithAllAccount:
                    PostOffice.Retweet(AccountStorage.Accounts, target);
                    break;
                case RetweetMouseActionCandidates.RetweetDeleteWithAllAccount:
                    PostOffice.Unretweet(AccountStorage.Accounts, target);
                    break;
            }
        }

        public static void ExecuteAction(TweetViewModel target, UnofficialRetweetQuoteMouseActionCandidates actionKind, string argument)
        {
            if (target.Status is TwitterDirectMessage) return;
            var status = target.Status;
            if (status is TwitterStatus && ((TwitterStatus)status).RetweetedOriginal != null)
                status = ((TwitterStatus)status).RetweetedOriginal;
            switch (actionKind)
            {
                case UnofficialRetweetQuoteMouseActionCandidates.DefaultUnofficialRetweet:
                    KernelService.MainWindowViewModel.InputBlockViewModel.SetOpenText(true, true);
                    KernelService.MainWindowViewModel.InputBlockViewModel.SetText(" RT @" + status.User.ScreenName + ": " + status.Text);
                    KernelService.MainWindowViewModel.InputBlockViewModel.SetInputCaretIndex(0);
                    break;
                case UnofficialRetweetQuoteMouseActionCandidates.DefaultQuoteTweet:
                    KernelService.MainWindowViewModel.InputBlockViewModel.SetOpenText(true, true);
                    KernelService.MainWindowViewModel.InputBlockViewModel.SetInReplyTo(null);
                    KernelService.MainWindowViewModel.InputBlockViewModel.SetInReplyTo(target);
                    KernelService.MainWindowViewModel.InputBlockViewModel.SetText(" QT @" + status.User.ScreenName + ": " + status.Text);
                    KernelService.MainWindowViewModel.InputBlockViewModel.SetInputCaretIndex(0);
                    break;
                case UnofficialRetweetQuoteMouseActionCandidates.CustomUnofficialRetweet:
                    KernelService.MainWindowViewModel.InputBlockViewModel.SetOpenText(true, true);
                    KernelService.MainWindowViewModel.InputBlockViewModel.SetText(String.Format(argument, "@" + status.User.ScreenName, status.Text));
                    KernelService.MainWindowViewModel.InputBlockViewModel.SetInputCaretIndex(0);
                    break;
                case UnofficialRetweetQuoteMouseActionCandidates.CustomQuoteTweet:
                    KernelService.MainWindowViewModel.InputBlockViewModel.SetOpenText(true, true);
                    KernelService.MainWindowViewModel.InputBlockViewModel.SetInReplyTo(null);
                    KernelService.MainWindowViewModel.InputBlockViewModel.SetInReplyTo(target);
                    KernelService.MainWindowViewModel.InputBlockViewModel.SetText(String.Format(argument, "@" + status.User.ScreenName, status.Text));
                    KernelService.MainWindowViewModel.InputBlockViewModel.SetInputCaretIndex(0);
                    break;
                case UnofficialRetweetQuoteMouseActionCandidates.CustomUnofficialRetweetImmediately:
                    PostImmediate(GetAccountInfos(),
                        String.Format(argument, "@" + status.User.ScreenName, status.Text));
                    break;
                case UnofficialRetweetQuoteMouseActionCandidates.CustomQuoteTweetImmediately:
                    PostImmediate(GetAccountInfos(),
                        String.Format(argument, "@" + status.User.ScreenName, status.Text),
                        status.Id);
                    break;
            }
        }

        private static IEnumerable<AccountInfo> GetAccountInfos()
        {
            var ctab = KernelService.MainWindowViewModel.ColumnOwnerViewModel.CurrentTab;
            if (ctab == null)
                return new AccountInfo[0];
            else
                return ctab.TabProperty.LinkAccountInfos;
        }

        private static IEnumerable<AccountInfo> GetAccountInfos(string argument)
        {
            if (argument == null) return new AccountInfo[0];
            return argument.Split(',').Select(s => AccountStorage.Get(s.Trim()))
                            .Where(i => i != null)
                            .Distinct();
        }

        private static void PostImmediate(IEnumerable<AccountInfo> infos, string body, long inReplyToId = 0)
        {
            infos.AsParallel().ForAll(i =>
            KernelService.MainWindowViewModel.InputBlockViewModel.AddUpdateWorker(
                new ViewModels.PartBlocks.InputBlock.TweetWorker(
                    KernelService.MainWindowViewModel.InputBlockViewModel,
                    i, body,
                    inReplyToId,
                    null, new string[0])));

        }
    }
}
