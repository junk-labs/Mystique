using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Inscribe.Filter.Core
{
    /// <summary>
    /// このプロパティがGUIを通して編集可能であることを示します。
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class GuiVisibleAttribute : Attribute
    {
        private string description;

        private string hint;

        /// <summary>
        /// このプロパティがGUIを通して編集可能であることを示します。
        /// </summary>
        /// <param name="desc">このプロパティの説明</param>
        public GuiVisibleAttribute(string desc) : this(desc, String.Empty) { }

        /// <summary>
        /// このプロパティがGUIを通して編集可能であることを示します。
        /// </summary>
        /// <param name="desc">このプロパティの説明</param>
        /// <param name="hint">このプロパティの追加説明</param>
        public GuiVisibleAttribute(string desc, string hint)
        {
            this.description = desc;
            this.hint = hint;
        }

        /// <summary>
        /// このプロパティの説明
        /// </summary>
        public string Description { get { return this.description; } }

        public string Hint { get { return this.hint; } }
    }
}
