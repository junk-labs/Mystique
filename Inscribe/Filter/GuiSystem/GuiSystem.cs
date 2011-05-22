using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Inscribe.Filter.GuiSystem
{
    public static class GuiSystem
    {
        private static Dictionary<string, string> nsDescription = new Dictionary<string, string>();
        private static Dictionary<string, string> clsDescription = new Dictionary<string, string>();

        public static void SetNamespaceDescription(string ns, string desc)
        {
            if (nsDescription.ContainsKey(ns))
            {
                nsDescription[ns] = desc;
            }
            else
            {
                nsDescription.Add(ns, desc);
            }
        }

        public static void SetClassDescription(string cls, string desc)
        {
            if (clsDescription.ContainsKey(cls))
            {
                clsDescription[cls] = desc;
            }
            else
            {
                clsDescription.Add(cls, desc);
            }
        }

        public static string GetNamespaceDescription(string ns)
        {
            return nsDescription.ContainsKey(ns) ? nsDescription[ns] : String.Empty;
        }

        public static string GetClassDescription(string cls)
        {
            return clsDescription.ContainsKey(cls) ? clsDescription[cls] : String.Empty;
        }
    }
}
