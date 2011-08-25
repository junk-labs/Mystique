using System;
using System.Linq;
using System.Windows.Input;

namespace Inscribe.Subsystems.KeyAssign
{
    public class AssignItem
    {
        public AssignItem(Key key, ModifierKeys modifiers, string action, bool preview, bool handleInTextBox)
        {
            this.Key = key;
            this.Modifiers = modifiers;
            this.ActionId = action;
            this.LookInPreview = preview;
            this.HandleInTextBox = handleInTextBox;
        }

        public AssignItem(string key, string action, bool preview, bool handleInTextBox)
        {
            try
            {
                var kexs = key.Split(new[] { "+" }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToArray();
                this.Modifiers = ModifierKeys.None;
                if (kexs.Length > 1)
                {
                    // MODIFIER付き
                    var modifiers = kexs.Take(kexs.Length - 1).Select(s => ConvertToModifier(s)).Distinct();
                    foreach (var m in modifiers)
                        this.Modifiers |= m;
                }
                this.Key = (Key)(new KeyConverter().ConvertFromString(kexs.Last()));
                if (!KeyAssignCore.ExistsAction(action))
                    throw new Exception("アクション " + action + " は存在しません。");
                this.ActionId = action;
                this.LookInPreview = preview;
                this.HandleInTextBox = handleInTextBox;
            }
            catch (Exception e)
            {
                throw new Exception("キーアサイン情報を読み込めません。", e);
            }
        }

        private ModifierKeys ConvertToModifier(string modStr)
        {
            if (modStr.Equals("Ctrl", StringComparison.CurrentCultureIgnoreCase) ||
                modStr.Equals("C", StringComparison.CurrentCultureIgnoreCase) ||
                modStr.Equals("Control", StringComparison.CurrentCultureIgnoreCase))
            {
                return ModifierKeys.Control;
            }
            else if (modStr.Equals("Alt", StringComparison.CurrentCultureIgnoreCase) ||
                modStr.Equals("A", StringComparison.CurrentCultureIgnoreCase) ||
                modStr.Equals("Alternate", StringComparison.CurrentCultureIgnoreCase) ||
                modStr.Equals("Meta", StringComparison.CurrentCultureIgnoreCase))
            {
                return ModifierKeys.Alt;
            }
            else if (modStr.Equals("Shift", StringComparison.CurrentCultureIgnoreCase) ||
                modStr.Equals("S", StringComparison.CurrentCultureIgnoreCase))
            {
                return ModifierKeys.Shift;
            }
            else if (modStr.Equals("Win", StringComparison.CurrentCultureIgnoreCase) ||
                modStr.Equals("Windows", StringComparison.CurrentCultureIgnoreCase) ||
                modStr.Equals("W", StringComparison.CurrentCultureIgnoreCase))
            {
                return ModifierKeys.Windows;
            }
            else
            {
                return ModifierKeys.None;
            }
        }

        public Key Key { get; private set; }

        public ModifierKeys Modifiers { get; private set; }

        public string ActionId { get; private set; }

        public bool LookInPreview { get; private set; }

        public bool HandleInTextBox { get; private set; }
    }
}
