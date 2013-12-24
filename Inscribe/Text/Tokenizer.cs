using System;
using System.Collections.Generic;
using System.Linq;
using Inscribe.Text;
using Inscribe.Anomaly.Utils;
using Inscribe.Helpers;

namespace Inscribe.Text
{
    /// <summary>
    /// ユーザーがツイートするテキストをあれこれ変換するやつ
    /// </summary>
    public static class Tokenizer
    {
        /// <summary>
        /// 文字列をトークン化します。
        /// </summary>
        /// <param name="escaped">エスケープされた文字列</param>
        /// <returns>トークンの列挙</returns>
        public static IEnumerable<TextToken> Tokenize(string raw)
        {
            if (String.IsNullOrEmpty(raw)) yield break;
            var escaped = ParsingExtension.EscapeEntity(raw);
            escaped = TwitterRegexPatterns.ValidUrl.Replace(escaped, m =>
            {
                // # => &sharp; (ハッシュタグで再識別されることを防ぐ)
                var repl = m.Groups[TwitterRegexPatterns.ValidUrlGroupUrl].Value.Replace("#", "&sharp;");
                return m.Groups[TwitterRegexPatterns.ValidUrlGroupBefore] + "<U>" + repl + "<";
            });
            escaped = TwitterRegexPatterns.ValidMentionOrList.Replace(
                escaped,
                m => m.Groups[TwitterRegexPatterns.ValidMentionOrListGroupBefore].Value +
                     "<A>" + m.Groups[TwitterRegexPatterns.ValidMentionOrListGroupAt].Value +
                     m.Groups[TwitterRegexPatterns.ValidMentionOrListGroupUsername].Value +
                     m.Groups[TwitterRegexPatterns.ValidMentionOrListGroupList].Value + "<");
            escaped = TwitterRegexPatterns.ValidHashtag.Replace(
                escaped,
                m => m.Groups[TwitterRegexPatterns.ValidHashtagGroupBefore] + 
                    "<H>" + m.Groups[TwitterRegexPatterns.ValidHashtagGroupHash].Value + 
                    m.Groups[TwitterRegexPatterns.ValidHashtagGroupTag].Value + "<");
            var splitted = escaped.Split(new[] { '<' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var s in splitted)
            {
                if (s.Contains('>'))
                {
                    var kind = s[0];
                    var body = ParsingExtension.ResolveEntity(s.Substring(2));
                    switch (kind)
                    {
                        case 'U':
                            // &sharp; => #
                            yield return new TextToken(TokenKind.Url, body.Replace("&sharp;", "#"));
                            break;
                        case 'A':
                            yield return new TextToken(TokenKind.AtLink, body);
                            break;
                        case 'H':
                            yield return new TextToken(TokenKind.Hashtag, body);
                            break;
                        default:
                            throw new InvalidOperationException("invalid grouping:" + kind.ToString());
                    }
                }
                else
                {
                    yield return new TextToken(TokenKind.Text, ParsingExtension.ResolveEntity(s));
                }
            }
        }
    }

    /// <summary>
    /// 文字トークン
    /// </summary>
    public class TextToken
    {


        public TextToken(TokenKind tknd, string tkstr)
        {
            Kind = tknd;
            Text = tkstr;
        }

        public TokenKind Kind { get; set; }

        public string Text { get; set; }
    }

    /// <summary>
    /// トークン種別
    /// </summary>
    public enum TokenKind
    {
        Text,
        Url,
        Hashtag,
        AtLink,
    }
}
