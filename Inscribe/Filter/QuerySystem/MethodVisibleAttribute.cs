using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Inscribe.Filter.QuerySystem
{
    /// <summary>
    /// このメソッドがクエリシステムに対して公開されていることを示します。
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class MethodVisibleAttribute : Attribute { }
}
