using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.CSharp;

namespace Acuerdo.Scripting
{
    // reference : http://labs.yaneu.com/20101017/

    /// <summary>
    /// C#によるスクリプトのコンパイルを実行し、アセンブリを取得します。
    /// </summary>
    public static class CompilationService
    {
        static Regex usingRegex = new Regex(@"[ \t]*(using [A-Za-z0-9\.]+)[ \t]+(in[ \t]*\""(.+)\"".*)?;");

        /// <summary>
        /// スクリプト ソースからアセンブリを生成します。<para />
        /// 生成した一時ファイルは実行環境がクリーンアップされると全て破棄されます。
        /// </summary>
        /// <param name="csSource">C# スクリプト ソース</param>
        /// <returns>アセンブリ</returns>
        public static Assembly Compile(this ExecutionEnvironment exenv, string csSource)
        {
            // todo: implement! ***************************
            throw new NotImplementedException();
            // ********************************************

            CompilerParameters cparam = new CompilerParameters();
            csSource = PreProcess(csSource, ref cparam);
            using (var compiler = new CSharpCodeProvider(
                new Dictionary<string, string>() { { "CompilerVersion", "v4.0" } }))
            {
                var result = compiler.CompileAssemblyFromSource(cparam, csSource);
                if (result.Errors.Count != 0)
                    throw new ArgumentException("コンパイル ソースが不正です。");
                return result.CompiledAssembly;
            }
        }

        private static string PreProcess(string source, ref CompilerParameters param)
        {
            var splited = source.Split(new[] { "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            // distinct用
            List<string> usings = new List<string>();
            // compile parameter用
            List<string> includes = new List<string>();
            List<string> references = new List<string>();
            // #pragma include "[cs source file]"
            // #pragma reference "[library path]"
            return source;
        }
    }
}
