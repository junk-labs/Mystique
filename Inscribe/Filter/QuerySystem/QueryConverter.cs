using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace Inscribe.Filter.QuerySystem
{
    public static class QueryConverter
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
            if (!(queryString.StartsWith("(") && queryString.EndsWith(")"))) // 最外殻を追加
                queryString = "(" + queryString + ")";
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
                /// エクスクラメーション マーク
                /// </summary>
                Exclamation,
                /// <summary>
                /// リテラル(べた書き文字列,数値など)
                /// </summary>
                Literal,
                /// <summary>
                /// &amp;&amp;記号
                /// </summary>
                ConcatenatorAnd,
                /// <summary>
                /// ||記号
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

            public Token(TokenType type, int debugIndex) : this()
            {
                this.Type = type;
                this.Value = null;
                this.DebugIndex = debugIndex;
            }

            public Token(TokenType type, string value, int debugIndex) : this()
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
                        return "&& [AND結合子]";
                    case TokenType.ConcatenatorOr:
                        return "|| [OR結合子]";
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
                        // CONCATENATE &&
                        if (CheckIsNotNext(query, strptr, '&'))
                            throw new ArgumentException("AND結合演算子は&&です。(" + strptr.ToString() + ")");
                        yield return new Token(Token.TokenType.ConcatenatorAnd, strptr);
                        strptr++;
                        break;
                    case '|':
                        // CONCATENATE ||
                        if (CheckIsNotNext(query, strptr, '|'))
                            throw new ArgumentException("OR結合演算子は||です。(" + strptr.ToString() + ")");
                        yield return new Token(Token.TokenType.ConcatenatorOr, strptr);
                        strptr++;
                        break;
                    case '.':
                        yield return new Token(Token.TokenType.Period, strptr);
                        break;
                    case ',':
                        yield return new Token(Token.TokenType.Comma, strptr);
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
                        var tokens = "&|.()\" \t\r\n";
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
            if(i + 1 >= q.Length ||q[i] != n)
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
                //System.Diagnostics.Debug.WriteLine("read:" + tokenQueueList[queueCursor]);
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
            KQRootTuple roots = null;
            try
            {
                roots = MakeKQRoot(ref reader);
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
                throw new ArgumentException("クエリ トークン種別 " + type.ToString() + " を読もうとしましたが、ぶった切れています。");
            var ntoken = reader.Get();
            if (ntoken.Type != type)
            {
                reader.PushError("クエリ トークン " + type.ToString() + " が必要です。または、トークンの種類が違います。(読み込まれたトークン: " + ntoken + ") (@" + ntoken.DebugIndex + ")");
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

        #endregion

        class KQRootTuple : SyntaxTuple
        {
            public ClusterTuple Cluster { get; set; }
        }

        private static KQRootTuple MakeKQRoot(ref TokenReader reader)
        {
            if (!reader.IsRemainToken)
                return new KQRootTuple() { Cluster = null };
            else
                return new KQRootTuple() { Cluster = MakeCluster(ref reader) };

        }

        class ClusterTuple : SyntaxTuple
        {
            public bool Negate { get; set; }

            public ExpressionTuple Expression { get; set; }
        }

        private static ClusterTuple MakeCluster(ref TokenReader reader)
        {
            var ctuple = new ClusterTuple();
            ctuple.Negate = TryLookAhead(ref reader, Token.TokenType.Exclamation);
            AssertNext(ref reader, Token.TokenType.OpenBracket);
            if (!TryLookAhead(ref reader, Token.TokenType.CloseBracket, true))
            {
                ctuple.Expression = MakeExpression(ref reader);
                AssertNext(ref reader, Token.TokenType.CloseBracket);
            }
            return ctuple;
        }

        class ExpressionTuple : SyntaxTuple
        {
            public ExpressionBodyTuple ExpressionBody { get; set; }

            public bool? ConcatOr { get; set; }

            public ExpressionTuple Expression { get; set; }
        }

        private static ExpressionTuple MakeExpression(ref TokenReader reader)
        {
            var extuple = new ExpressionTuple()
            {
                ExpressionBody = MakeExpressionBody(ref reader)
            };
            if (reader.IsRemainToken)
            {
                var ntoken = reader.LookAhead();
                switch (ntoken.Type)
                {
                    case Token.TokenType.ConcatenatorAnd:
                        reader.Get();
                        extuple.ConcatOr = false;
                        extuple.Expression = MakeExpression(ref reader);
                        break;
                    case Token.TokenType.ConcatenatorOr:
                        reader.Get();
                        extuple.ConcatOr = true;
                        extuple.Expression = MakeExpression(ref reader);
                        break;
                    case Token.TokenType.CloseBracket:
                        break;
                    default:
                        throw new ArgumentException("トークン " + ntoken.ToString() + " はここに置くことはできません。(@" + ntoken.DebugIndex + ")");
                }
            }
            return extuple;
        }

        class ExpressionBodyTuple : SyntaxTuple
        {
            public ClusterTuple Cluster { get; set; }

            public MethodDeclarationTuple MethodDeclaration { get; set; }
        }

        private static ExpressionBodyTuple MakeExpressionBody(ref TokenReader reader)
        {
            bool rewind = TryLookAhead(ref reader, Token.TokenType.Exclamation);
            if (TryLookAhead(ref reader, Token.TokenType.OpenBracket, false))
            {
                // 開き括弧/!マーク -> クラスタ
                if (rewind)
                    reader.RewindOne();
                return new ExpressionBodyTuple() { Cluster = MakeCluster(ref reader) };
            }
            else
            {
                if (rewind)
                    reader.RewindOne();
                return new ExpressionBodyTuple() { MethodDeclaration = MakeMethodDeclaration(ref reader) };
            }
        }

        class MethodDeclarationTuple : SyntaxTuple
        {
            public Token Namespace { get; set; }

            public Token Class { get; set; }

            public MethodAndArgsTuple MethodAndArgs { get; set; }

            public bool Negate { get; set; }
        }

        private static MethodDeclarationTuple MakeMethodDeclaration(ref TokenReader reader)
        {
            bool negate = TryLookAhead(ref reader, Token.TokenType.Exclamation);
            var ns = AssertNext(ref reader, Token.TokenType.Literal);
            AssertNext(ref reader, Token.TokenType.Period);
            var cls = AssertNext(ref reader, Token.TokenType.Literal);
            AssertNext(ref reader, Token.TokenType.Period);
            return new MethodDeclarationTuple()
            {
                Namespace = ns,
                Class = cls,
                MethodAndArgs = MakeMethodAndArgs(ref reader),
                Negate = negate
            };
        }

        class MethodAndArgsTuple : SyntaxTuple
        {
            public Token MethodName { get; set; }

            public ArgumentsTuple Arguments { get; set; }
        }

        private static MethodAndArgsTuple MakeMethodAndArgs(ref TokenReader reader)
        {
            var mname = AssertNext(ref reader, Token.TokenType.Literal);
            if (TryLookAhead(ref reader, Token.TokenType.OpenBracket))
            {
                // 引数付きメソッド呼び出し
                var ret = new MethodAndArgsTuple()
                {
                    MethodName = mname,
                    Arguments = MakeArguments(ref reader)
                };
                AssertNext(ref reader, Token.TokenType.CloseBracket);
                return ret;
            }
            else
            {
                // 引数を省略したメソッド呼び出し
                return new MethodAndArgsTuple() { MethodName = mname };
            }
        }

        class ArgumentsTuple : SyntaxTuple
        {
            public ArgBodyTuple ArgBody { get; set; }
        }

        private static ArgumentsTuple MakeArguments(ref TokenReader reader)
        {
            if (TryLookAhead(ref reader, Token.TokenType.CloseBracket, false)) // 引数なし
                return null;
            else
                return new ArgumentsTuple() { ArgBody = MakeArgBody(ref reader) };
        }

        class ArgBodyTuple : SyntaxTuple
        {
            public Token Argument { get; set; }
            public ArgBodyTuple ArgBody { get; set; }
        }

        private static ArgBodyTuple MakeArgBody(ref TokenReader reader)
        {
            var token = reader.Get();
            if (token.Type != Token.TokenType.String && token.Type != Token.TokenType.Literal)
            {
                throw new ArgumentException("引数が不正です。(@" + token.DebugIndex + ") " + token.ToString());
            }
            var abt = new ArgBodyTuple() { Argument = token };
            if (TryLookAhead(ref reader, Token.TokenType.Comma))
            {
                abt.ArgBody = MakeArgBody(ref reader);
            }
            return abt;
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
            var root = tuple as KQRootTuple;
            if (root == null)
            {
                throw new InvalidOperationException("内部エラー: ルートタプル種別が一致しません。(渡された型: " + tuple.GetType().ToString() + " / " + tuple.ToString() + ")");
            }
            return AnalysisCluster(root.Cluster);
        }

        private static FilterCluster AnalysisCluster(ClusterTuple tuple)
        {
            if (tuple == null)
            {
                return new FilterCluster();
            }
            else
            {
                if (tuple.Expression == null)
                {
                    return new FilterCluster() { Negate = tuple.Negate };
                }
                else
                {
                    var clstr = AnalysisExpression(tuple.Expression);
                    clstr.Negate = tuple.Negate;
                    return clstr;
                }
            }
        }

        private static FilterCluster AnalysisExpression(ExpressionTuple expressionTuple)
        {
            var retcluster = new FilterCluster()
            {
                ConcatenateOR = expressionTuple.ConcatOr.GetValueOrDefault(false)
            };
            if (expressionTuple.Expression == null)
            {
                retcluster.Filters = new[] { AnalysisExpressionBody(expressionTuple.ExpressionBody) };
            }
            else
            {
                if (!expressionTuple.ConcatOr.HasValue)
                    throw new InvalidOperationException("内部エラー: ExpressionTupleはチェーンを構成しますが、接続情報がありません。");
                retcluster.Filters = new[] { AnalysisExpressionBody(expressionTuple.ExpressionBody), AnalysisExpression(expressionTuple.Expression) };
            }
            return retcluster;
        }

        private static IFilter AnalysisExpressionBody(ExpressionBodyTuple expressionBodyTuple)
        {
            if (expressionBodyTuple.Cluster != null)
                return AnalysisCluster(expressionBodyTuple.Cluster);
            else
                return AnalysisMethodDeclaration(expressionBodyTuple.MethodDeclaration);
        }

        private static IFilter AnalysisMethodDeclaration(MethodDeclarationTuple methodDeclarationTuple)
        {
            if (methodDeclarationTuple.Namespace.Type != Token.TokenType.Literal)
                throw new InvalidOperationException("内部エラー: 型が一致しません(名前空間)");
            if (methodDeclarationTuple.Class.Type != Token.TokenType.Literal)
                throw new InvalidOperationException("内部エラー: 型が一致しません(クラス)");
            var ns = methodDeclarationTuple.Namespace.Value;
            var cls = methodDeclarationTuple.Class.Value;
            var fil = Manager.FilterRegistrant.GetFilterFromNsAndClass(ns, cls).ToArray();
            IFilter composed = null;
            if (fil.Length == 0)
            {
                throw new ArgumentException("フィルタが存在しません: " + ns + "." + cls);
            }
            else if (fil.Length == 1)
            {
                composed = AnalysisMethodAndArgs(methodDeclarationTuple.MethodAndArgs, fil[0], ns + "." + cls, true);
            }
            else
            {
                foreach (var f in fil)
                {
                    composed = AnalysisMethodAndArgs(methodDeclarationTuple.MethodAndArgs, f, ns + "." + cls, false);
                    if (composed != null) break;
                }
                if (composed == null)
                {
                    throw new ArgumentException("すべてのオーバーロードがマッチしませんでした: " + ns + "." + cls + "." + methodDeclarationTuple.MethodAndArgs.MethodName.Value);
                }
            }
            composed.Negate = methodDeclarationTuple.Negate;
            return composed;
        }

        private static IFilter AnalysisMethodAndArgs(MethodAndArgsTuple methodAndArgsTuple, Type filter, string filterInfo, bool terminal)
        {
            if (methodAndArgsTuple.MethodName.Type != Token.TokenType.Literal)
                throw new InvalidOperationException("内部エラー: 型が一致しません(メソッド");
            var methods = QueryCandidateMethods(filter);
            var arguments = GetCTSTypedArguments(methodAndArgsTuple.Arguments).ToArray();
            List<MethodInfo> foundMethods = new List<MethodInfo>();
            foreach (var m in methods)
            {
                if (m.Name == methodAndArgsTuple.MethodName.Value)
                {
                    var param = m.GetParameters();
                    if(param.Length == arguments.Length)
                    {
                        bool succ = true;
                        for(int i = 0; i < param.Length; i++)
                        {
                            if(!param[i].ParameterType.Equals(arguments[i].GetType()))
                            {
                                succ = false;
                                break;
                            }
                        }
                        if(succ)
                        {
                            // 見つかりました
                            return CreateFilterInstance(filter, m, arguments);
                        }
                    }
                    foundMethods.Add(m);
                }
            }
            if (terminal)
            {
                if (foundMethods.Count > 0)
                {
                    throw new ArgumentException(
                        "指定のオーバーロードを持つメソッド " + methodAndArgsTuple.MethodName.Value + " は、フィルタ " + filterInfo + " には見つかりませんでした。" + Environment.NewLine +
                        "フィルタ " + filterInfo + " が実装するメソッド:" + Environment.NewLine +
                        " - " + String.Join(Environment.NewLine + " - ", from m in methods
                                                                         select m.Name + "(" +
                                                                             String.Join(", ", (from p in m.GetParameters()
                                                                                               select p.ParameterType.ToString())) + ")"));
                }
                else
                {
                    throw new ArgumentException(
                        "メソッド \"" + methodAndArgsTuple.MethodName.Value + "\"は、フィルタ " + filterInfo + " には見つかりませんでした。" + Environment.NewLine +
                        "フィルタ " + filterInfo + " が実装するメソッド:" + Environment.NewLine +
                        " - " + String.Join(Environment.NewLine + " - ", methods.Select(m => m.Name).Distinct()));
                }
            }
            else
            {
                return null;
            }
        }

        private static IFilter CreateFilterInstance(Type filterType, MethodInfo method, object[] parameter)
        {
            IFilter instance = null;
            try
            {
                instance = (IFilter)Activator.CreateInstance(filterType);
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("内部エラー: フィルタのインスタンスを作成できません(" + filterType.ToString() + ")", e);
            }
            try
            {
                method.Invoke(instance, parameter);
            }
            catch (ArgumentException ae)
            {
                throw new ArgumentException("フィルタの生成時にエラーが発生しました。", ae);
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("内部エラー: フィルタの生成時にエラーが発生しました。", e);
            }
            return instance;
        }

        private static IEnumerable<object> GetCTSTypedArguments(ArgumentsTuple argTuple)
        {
            foreach (var tok in GetArguments(argTuple))
            {
                yield return GetCTSTypedValue(tok);
            }
        }

        private static IEnumerable<Token> GetArguments(ArgumentsTuple argTuple)
        {
            if (argTuple == null || argTuple.ArgBody == null) yield break;
            var body = argTuple.ArgBody;
            while (body != null)
            {
                yield return body.Argument;
                body = body.ArgBody;
            }
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
                    if(bool.TryParse(tok.Value, out bv))
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
                    // unknown
                    break;
            }
            throw new ArgumentException("不明な型の引数です:" + tok.ToString());
        }

        private static IEnumerable<MethodInfo> QueryCandidateMethods(Type type)
        {
            foreach (var meth in type.GetMethods())
            {
                if (meth.IsSpecialName || !meth.IsPublic || meth.IsAbstract) continue;
                foreach (var attr in Attribute.GetCustomAttributes(meth, typeof(MethodVisibleAttribute)))
                {
                    var openmeth = attr as MethodVisibleAttribute;
                    if (openmeth != null)
                    {
                        yield return meth;
                        break;
                    }
                }
            }
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
                    else if(query[cursor + 1] == '"' || query[cursor + 1] == '\\')
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

        /*

        #region Simplification

        /// <summary>
        /// フィルタの無駄な括弧を外し、簡単にします。
        /// </summary>
        /// <param name="cluster">簡単化するフィルタクラスタ</param>
        /// <returns>簡単化済みのフィルタクラスタ</returns>
        public static FilterCluster Simplify(FilterCluster cluster)
        {
            var nc = new FilterCluster()
            {
                ConcatenateOR = cluster.ConcatenateOR,
                Negate = false
            };
            nc.Filters = SimplifyCo(cluster, cluster.ConcatenateOR).ToArray();
            return nc;
        }

        /// <summary>
        /// フィルタの要素を処理します。
        /// </summary>
        /// <param name="filter">フィルタ要素</param>
        /// <param name="inConcatOr">この要素が含まれるフィルタがOR結合であるか</param>
        /// <returns>フィルタ要素か、クラスタ</returns>
        private static IEnumerable<IFilter> ExplodeItem(IFilter filter, bool inConcatOr)
        {
            var cluster = filter as FilterCluster;
            if (cluster == null)
            {
                // フィルタ
                return new[] { filter };
            }
            else
            {
                return SimplifyCo(cluster, inConcatOr);
            }
        }

        /// <summary>
        /// フィルタの簡単化を実行します
        /// </summary>
        /// <param name="cluster">処理対象クラスタ</param>
        /// <param name="inConcatOr">上層クラスタの合成手段</param>
        /// <returns></returns>
        private static IEnumerable<IFilter> SimplifyCo(FilterCluster cluster, bool inConcatOr)
        {
            if (cluster == null || cluster.Filters == null)
            {
                // ＼スカ／
                yield break;
            }
            else if (cluster.Filters.Count() == 1)
            {
                // 要素が1つなら要素だけ返してしまえホトトギス
                yield return cluster.Filters.ElementAt(0);
            }
            else
            {
                if (cluster.ConcatenateOR == inConcatOr && !cluster.Negate)
                {
                    // 直上のクラスタと同じ連結を用いている
                    // rankを落とす
                    foreach (var i in cluster.Filters)
                    {
                        foreach (var f in ExplodeItem(i, inConcatOr))
                        {
                            yield return f;
                        }
                    }
                }
                else
                {
                    // 同じ連結ではない

                    // 新しいフィルタクラスタを作る
                    var nc = new FilterCluster()
                    {
                        ConcatenateOR = cluster.ConcatenateOR,
                        Negate = cluster.Negate
                    };

                    // 新しいフィルタクラスタにセットするフィルタ
                    List<IFilter> nfs = new List<IFilter>();
                    foreach (var c in cluster.Filters)
                    {
                        foreach (var i in ExplodeItem(c, cluster.ConcatenateOR))
                        {
                            nfs.Add(i);
                        }
                    }
                    nc.Filters = nfs.ToArray();
                    yield return nc;
                }
            }
        }

        #endregion

        */

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
                    ConcatenateOR = false, // 常に AND 結合
                    Negate = cluster.ConcatenateOR != cluster.Negate}
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
            else if (cluster.Negate == false && cluster.ConcatenateOR == parent.ConcatenateOR)
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

            // [全ての空フィルタクラスタはANDかNANDである]
            if (emptyClusters.FirstOrDefault(f => f.ConcatenateOR) != null)
                throw new ArgumentException("All empty filters must be AND or NAND.");

            // フィルタと非空フィルタクラスタを待避
            var filters = cluster.Filters.Except(emptyClusters).ToArray();

            // 1    : U [全ツイートの抽出]
            // 0    : F [一つだけ含まれるフィルタ]
            // -1   : Φ [抽出されるツイート無し]
            int resultValue = 0;
            if (cluster.ConcatenateOR)
            {
                // OR 結合モード

                // AND 空フィルタが含まれていたら resultvalue = 1
                if (emptyClusters.FirstOrDefault(f => !f.Negate) != null)
                    resultValue = 1;
                // そうでない場合は、唯一含まれるフィルタが存在すればresultValue = 0, でなければ -1
                else if (filters.Length > 0)
                    resultValue = 0;
                else
                    resultValue = -1;
            }
            else
            {
                // AND 結合モード

                // NAND 空フィルタが含まれていたら resultvalue = -1
                if (emptyClusters.FirstOrDefault(f => f.Negate) != null)
                    resultValue = -1;
                // そうでない場合は、唯一含まれるフィルタが存在すればresultValue = 0, でなければ 1
                else if (filters.Length > 0)
                    resultValue = 0;
                else
                    resultValue = 1;
            }

            if (resultValue == 1) // U
                return new FilterCluster()
                {
                    ConcatenateOR = false,
                    Negate = cluster.Negate
                };
            else if (resultValue == 0) // F
                return new FilterCluster()
                {
                    ConcatenateOR = cluster.ConcatenateOR,
                    Filters = filters,
                    Negate = cluster.Negate
                };
            else if (resultValue == -1) // Φ
                return new FilterCluster()
                {
                    ConcatenateOR = false,
                    Negate = !cluster.Negate
                };
            else
                throw new InvalidOperationException("resultValue is invalid:" + resultValue.ToString());
        }

        #endregion
    }
}