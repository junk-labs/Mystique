using System;
using System.Collections.Generic;
using System.Linq;
using Inscribe.Text;

namespace Mystique.Views.Text
{
    /// <summary>
    /// ユーザーがツイートするテキストをあれこれ変換するやつ
    /// </summary>
    public static class Tokenizer
    {
        private static string Escape(string raw)
        {
            return raw.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;");
        }

        private static string Unescape(string escaped)
        {
            return escaped.Replace("&lt;", "<").Replace("&gt;", ">").Replace("&amp;", "&");
        }


        /// <summary>
        /// 文字列をトークン化します。
        /// </summary>
        /// <param name="escaped">エスケープされた文字列</param>
        /// <returns>トークンの列挙</returns>
        public static IEnumerable<Token> Tokenize(string raw)
        {
            if (String.IsNullOrEmpty(raw)) yield break;
            var escaped = Escape(raw);
            escaped = RegularExpressions.UrlRegex.Replace(escaped, (m) =>
            {
                // # => &sharp; (ハッシュタグで再識別されることを防ぐ)
                var repl = m.Groups[1].Value.Replace("#", "&sharp;");
                return "<U>" + repl + "<";
            });
            escaped = RegularExpressions.AtRegex.Replace(escaped, "<A>@$1<");
            escaped = RegularExpressions.HashRegex.Replace(escaped, (m) =>
            {
                if (m.Groups.Count > 0)
                {
                    return "<H>" + m.Groups[0].Value + "<";
                }
                else
                {
                    return m.Value;
                }
            });
            var splitted = escaped.Split(new[] { '<' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var s in splitted)
            {
                if (s.Contains('>'))
                {
                    var kind = s[0];
                    var body = Unescape(s.Substring(2));
                    switch (kind)
                    {
                        case 'U':
                            // &sharp; => #
                            yield return new Token(TokenKind.Url, body.Replace("&sharp;", "#"));
                            break;
                        case 'A':
                            yield return new Token(TokenKind.AtLink, body);
                            break;
                        case 'H':
                            yield return new Token(TokenKind.Hashtag, body);
                            break;
                        default:
                            throw new InvalidOperationException("無効な分類です:" + kind.ToString());
                    }
                }
                else
                {
                    yield return new Token(TokenKind.Text, Unescape(s));
                }
            }
        }
    }

    /// <summary>
    /// 文字トークン
    /// </summary>
    public class Token
    {


        public Token(TokenKind tknd, string tkstr)
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
        ImageInsertionLink
    }
}
