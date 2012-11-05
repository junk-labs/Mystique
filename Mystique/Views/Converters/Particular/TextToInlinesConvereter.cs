using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using Inscribe.Common;
using Inscribe.Configuration;
using Inscribe.Configuration.Settings;
using Inscribe.Core;
using Inscribe.Filter.Filters.Numeric;
using Inscribe.Filter.Filters.Text;
using Inscribe.Plugin;
using Inscribe.Storage;
using Mystique.Views.Common;
using System.Web;
using Inscribe.Text;

namespace Mystique.Views.Converters.Particular
{
    public enum InlineConversionMode
    {
        Full,
        Digest,
        Source,
    }

    public class TextToInlinesConverter : OneWayConverter<string, IEnumerable<Inline>>
    {
        public override IEnumerable<Inline> ToTarget(string input, object parameter)
        {
            InlineConversionMode kind;
            if (!Enum.TryParse(parameter as string, true, out kind))
                kind = InlineConversionMode.Full;
            switch (kind)
            {
                case InlineConversionMode.Full:
                    return TextToFlowConversionStatic.Generate(input).ToArray();
                case InlineConversionMode.Digest:
                    return TextToFlowConversionStatic.GenerateDigest(input).ToArray();
                default:
                    return TextToFlowConversionStatic.Generate(input).ToArray();
            }
        }
    }

    public static class TextToFlowConversionStatic
    {
        private const string TweetUrlRegex = @"^https?://(www\.)?twitter\.com/(#!/)?\w+/status(es)?/(?<id>\d+)/?(\?.*)?$";

        /// <summary>
        /// 文字をリッチテキストフォーマットして返します。
        /// </summary>
        public static IEnumerable<Inline> Generate(this string text)
        {
            System.Diagnostics.Debug.WriteLine("called");
            if (!Application.Current.Dispatcher.CheckAccess())
                return Application.Current.Dispatcher.Invoke((Func<string, IEnumerable<Inline>>)(s => GenerateSink(s).ToArray()),
                    text) as IEnumerable<Inline>;
            else
                return GenerateSink(text);
        }

        private static IEnumerable<Inline> GenerateSink(string text)
        {
            foreach (var tok in Tokenizer.Tokenize(text))
            {
                var ctt = tok.Text;
                switch (tok.Kind)
                {
                    case TokenKind.AtLink:
                        yield return TextElementGenerator.GenerateHyperlink(ctt,
                            () => InternalLinkClicked(InternalLinkKind.User, ctt));
                        break;
                    case TokenKind.Hashtag:
                        var hashlink = TextElementGenerator.GenerateHyperlink(ctt,
                            () => InternalLinkClicked(InternalLinkKind.Hash, ctt));
                        hashlink.ToolTip = new TextBlock(new Run("Ctrlキーを押しながらクリックすると、ブラウザを開いて検索します..."));
                        yield return hashlink;
                        break;
                    case TokenKind.Url:
                        String resolved = null;
                        try
                        {
                            resolved = new Uri(ctt).PunyDecode().OriginalString;
                        }
                        catch (UriFormatException) { }
                        var urllink = Regex.IsMatch(ctt, TweetUrlRegex)
                            ? TextElementGenerator.GenerateHyperlink(
                                String.IsNullOrEmpty(resolved) ? ctt : resolved,
                                () => InternalLinkClicked(InternalLinkKind.Tweet, ctt))
                            : TextElementGenerator.GenerateHyperlink(
                                String.IsNullOrEmpty(resolved) ? ctt : resolved,
                                () => ExternalLinkClicked(ctt));

                        switch (Setting.Instance.TweetExperienceProperty.UrlResolveMode)
                        {
                            case UrlResolve.OnPointed:
                            case UrlResolve.Never:
                                urllink.ToolTip = new UrlTooltip(ctt);
                                break;
                            case UrlResolve.OnReceived:
                                string nurl = null;
                                if ((nurl = UrlResolveCacheStorage.Lookup(ctt)) == null)
                                {
                                    nurl = ShortenManager.Extract(ctt);
                                    if (nurl != ctt)
                                    {
                                        // resolved
                                        UrlResolveCacheStorage.AddResolved(ctt, nurl);
                                    }
                                    else
                                    {
                                        // unresolved
                                        UrlResolveCacheStorage.AddResolved(ctt, ctt);
                                    }
                                }
                                if (String.IsNullOrEmpty(nurl))
                                {
                                    urllink.ToolTip = new UrlTooltip(ctt);
                                }
                                else
                                {
                                    urllink.ToolTip = new UrlTooltip(nurl);
                                }
                                break;
                        }
                        ToolTipService.SetShowDuration(urllink, Setting.Instance.TweetExperienceProperty.UrlTooltipShowLength);
                        yield return urllink;
                        break;
                    case TokenKind.Text:
                    default:
                        yield return TextElementGenerator.GenerateRun(ctt);
                        break;
                }
            }
        }

