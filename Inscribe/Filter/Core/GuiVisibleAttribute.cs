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
        /// <summary>
        /// このプロパティがGUIを通して編集可能であることを示します。
        /// </summary>
        /// <param name="desc">このプロパティの説明</param>
        public GuiVisibleAttribute(string desc)
        {
            this.description = desc;
        }

        /// <summary>
        /// このプロパティの説明
        /// </summary>
        public string Description { get { return description; } }
    }
}
