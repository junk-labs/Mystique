using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using Inscribe.Common;
using Inscribe.Configuration;
using Inscribe.Configuration.Settings;
using Inscribe.Core;
using Inscribe.Filter.Filters.Text;
using Inscribe.Plugin;
using Inscribe.Storage;
using Mystique.Views.Common;
using Mystique.Views.Text;

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
                    return TextToFlowConversionStatic.Generate(input);
                case InlineConversionMode.Digest:
                    return TextToFlowConversionStatic.GenerateDigest(input);
                default:
                    return TextToFlowConversionStatic.Generate(input);
            }
        }
    }

    public static class TextToFlowConversionStatic
    {
        /// <summary>
        /// 文字をリッチテキストフォーマットして返します。
        /// </summary>
        public static IEnumerable<Inline> Generate(this string text)
        {
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
                        var atlink = new Hyperlink(new Run(ctt));
                        atlink.PreviewMouseLeftButtonDown += (o, e) =>
                        {
                            e.Handled = true;
                            InternalLinkClicked(InternalLinkKind.User, ctt);
                        };
                        yield return atlink;
                        break;
                    case TokenKind.Hashtag:
                        var hashlink = new Hyperlink(new Run(ctt));
                        hashlink.PreviewMouseLeftButtonDown += (o, e) =>
                        {
                            e.Handled = true;
                            InternalLinkClicked(InternalLinkKind.Hash, ctt);
                        };
                        yield return hashlink;
                        break;
                    case TokenKind.Url:
                        var urllink = new Hyperlink();
                        switch (Setting.Instance.TweetExperienceProperty.UrlResolveMode)
                        {
                            case UrlResolve.OnPointed:
                            case UrlResolve.Never:
                                urllink.Inlines.Add(new Run(ctt));
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
                                    urllink.Inlines.Add(new Run(ctt));
                                    urllink.ToolTip = new UrlTooltip(ctt);
                                }
                                else
                                {
                                    urllink.Inlines.Add(new Run(nurl));
                                    urllink.ToolTip = new UrlTooltip(nurl);
                                }
                                break;
                        }
                        ToolTipService.SetShowDuration(urllink, Setting.Instance.TweetExperienceProperty.UrlTooltipShowLength);
                        urllink.PreviewMouseLeftButtonDown += (o, e) =>
                        {
                            e.Handled = true;
                            OuterLinkClicked(ctt);
                        };
                        yield return urllink;
                        break;
                    case TokenKind.Text:
                    default:
                        yield return new Run(ctt);
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
                        var atlink = new Hyperlink(new Run(tok.Text));
                        atlink.MouseLeftButtonDown += (o, e) => InternalLinkClicked(InternalLinkKind.User, tok.Text);
                        yield return atlink;
                        break;
                    case TokenKind.Hashtag:
                        var hashlink = new Hyperlink(new Run(tok.Text));
                        hashlink.MouseLeftButtonDown += (o, e) => InternalLinkClicked(InternalLinkKind.Hash, tok.Text);
                        yield return hashlink;
                        break;
                    case TokenKind.Url:
                        var urllink = new Hyperlink(new Run(tok.Text));
                        urllink.MouseLeftButtonDown += (o, e) => OuterLinkClicked(tok.Text);
                        yield return urllink;
                        break;
                    case TokenKind.Text:
                    default:
                        yield return new Run(tok.Text);
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
                        yield return new Action(() =>
                        {
                            OuterLinkClicked(ctt);
                        });
                        break;
                }
            }
        }

        enum InternalLinkKind
        {
            User,
            Hash
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
                    System.Diagnostics.Debug.WriteLine("Extracting hash:" + source);
                    if (Keyboard.GetKeyStates(Key.LeftCtrl) == KeyStates.Down || Keyboard.GetKeyStates(Key.RightCtrl) == KeyStates.Down)
                    {
                        Browser.Start("http://twitter.com/#search?q=" + source);
                    }
                    else
                    {
                        if (KernelService.MainWindowViewModel.ColumnOwnerViewModel.CurrentFocusColumn != null &&
                            KernelService.MainWindowViewModel.ColumnOwnerViewModel.CurrentFocusColumn.SelectedTabViewModel != null)
                            KernelService.MainWindowViewModel.ColumnOwnerViewModel.CurrentFocusColumn
                                .SelectedTabViewModel.AddTopTimeline(new[] { new FilterText(source) });
                    }
                    break;
                default:
                    InvalidLinkClicked("Internal::" + kind.ToString() + "," + source);
                    break;

            }
        }

        static void OuterLinkClicked(string navigate)
        {
            Browser.Start(navigate);
        }

        static void InvalidLinkClicked(string show)
        {
            ExceptionStorage.Register(new Exception("Invalid Link."), ExceptionCategory.InternalError, show);
        }
    }
}