        /// <summary>
        /// 文字をリッチテキストフォーマットして返します。<para />
        /// URL短縮解決や画像展開を行いません。
        /// </summary>
        public static IEnumerable<Inline> GenerateDigest(this string text)
        {
            if (!Application.Current.Dispatcher.CheckAccess())
                return Application.Current.Dispatcher.Invoke((Func<string, IEnumerable<Inline>>)(s => GenerateDigestSink(s).ToArray()),
                    text) as IEnumerable<Inline>;
            else
                return GenerateDigestSink(text);
        }

        private static IEnumerable<Inline> GenerateDigestSink(this string text)
        {
            foreach (var tok in Tokenizer.Tokenize(text))
            {
                switch (tok.Kind)
                {
                    case TokenKind.AtLink:
                        yield return TextElementGenerator.GenerateHyperlink(tok.Text,
                            () => InternalLinkClicked(InternalLinkKind.User, tok.Text));
                        break;
                    case TokenKind.Hashtag:
                        yield return TextElementGenerator.GenerateHyperlink(tok.Text,
                            () => InternalLinkClicked(InternalLinkKind.Hash, tok.Text));
                        break;
                    case TokenKind.Url:
                        yield return TextElementGenerator.GenerateHyperlink(tok.Text,
                            Regex.IsMatch(tok.Text, TweetUrlRegex)
                                ? new Action(() => InternalLinkClicked(InternalLinkKind.Tweet, tok.Text))
                                : new Action(() => ExternalLinkClicked(tok.Text)));
                        break;
                    case TokenKind.Text:
                    default:
                        yield return TextElementGenerator.GenerateRun(tok.Text);
                        break;
                }
            }
        }

        public static IEnumerable<Action> GenerateActions(this string text)
        {
            foreach (var tok in Tokenizer.Tokenize(text))
            {
                var ctt = tok.Text;
                switch (tok.Kind)
                {
                    case TokenKind.AtLink:
                        yield return new Action(() =>
                        {
                            InternalLinkClicked(InternalLinkKind.User, ctt);
                        });
                        break;
                    case TokenKind.Hashtag:
                        yield return new Action(() =>
                        {
                            InternalLinkClicked(InternalLinkKind.Hash, ctt);
                        });
                        break;
                    case TokenKind.Url:
                        yield return Regex.IsMatch(tok.Text, TweetUrlRegex)
                            ? new Action(() => InternalLinkClicked(InternalLinkKind.Tweet, ctt))
                            : new Action(() => ExternalLinkClicked(ctt));
                        break;
                }
            }
        }

        enum InternalLinkKind
        {
            User,
            Hash,
            Tweet
        }

        static void InternalLinkClicked(InternalLinkKind kind, string source)
        {
            switch (kind)
            {
                case InternalLinkKind.User:
                    if (KernelService.MainWindowViewModel.ColumnOwnerViewModel.CurrentFocusColumn != null &&
                        KernelService.MainWindowViewModel.ColumnOwnerViewModel.CurrentFocusColumn.SelectedTabViewModel != null)
                        KernelService.MainWindowViewModel.ColumnOwnerViewModel.CurrentFocusColumn
                            .SelectedTabViewModel.AddTopUser(source);
                    break;
                case InternalLinkKind.Hash:
                    if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
                    {
                        // Browser.Start("http://twitter.com/#search?q=" + source);
                        Browser.Start("http://twitter.com/search/%23" + source.Replace("#", ""));
                    }
                    else
                    {
                        if (KernelService.MainWindowViewModel.ColumnOwnerViewModel.CurrentFocusColumn != null &&
                            KernelService.MainWindowViewModel.ColumnOwnerViewModel.CurrentFocusColumn.SelectedTabViewModel != null)
                            KernelService.MainWindowViewModel.ColumnOwnerViewModel.CurrentFocusColumn
                                .SelectedTabViewModel.AddTopTimeline(new[] { new FilterText(source) });
                    }
                    break;
                case InternalLinkKind.Tweet:
                    if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
                    {
                        Browser.Start(source);
                    }
                    else
                    {
                        if (KernelService.MainWindowViewModel.ColumnOwnerViewModel.CurrentFocusColumn != null &&
                            KernelService.MainWindowViewModel.ColumnOwnerViewModel.CurrentFocusColumn.SelectedTabViewModel != null)
                            KernelService.MainWindowViewModel.ColumnOwnerViewModel.CurrentFocusColumn
                                .SelectedTabViewModel.AddTopTimeline(new[] { new FilterStatusId(long.Parse(Regex.Match(source, TweetUrlRegex).Groups["id"].ToString()), true) });
                    }
                    break;
                default:
                    InvalidLinkClicked("Internal::" + kind.ToString() + "," + source);
                    break;
            }
        }

        static void ExternalLinkClicked(string navigate)
        {
            Browser.Start(navigate);
        }

        static void InvalidLinkClicked(string show)
        {
            ExceptionStorage.Register(new Exception("Invalid Link."), ExceptionCategory.InternalError, show);
        }
    }
}