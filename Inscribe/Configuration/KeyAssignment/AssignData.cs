using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.IO;
using System.Windows.Input;

namespace Inscribe.Configuration.KeyAssignment
{
    public static class AssignLoader
    {
        public static AssignDescription LoadAssign(string path)
        {
            try
            {
                if (!File.Exists(path))
                    throw new FileNotFoundException("ファイル " + path + " が見つかりません。");
                return LoadAssign(XElement.Load(path));
            }
            catch (Exception e)
            {
                throw new Exception("キーアサインの読み込みに失敗しました。", e);
            }
        }

        private static AssignDescription LoadAssign(XElement elem)
        {
            var assigns = elem.Element("KeyAssign").Descendants("Assigns");
            return new AssignDescription()
            {
                AssignDatas = assigns.Select(a => new Tuple<AssignRegion, IEnumerable<AssignItem>>(
                    ConvertToRegion(a.Attribute("Region").Value), a.Elements("Assign").Select(ae => ConvertToAssignItem(ae))))
            };
        }

        private static AssignRegion ConvertToRegion(string region)
        {
            return (AssignRegion)Enum.Parse(typeof(AssignRegion), region);
        }

        private static AssignItem ConvertToAssignItem(XElement region)
        {
            var kstr = region.Attribute("Key").Value;
            var meth = region.Attribute("Method").Value;
            var eit = region.Attribute("EnableInTextBox");
            if (eit != null)
                return new AssignItem(kstr, meth, bool.Parse(eit.Value));
            else
                return new AssignItem(kstr, meth);
        }
    }

    public class AssignDescription
    {
        public IEnumerable<Tuple<AssignRegion, IEnumerable<AssignItem>>> AssignDatas { get; set; }
    }

    public class AssignItem
    {
        public AssignItem(Key key, ModifierKeys modifiers, string method, bool allowInText = false)
        {
            this.Key = key;
            this.Modifiers = modifiers;
            this.Method = method;
            this.AllowedInTextBox = allowInText;
        }

        public AssignItem(string key, string method, bool allowInText = false)
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
                this.Method = method;
                this.AllowedInTextBox = allowInText;
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
                return ModifierKeys.Alt;
            }
            else
            {
                return ModifierKeys.None;
            }
        }

        public Key Key { get; private set; }

        public ModifierKeys Modifiers { get; private set; }

        public string Method { get; private set; }

        public bool AllowedInTextBox { get; private set; }
    }
}
