using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using Inscribe.Common;
using Inscribe.Configuration;
using Inscribe.Configuration.Settings;
using Inscribe.Plugin;
using Mystique.Views.Text;
using Inscribe.Caching;
using Mystique.Views.Common;
using Inscribe.Core;
using Inscribe.Filter.Filters.Text;

namespace Mystique.Views.Converters.Particular
{
    public enum FlowDocumentConversion
    {
        Full,
        Digest,
        Source,
    }

    public class TextToFlowDocumentConverter : OneWayConverter<string, IEnumerable<Inline>>
    {
        public override IEnumerable<Inline> ToTarget(string input, object parameter)
        {
            FlowDocumentConversion kind;
            if (!Enum.TryParse(parameter as string, true, out kind))
                kind = FlowDocumentConversion.Full;
            switch (kind)
            {
                case FlowDocumentConversion.Full:
                    return TextToFlowConversionStatic.Generate(input);

                case FlowDocumentConversion.Digest:
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
            foreach (var tok in Tokenizer.Tokenize(text))
            {
                var ctt = tok.Text;
                switch (tok.Kind)
                {
                    case Tokenizer.Token.TokenKind.AtLink:
                        var atlink = new Hyperlink(new Run(ctt));
                        atlink.PreviewMouseLeftButtonDown += (o, e) =>
                        {
                            e.Handled = true;
                            InternalLinkClicked(InternalLinkKind.User, ctt);
                        };
                        yield return atlink;
                        break;
                    case Tokenizer.Token.TokenKind.Hashtag:
                        var hashlink = new Hyperlink(new Run(ctt));
                        hashlink.PreviewMouseLeftButtonDown += (o, e) =>
                        {
                            e.Handled = true;
                            InternalLinkClicked(InternalLinkKind.Hash, ctt);
                        };
                        yield return hashlink;
                        break;
                    case Tokenizer.Token.TokenKind.URL:
                        var urllink = new Hyperlink();
                        switch (Setting.Instance.TweetExperienceProperty.UrlResolving)
                        {
                            case TweetExperienceProperty.UrlResolveStrategy.OnPointed:
                            case TweetExperienceProperty.UrlResolveStrategy.Never:
                                urllink.Inlines.Add(new Run(ctt));
                                urllink.ToolTip = new UrlTooltip(ctt);
                                break;
                            case TweetExperienceProperty.UrlResolveStrategy.OnReceived:
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
                    case Tokenizer.Token.TokenKind.Text:
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
            foreach (var tok in Tokenizer.Tokenize(text))
            {
                switch (tok.Kind)
                {
                    case Tokenizer.Token.TokenKind.AtLink:
                        var atlink = new Hyperlink(new Run(tok.Text));
                        atlink.MouseLeftButtonDown += (o, e) => InternalLinkClicked(InternalLinkKind.User, tok.Text);
                        yield return atlink;
                        break;
                    case Tokenizer.Token.TokenKind.Hashtag:
                        var hashlink = new Hyperlink(new Run(tok.Text));
                        hashlink.MouseLeftButtonDown += (o, e) => InternalLinkClicked(InternalLinkKind.Hash, tok.Text);
                        yield return hashlink;
                        break;
                    case Tokenizer.Token.TokenKind.URL:
                        var urllink = new Hyperlink(new Run(tok.Text));
                        urllink.MouseLeftButtonDown += (o, e) => OuterLinkClicked(tok.Text);
                        yield return urllink;
                        break;
                    case Tokenizer.Token.TokenKind.Text:
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
                    case Tokenizer.Token.TokenKind.AtLink:
                        yield return new Action(() =>
                        {
                            InternalLinkClicked(InternalLinkKind.User, ctt);
                        });
                        break;
                    case Tokenizer.Token.TokenKind.Hashtag:
                        yield return new Action(() =>
                        {
                            InternalLinkClicked(InternalLinkKind.Hash, ctt);
                        });
                        break;
                    case Tokenizer.Token.TokenKind.URL:
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

        static void ImageLinkClicked(string navigate)
        {
            Uri uri;
            if (Uri.TryCreate(navigate, UriKind.Absolute, out uri))
            {
                Browser.Start(navigate);
            }
            else
            {
                InvalidLinkClicked(navigate);
            }
        }

        static void InvalidLinkClicked(string show)
        {
            // TODO: Notify error
        }
    }
}