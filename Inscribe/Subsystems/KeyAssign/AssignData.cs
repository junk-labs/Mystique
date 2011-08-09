using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.IO;
using System.Windows.Input;

namespace Inscribe.Subsystems.KeyAssign
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

        private static AssignDescription LoadAssign(XElement root)
        {
            var name = root.Attribute("Name").Value;
            var assigns = root.Element("AssignGroups").Descendants("AssignGroup");
            return new AssignDescription()
            {
                Name = name,
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
            var action = region.Attribute("Action").Value;
            if (String.IsNullOrWhiteSpace(kstr) || String.IsNullOrWhiteSpace(action))
                throw new Exception("キーとアクションが無い要素があります。");
            var preview = region.Attribute("Preview");
            if (preview != null)
                return new AssignItem(kstr, action, bool.Parse(preview.Value));
            else
                return new AssignItem(kstr, action);
        }
    }

    public class AssignDescription
    {
        public string Name { get; set; }

        public IEnumerable<Tuple<AssignRegion, IEnumerable<AssignItem>>> AssignDatas { get; set; }
    }

    public class AssignItem
    {
        public AssignItem(Key key, ModifierKeys modifiers, string action, bool preview = false)
        {
            this.Key = key;
            this.Modifiers = modifiers;
            this.ActionId = action;
            this.LookInPreview = preview;
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
                this.ActionId = method;
                this.LookInPreview = allowInText;
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
    }
}
