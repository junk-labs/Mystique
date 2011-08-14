using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Dulcet.Util;

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
            var preview = region.Attribute("Preview").ParseBool(false);
            var handleInText = region.Attribute("InTextBox").ParseBool(false);
            return new AssignItem(kstr, action, preview, handleInText);
        }
    }
}
