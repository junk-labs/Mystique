using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Inscribe.Filter.Core
{
    /// <summary>
    /// Krile Query Version 4
    /// </summary>
    public static class QueryCompiler
    {
        #region Querification

        /// <summary>
        /// クエリ文字をフィルタ化します。
        /// </summary>
        /// <param name="queryString">クエリ文字列</param>
        /// <returns>フィルタクラスタ</returns>
        /// <exception cref="System.ArgumentException">コンバートに失敗しました。</exception>
        public static FilterCluster ToFilter(string queryString)
        {
            // フィルタの構造解析
            if (String.IsNullOrWhiteSpace(queryString)) // カ ラ
                return new FilterCluster();
            // トークン化
            if (!queryString.StartsWith("(")) // 最外殻を追加
                queryString = "(" + queryString + ")";
            System.Diagnostics.Debug.WriteLine(queryString);
            var tokens = Tokenize(queryString);
            var syntaxes = MakeTuples(tokens);
            var filter = GenerateFilter(syntaxes);
            System.Diagnostics.Debug.WriteLine(filter.ToQuery());
            return Optimize(filter);
        }

        #region Tokenize

        struct Token
        {
            public enum TokenType
            {
                /// <summary>
                /// ピリオド
                /// </summary>
                Period,
                /// <summary>
                /// カンマ
                /// </summary>
                Comma,
                /// <summary>
                /// コロン記号
                /// </summary>
                Collon,
                /// <summary>
                /// エクスクラメーション マーク
                /// </summary>
                Exclamation,
                /// <summary>
                /// リテラル(べた書き文字列,数値など)
                /// </summary>
                Literal,
                /// <summary>
                /// &amp;記号
                /// </summary>
                ConcatenatorAnd,
                /// <summary>
                /// |記号
                /// </summary>
                ConcatenatorOr,
                /// <summary>
                /// (記号
                /// </summary>
                OpenBracket,
                /// <summary>
                /// )記号
                /// </summary>
                CloseBracket,
                /// <summary>
                /// 文字列
                /// </summary>
                String,
                /// <summary>
                /// 空白
                /// </summary>
                Space
            }

            /// <summary>
            /// このトークンの種別
            /// </summary>
            public TokenType Type { get; set; }

            /// <summary>
            /// このトークンの値
            /// </summary>
            public string Value { get; set; }

            /// <summary>
            /// デバッグ用のインデックス
            /// </summary>
            public int DebugIndex { get; set; }

            public Token(TokenType type, int debugIndex)
                : this()
            {
                this.Type = type;
                this.Value = null;
                this.DebugIndex = debugIndex;
            }

            public Token(TokenType type, string value, int debugIndex)
                : this()
            {
                this.Type = type;
                this.Value = value;
                this.DebugIndex = debugIndex;
            }

            public override string ToString()
            {
                switch (Type)
                {
                    case TokenType.OpenBracket:
                        return "( [開き括弧]";
                    case TokenType.CloseBracket:
                        return ") [閉じ括弧]";
                    case TokenType.ConcatenatorAnd:
                        return "& [AND結合子]";
                    case TokenType.ConcatenatorOr:
                        return "| [OR結合子]";
                    case TokenType.Literal:
                        return "リテラル (" + (Value == null ? "[null]" : Value) + ")";
                    case TokenType.Period:
                        return ". [ピリオド]";
                    case TokenType.Comma:
                        return ", [カンマ]";
                    case TokenType.Exclamation:
                        return "! [エクスクラメーション]";
                    case TokenType.Space:
                        return "  [スペース]";
                    case TokenType.String:
                        return "文字列 (" + (Value == null ? "[null]" : "\"" + Value + "\"") + ")";
                    default:
                        return "[不明なトークン]";

                }
            }
        }

        private static IEnumerable<Token> Tokenize(string query)
        {
            int strptr = 0;
            do
            {
                switch (query[strptr])
                {
                    case '&':
                        yield return new Token(Token.TokenType.ConcatenatorAnd, strptr);
                        break;
                    case '|':
                        yield return new Token(Token.TokenType.ConcatenatorOr, strptr);
                        break;
                    case '.':
                        yield return new Token(Token.TokenType.Period, strptr);
                        break;
                    case ',':
                        yield return new Token(Token.TokenType.Comma, strptr);
                        break;
                    case ':':
                        yield return new Token(Token.TokenType.Collon, strptr);
                        break;
                    case '!':
                        yield return new Token(Token.TokenType.Exclamation, strptr);
                        break;
                    case '(':
                        yield return new Token(Token.TokenType.OpenBracket, strptr);
                        break;
                    case ')':
                        yield return new Token(Token.TokenType.CloseBracket, strptr);
                        break;
                    case '"':
                        yield return new Token(Token.TokenType.String, GetInQuoteString(query, ref strptr), strptr);
                        break;
                    case ' ':
                    case '\t':
                    case '\r':
                    case '\n':
                        yield return new Token(Token.TokenType.Space, strptr);
                        break;
                    default:
                        int begin = strptr;
                        // 何らかのトークンに出会うまで空回し
                        var tokens = "&|.,:!()\" \t\r\n";
                        do
                        {
                            if (tokens.Contains(query[strptr]))
                            {
                                // トークン化
                                yield return new Token(Token.TokenType.Literal,
                                    query.Substring(begin, strptr - begin), begin);
                                strptr--; // 巻き戻し
                                break;
                            }
                            strptr++;
                        } while (strptr < query.Length);
                        break;
                }
                strptr++;
            } while (strptr < query.Length);
        }

        /// <summary>
        /// iの次の文字列がnであることを確認します。<para />
        /// 適合しない場合にtrueを返します。
        /// </summary>
        private static bool CheckIsNotNext(string q, int i, char n)
        {
            if (i + 1 >= q.Length || q[i] != n)
                return true;
            else
                return false;
        }

        #endregion

        #region Syntax analysis

        abstract class SyntaxTuple { }

        class TokenReader
        {
            List<Token> tokenQueueList;
            int queueCursor = 0;
            public TokenReader(IEnumerable<Token> tokens)
            {
                tokenQueueList = new List<Token>(32);
                foreach (var t in tokens)
                {
                    // スペースを飛ばす
                    if (t.Type == Token.TokenType.Space)
                        continue;
                    tokenQueueList.Add(t);
                }
            }

            /// <summary>
            /// 値を取出し、キューを一つ進めます。
            /// </summary>
            /// <returns></returns>
            public Token Get()
            {
                System.Diagnostics.Debug.WriteLine("read:" + tokenQueueList[queueCursor]);
                return tokenQueueList[queueCursor++];
            }

            /// <summary>
            /// 次のトークンを先読みします。
            /// </summary>
            public Token LookAhead()
            {
                return tokenQueueList[queueCursor];
            }

            /// <summary>
            /// キューを一つ戻します。
            /// </summary>
            public void RewindOne()
            {
                if (queueCursor == 0)
                    throw new InvalidOperationException("トークンリーダーは初期状態まで巻き戻っています。もう戻せません。");
                queueCursor--;
            }

            public bool IsRemainToken
            {
                get { return queueCursor < tokenQueueList.Count; }
            }

            List<string> errorDescriptors = new List<string>();

            /// <summary>
            /// エラー文章を追加します。
            /// </summary>
            /// <param name="errorDescription">エラーの説明</param>
            public void PushError(string errorDescription)
            {
                errorDescriptors.Add(errorDescription);
            }

            /// <summary>
            /// 解析中にエラーが生じたかを報告します。
            /// </summary>
            public bool IsContainErrors
            {
                get { return errorDescriptors.Count > 0; }
            }

            /// <summary>
            /// 解析中に生じたエラーを保持します。
            /// </summary>
            /// <returns>エラー一覧</returns>
            public IEnumerable<string> Errors()
            {
                return errorDescriptors;
            }
        }

        static SyntaxTuple MakeTuples(IEnumerable<Token> tokens)
        {
            var reader = new TokenReader(tokens);
            RootTuple roots = null;
            try
            {
                roots = MakeRoot(ref reader);
            }
            catch (ArgumentException e)
            {
                throw new ArgumentException(e.Message + Environment.NewLine +
                    "他のエラー:" + String.Join(Environment.NewLine, reader.Errors()), e);
            }
            if (reader.IsContainErrors)
            {
                throw new ArgumentException("シンタックス エラー:" + String.Join(Environment.NewLine, reader.Errors()));
            }
            if (reader.IsRemainToken)
            {
                throw new ArgumentException("閉じ括弧が多すぎる可能性があります。(クエリは途中で解析を完了しました) (next:@" + reader.Get().DebugIndex + ")");
            }
            return roots;
        }

        #region Syntax analyzing utils

        /// <summary>
        /// 次に出現するトークンが特定の種類であることを確認し、読みます。<para />
        /// 読めなかった場合はリワインドします。
        /// </summary>
        /// <param name="reader">トークン リーダー</param>
        /// <param name="type">トークンの種類</param>
        private static Token AssertNext(ref TokenReader reader, Token.TokenType type)
        {
            if (!reader.IsRemainToken)
                reader.PushError("クエリが解析の途中で終了しました。" +
                    "ここには " + type.ToString() + " が存在しなければなりません。");
            var ntoken = reader.Get();
            if (ntoken.Type != type)
            {
                reader.PushError("不明な文字です: " + reader.LookAhead() + " (インデックス:" + reader.LookAhead().DebugIndex + ")" +
                    "ここには " + type.ToString() + " が存在しなければなりません。");
                return new Token() { Type = type, Value = null, DebugIndex = -1 };
            }
            return ntoken;
        }

        /// <summary>
        /// 次のトークンを先読みし、指定した型であるかを判定します。
        /// </summary>
        /// <param name="reader">トークン リーダー</param>
        /// <param name="type">先読みするトークンの種類</param>
        /// <param name="trim">読み込みが合致した時にトークンを読み飛ばす</param>
        /// <returns>合致すればtrue</returns>
        private static bool TryLookAhead(ref TokenReader reader, Token.TokenType type, bool trim = true)
        {
            if (!reader.IsRemainToken)
                return false;
            if (reader.LookAhead().Type == type)
            {
                if (trim)
                    reader.Get();
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 次の一つのトークンがどれかに合致するか確認します。
        /// </summary>
        /// <param name="reader">リーダートークン</param>
        /// <param name="isEnd">文末を受け入れるか</param>
        /// <param name="tokens">可能なトークン集合</param>
        private static void AssertNextAny(ref TokenReader reader, bool isEnd, params Token.TokenType[] tokens)
        {
            var descstr = "";
            if (tokens.Count() == 1)
                descstr = "ここには " + tokens.First().ToString() + " が存在しなければなりません。";
            else
                descstr = "ここには " + String.Join(", ", tokens.Select(s => s.ToString())) + " のうちいずれかが存在しなければなりません。";

            if (!reader.IsRemainToken)
            {
                if (isEnd) return;
                reader.PushError("クエリが解析の途中で終了しました。" + descstr);
            }
            if (tokens == null) return;
            var lat = reader.LookAhead().Type;
            if (!tokens.Any(f => f == lat))
            {
                reader.PushError("不明な文字です: " + reader.LookAhead() +
                    " (インデックス:" + reader.LookAhead().DebugIndex + ")" + descstr);
            }
        }

        #endregion


        class RootTuple : SyntaxTuple
        {
            public ClusterTuple Cluster { get; set; }
        }

        private static RootTuple MakeRoot(ref TokenReader reader)
        {
            System.Diagnostics.Debug.WriteLine("MakeRoot");
            var ret = new RootTuple();
            if (reader.IsRemainToken)
            {
                ret.Cluster = MakeCluster(ref reader);
            }
            if (reader.IsRemainToken)
                throw new ArgumentException("クエリが途中で終了しています。閉じ括弧が多すぎる可能性があります。(次のクエリ:" + reader.LookAhead().ToString() +
                    ", インデックス:" + reader.LookAhead().DebugIndex.ToString());
            return ret;
        }

        class ClusterTuple : SyntaxTuple
        {
            public bool Negate { get; set; }

            public InnerClusterTuple InnerCluster { get; set; }
        }

        private static ClusterTuple MakeCluster(ref TokenReader reader)
        {
            System.Diagnostics.Debug.WriteLine("MakeCluster");
            var ctuple = new ClusterTuple();
            ctuple.Negate = TryLookAhead(ref reader, Token.TokenType.Exclamation);
            AssertNext(ref reader, Token.TokenType.OpenBracket);
            if (reader.IsRemainToken && !TryLookAhead(ref reader, Token.TokenType.CloseBracket, false))
            {
                ctuple.InnerCluster = MakeInnerCluster(ref reader);
            }
            AssertNext(ref reader, Token.TokenType.CloseBracket);
            AssertNextAny(ref reader, true, Token.TokenType.CloseBracket, 
                Token.TokenType.ConcatenatorAnd, Token.TokenType.ConcatenatorOr);
            return ctuple;
        }

        class InnerClusterTuple : SyntaxTuple
        {
            public ExpressionTuple Expression { get; set; }

            public ConcatenatorTuple Concatenator { get; set; }
        }

        private static InnerClusterTuple MakeInnerCluster(ref TokenReader reader)
        {
            System.Diagnostics.Debug.WriteLine("MakeInnerCluster");
            // Follow -> )
            if (TryLookAhead(ref reader, Token.TokenType.CloseBracket, false))
                return new InnerClusterTuple();
            else
            {
                var rct =  new InnerClusterTuple()
                {
                    Expression = MakeExpression(ref reader),
                    Concatenator = MakeConcatenator(ref reader)
                };
                AssertNextAny(ref reader, false, Token.TokenType.CloseBracket);
                return rct;
            }
        }

        class ConcatenatorTuple : SyntaxTuple
        {
            public ExpressionTuple Expression { get; set; }
            public bool IsConcatenateAnd { get; set; }
            public ConcatenatorTuple Concatenator { get; set; }
        }

        private static ConcatenatorTuple MakeConcatenator(ref TokenReader reader)
        {
            System.Diagnostics.Debug.WriteLine("MakeConcatenator");
            ConcatenatorTuple ret = new ConcatenatorTuple();
            if (TryLookAhead(ref reader, Token.TokenType.ConcatenatorAnd))
            {
                ret.IsConcatenateAnd = true;
                ret.Expression = MakeExpression(ref reader);
                ret.Concatenator = MakeConcatenator(ref reader);
            }
            else if (TryLookAhead(ref reader, Token.TokenType.ConcatenatorOr))
            {
                ret.IsConcatenateAnd = false;
                ret.Expression = MakeExpression(ref reader);
                ret.Concatenator = MakeConcatenator(ref reader);
            }
            else
            {
                ret = new ConcatenatorTuple();
            }
            AssertNextAny(ref reader, false, Token.TokenType.CloseBracket);
            return ret;
        }

        class ExpressionTuple : SyntaxTuple
        {
            public ClusterTuple Cluster { get; set; }
            public FilterTuple Filter { get; set; }
        }

        private static ExpressionTuple MakeExpression(ref TokenReader reader)
        {
            System.Diagnostics.Debug.WriteLine("MakeExpression");
            ExpressionTuple ret = new ExpressionTuple();
            if (TryLookAhead(ref reader, Token.TokenType.Exclamation, false) ||
                TryLookAhead(ref reader, Token.TokenType.OpenBracket, false))
            {
                // 新クラスタの開始
                ret.Cluster = MakeCluster(ref reader);
            }
            else
            {
                // フィルタの開始
                ret.Filter = MakeFilter(ref reader);
            }
            AssertNextAny(ref reader, true, Token.TokenType.CloseBracket,
                Token.TokenType.ConcatenatorAnd, Token.TokenType.ConcatenatorOr);
            return ret;
        }

        class FilterTuple : SyntaxTuple
        {
            public Token Name { get; set; }

            public FilterAttrTuple FilterAttr { get; set; }
        }

        private static FilterTuple MakeFilter(ref TokenReader reader)
        {
            System.Diagnostics.Debug.WriteLine("MakeFilter");
            var ret = new FilterTuple()
            {
                Name = AssertNext(ref reader, Token.TokenType.Literal),
                FilterAttr = MakeFilterAttr(ref reader)
            };
            AssertNextAny(ref reader, false, Token.TokenType.CloseBracket,
                Token.TokenType.ConcatenatorAnd, Token.TokenType.ConcatenatorOr);
            return ret;
        }

        class FilterAttrTuple : SyntaxTuple
        {
            public bool IsNegate { get; set; }
            public ArgDescriptTuple ArgDescript { get; set; }
        }

        private static FilterAttrTuple MakeFilterAttr(ref TokenReader reader)
        {
            System.Diagnostics.Debug.WriteLine("MakeFilterAttr");
            var ret = new FilterAttrTuple();
            if(TryLookAhead(ref reader, Token.TokenType.Exclamation))
            {
                // Negate
                ret.IsNegate = true;
            }
            // 引数を読む
            ret.ArgDescript = MakeArgDescript(ref reader);
            // 終わり
            AssertNextAny(ref reader, false,  Token.TokenType.CloseBracket,
                Token.TokenType.ConcatenatorAnd, Token.TokenType.ConcatenatorOr);
            return ret;
        }

        class ArgDescriptTuple : SyntaxTuple
        {
            public ArgsTuple Args { get; set; }
        }

        private static ArgDescriptTuple MakeArgDescript(ref TokenReader reader)
        {
            System.Diagnostics.Debug.WriteLine("MakeArgDescript");
            ArgDescriptTuple ret = new ArgDescriptTuple();
            if (TryLookAhead(ref reader, Token.TokenType.Collon))
            {
                // NOT EPSILON
                ret.Args = MakeArgs(ref reader);
            }
            AssertNextAny(ref reader, false,  Token.TokenType.CloseBracket,
                Token.TokenType.ConcatenatorAnd, Token.TokenType.ConcatenatorOr);
            return ret;
        }

        class ArgsTuple : SyntaxTuple
        {
            public ArgBodyTuple ArgBody { get; set; }
            public ArgConcatrTuple ArgConcatr { get; set; }
        }

        private static ArgsTuple MakeArgs(ref TokenReader reader)
        {
            System.Diagnostics.Debug.WriteLine("MakeArgs");
            var ret = new ArgsTuple()
            {
                ArgBody = MakeArgBody(ref reader),
                ArgConcatr = MakeArgConcatr(ref reader)
            };
            AssertNextAny(ref reader, false, Token.TokenType.CloseBracket,
                Token.TokenType.ConcatenatorAnd, Token.TokenType.ConcatenatorOr);
            return ret;
        }

        class ArgConcatrTuple : SyntaxTuple
        {
            public ArgBodyTuple ArgBody { get; set; }
            public ArgConcatrTuple ArgConcatr{get;set;}
        }

        private static ArgConcatrTuple MakeArgConcatr(ref TokenReader reader)
        {
            System.Diagnostics.Debug.WriteLine("MakeArgConcatr");
            ArgConcatrTuple ret = new ArgConcatrTuple();
            if (TryLookAhead(ref reader, Token.TokenType.Comma))
            {
                ret.ArgBody = MakeArgBody(ref reader);
                ret.ArgConcatr = MakeArgConcatr(ref reader);
            }
            AssertNextAny(ref reader, false, Token.TokenType.CloseBracket,
                Token.TokenType.ConcatenatorAnd, Token.TokenType.ConcatenatorOr);
            return ret;
        }

        class ArgBodyTuple : SyntaxTuple
        {
            public Token Arg { get; set; }
        }

        private static ArgBodyTuple MakeArgBody(ref TokenReader reader)
        {
            System.Diagnostics.Debug.WriteLine("MakeArgBody");
            var token = reader.Get();
            if (token.Type != Token.TokenType.String && token.Type != Token.TokenType.Literal)
            {
                reader.PushError("不明な文字です: " + reader.LookAhead() + " (インデックス:" + reader.LookAhead().DebugIndex + ")" +
                "フィルタ引数は、単純な文字列かダブルクオートで括られた文字列のみが指定できます。");
            }
            var ret = new ArgBodyTuple() { Arg = token };
            AssertNextAny(ref reader, false, Token.TokenType.Comma, Token.TokenType.CloseBracket,
                Token.TokenType.ConcatenatorAnd, Token.TokenType.ConcatenatorOr);
            return ret;
        }

        #endregion

        #region Generate filter

        /// <summary>
        /// 組からフィルタを生成します。
        /// </summary>
        /// <param name="tuple">構文解析済みの組</param>
        /// <returns>フィルタ</returns>
        private static FilterCluster GenerateFilter(SyntaxTuple tuple)
        {
            var root = tuple as RootTuple;
            if (root == null)
            {
                throw new InvalidOperationException("内部エラー: ルートタプル種別が一致しません。(渡された型: " + tuple.GetType().ToString() + " / " + tuple.ToString() + ")");
            }
            return AnalysisCluster(root.Cluster);
        }

        private static FilterCluster AnalysisCluster(ClusterTuple cTuple)
        {
            if (cTuple.InnerCluster != null)
            {
                var ic = AnalysisInnerCluster(cTuple.InnerCluster);
                ic.Negate = cTuple.Negate;
                return ic;
            }
            else
            {
                return new FilterCluster() { Negate = cTuple.Negate };
            }
        }

        private static FilterCluster AnalysisInnerCluster(InnerClusterTuple icTuple)
        {
            return AnalysisConcatenator(
                AnalysisExpression(icTuple.Expression),
                icTuple.Concatenator);
        }

        private static FilterCluster AnalysisConcatenator(IFilter leftExpr, ConcatenatorTuple coTuple)
        {
            if (coTuple.Expression == null)
            {
                return new FilterCluster() { Filters = new[] { leftExpr } };
            }
            else
            {
                return new FilterCluster()
                {
                    ConcatenateAnd = coTuple.IsConcatenateAnd,
                    Filters = new[]{
                        leftExpr,
                        AnalysisConcatenator(AnalysisExpression(coTuple.Expression), coTuple.Concatenator)}
                };
            }
        }

        private static IFilter AnalysisExpression(ExpressionTuple exprTuple)
        {
            if (exprTuple.Cluster != null)
            {
                // Cluster Tuple
                if (exprTuple.Filter != null)
                    throw new InvalidOperationException("[内部エラー]AVAILABLE EXPRESSION.CLUSTER AND EXPRESSION.FILTER.");
                return AnalysisCluster(exprTuple.Cluster);
            }
            else
            {
                // Filter Tuple
                if (exprTuple.Filter == null)
                    throw new InvalidOperationException("[内部エラー]UNAVAILABLE EXPRESSION.CLUSTER NOR EXPRESSION.FILTER.");
                return AnalysisFilterAndAttr(exprTuple.Filter);
            }
        }

        private static FilterBase AnalysisFilterAndAttr(FilterTuple filterTuple)
        {
            var negate = filterTuple.FilterAttr.IsNegate;
            var argument = AnalysisArgDescript(filterTuple.FilterAttr.ArgDescript).ToArray();
            if(filterTuple.Name.Type != Token.TokenType.Literal)
                throw new InvalidOperationException("[内部エラー]MISMATCHED TYPE: FILTER TYPE REQUIRED Literal, BUT " + filterTuple.Name.Type.ToString() + ".");
            var filters = Core.FilterRegistrant.GetFilter(filterTuple.Name.Value);
            if(filters == null || filters.Count() == 0)
                throw new InvalidOperationException("ID \"" + filterTuple.Name.Value + "\" のフィルタは見つかりませんでした。");
            foreach(var f in filters)
            {
                var wellformed = (from c in f.GetConstructors()
                                  let cp = c.GetParameters()
                                  where cp.Count() == argument.Count()
                                  select cp)
                                  .Any(pia => pia.Zip(argument, (pi, o) => new { pi, o })
                                     .All(a => a.pi.ParameterType.Equals(a.o.GetType())));
                if (wellformed)
                {
                    var instance = CreateFilterInstance(f, argument);
                    instance.Negate = negate;
                    return instance;
                }
            }
            // オーバーロードが何も一致しない
            throw new ArgumentException("ID \"" + filterTuple.Name.Value + "\" のフィルタは存在しますが、引数が一致しません。(引数:" + String.Join(", ", argument.Select(a => a.ToString())) + ")");
        }

        private static IEnumerable<object> AnalysisArgDescript(ArgDescriptTuple adTuple)
        {
            return GetCTSTypedArguments(adTuple.Args);
        }

        private static IEnumerable<object> GetCTSTypedArguments(ArgsTuple argTuple)
        {
            foreach (var tok in GetArguments(argTuple))
            {
                yield return GetCTSTypedValue(tok);
            }
        }

        private static IEnumerable<Token> GetArguments(ArgsTuple argTuple)
        {
            if (argTuple == null || argTuple.ArgBody == null) yield break;
            yield return argTuple.ArgBody.Arg;
            var conc = argTuple.ArgConcatr;
            while (conc.ArgBody != null)
            {
                yield return conc.ArgBody.Arg;
                conc = conc.ArgConcatr;
            }
        }

        private static FilterBase CreateFilterInstance(Type filterType, object[] parameter)
        {
            FilterBase instance = null;
            try
            {
                instance = (FilterBase)Activator.CreateInstance(filterType, parameter);
            }
            catch (ArgumentException e)
            {
                throw new InvalidOperationException("内部エラー: フィルタの作成時にエラーが発生しました(" + filterType.ToString() + "):" + e.Message, e);
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("内部エラー: フィルタインスタンスを作成できません(" + filterType.ToString() + "):" + e.Message, e);
            }
            return instance;
        }

        #region Reflection

        private static object GetCTSTypedValue(Token tok)
        {
            switch (tok.Type)
            {
                case Token.TokenType.String:
                    // string
                    return tok.Value;
                case Token.TokenType.Literal:
                    // string or integer or boolean
                    bool bv;
                    if (bool.TryParse(tok.Value, out bv))
                    {
                        // boolean
                        return bv;
                    }
                    long lv;
                    if (long.TryParse(tok.Value, out lv))
                    {
                        // long
                        return lv;
                    }
                    LongRange lrv;
                    if (LongRange.TryParse(tok.Value, out lrv))
                    {
                        // range
                        return lrv;
                    }
                    // unknown
                    break;
            }
            throw new ArgumentException("不明な型の引数です:" + tok.ToString());
        }

        #endregion

        #endregion

        #region Common

        /// <summary>
        /// 先頭から終了のダブルクオートに出会うまでのテキストを取得します。<para />
        /// エスケープシーケンスを考慮します。
        /// </summary>
        /// <param name="query">クエリ文字列</param>
        /// <returns>文字列部分</returns>
        /// <param name="nextIndex">文字列の開始ダブルクオートのインデックスを渡してください。解析終了後は、文字列終了のダブルクオートを示します</param>
        /// <exception cref="System.ArgumentException">文字列の解析に失敗</exception>
        private static string GetInQuoteString(string query, ref int cursor)
        {
            int begin = cursor++;
            while (cursor < query.Length)
            {
                if (query[cursor] == '\\')
                {
                    // 次のダブルクオートかバックスラッシュを読み飛ばす
                    if (cursor + 1 == query.Length)
                    {
                        throw new ArgumentException("バックスラッシュでクエリが終了しています。");
                    }
                    else if (query[cursor + 1] == '"' || query[cursor + 1] == '\\')
                    {
                        cursor++;
                    }
                }
                else if (query[cursor] == '"')
                {
                    // ここで文字列おしまい
                    return query.Substring(begin + 1, cursor - begin - 1).UnescapeFromQuery();
                }
                cursor++;
            }
            throw new ArgumentException("文字列が終了していません: " + query.Substring(begin));
        }

        #endregion

        #endregion

        #region Optimization

        /// <summary>
        /// フィルタの最適化
        /// </summary>
        public static FilterCluster Optimize(FilterCluster cluster)
        {
            // ルートフィルタクラスタの最適化
            var items = cluster.Filters.SelectMany(f => OptimizeCo(f, cluster));
            if (items.Count() == 1 && items.First() is FilterCluster)
            {
                var rc = items.First() as FilterCluster;
                if (cluster.Negate)
                    rc.Negate = !rc.Negate;
                return rc.Contraction();
            }
            else
            {
                cluster.Filters = items.ToArray();
                return cluster.Contraction();
            }
        }

        private static IEnumerable<IFilter> OptimizeCo(IFilter filter, FilterCluster parent)
        {
            if (filter is FilterCluster)
                return OptimizeCoCluster((FilterCluster)filter, parent);
            else
                return new[] { filter };
        }

        private static IEnumerable<IFilter> OptimizeCoCluster(FilterCluster cluster, FilterCluster parent)
        {
            if (parent == null)
                throw new ArgumentNullException("parent");
            var items = (cluster.Filters ?? new IFilter[0]).SelectMany(f => OptimizeCo(f, cluster)).ToArray();
            if (items.Length == 0)
            {
                // 要素無しのフィルタ
                return new[]{new FilterCluster(){
                    ConcatenateAnd = false, // 常に AND 結合
                    Negate = cluster.ConcatenateAnd != cluster.Negate}
                };
                // AND  => U false  false
                // NAND => Φ false  true
                // OR   => Φ true   false
                // NOR  => U true   true
                // ANDかNANDに集約, つまり Negate を U なら false, Φ なら trueにセットする
            }
            else if (items.Length == 1)
            {
                // 要素が1つしかない場合、このクラスタをスルーする
                // このクラスタがNegateであれば、直下アイテムのNegate値を変更する
                if (cluster.Negate)
                    items[0].Negate = !items[0].Negate;
                // 所属を変更する
                return OptimizeCo(items[0], parent);
            }
            else if (cluster.Negate == false && cluster.ConcatenateAnd == parent.ConcatenateAnd)
            {
                // このクラスタがNegateでなく、直上のクラスタと同じ結合である場合
                // 直上のクラスタに合成する
                return items.SelectMany(f => OptimizeCo(f, parent));
            }
            else
            {
                // クラスタのアイテムを更新
                cluster.Filters = items;
                return new[] { cluster.Contraction() };
            }
        }

        /// <summary>
        /// フィルタを縮約化します。
        /// </summary>
        private static FilterCluster Contraction(this FilterCluster cluster)
        {
            // クラスタ内の全空フィルタクラスタを取得する
            var emptyClusters = cluster.Filters.OfType<FilterCluster>().Where(f => f.Filters.Count() == 0).ToArray();

            // [全ての空フィルタクラスタはORかNORである]
            if (emptyClusters.FirstOrDefault(f => f.ConcatenateAnd) != null)
                throw new ArgumentException("All empty filters must be OR or NOR.");

            // フィルタと非空フィルタクラスタを待避
            var filters = cluster.Filters.Except(emptyClusters).ToArray();

            // 1    : U [全ツイートの抽出: !() [NOR(Φ)]]
            // 0    : F [一つだけ含まれるフィルタ]
            // -1   : Φ [抽出されるツイート無し: () [OR(Φ)]
            int resultValue = 0;
            if (cluster.ConcatenateAnd)
            {
                // AND 結合モード

                // OR 空フィルタが含まれていたら resultvalue = -1 (Φ)
                if (emptyClusters.FirstOrDefault(f => !f.Negate) != null)
                    resultValue = -1;
                // そうでない場合は、フィルタが存在すればresultValue = 0, でなければ 1
                else if (filters.Length > 0)
                    resultValue = 0;
                else
                    resultValue = 1;
            }
            else
            {
                // OR 結合モード

                // NOR 空フィルタが含まれていたら resultvalue = 1 (U)
                if (emptyClusters.FirstOrDefault(f => f.Negate) != null)
                    resultValue = 1;
                // そうでない場合は、フィルタが存在すればresultValue = 0, でなければ -1
                else if (filters.Length > 0)
                    resultValue = 0;
                else
                    resultValue = -1;
            }

            if (resultValue == 1) // U
                return new FilterCluster()
                {
                    ConcatenateAnd = true,
                    Negate = cluster.Negate
                };
            else if (resultValue == 0) // F
                return new FilterCluster()
                {
                    ConcatenateAnd = cluster.ConcatenateAnd,
                    Filters = filters,
                    Negate = cluster.Negate
                };
            else if (resultValue == -1) // Φ
                return new FilterCluster()
                {
                    ConcatenateAnd = false,
                    Negate = !cluster.Negate
                };
            else
                throw new InvalidOperationException("resultValue is invalid:" + resultValue.ToString());
        }

        #endregion
    }
}
