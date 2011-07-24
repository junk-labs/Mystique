using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Inscribe.Configuration.Settings
{
    public class KeyAssignProperty
    {
        public static string DefaultAssignFileName = "default.xml";

        public KeyAssignProperty()
        {
            this.KeyAssignFile = DefaultAssignFileName;
        }

        public string KeyAssignFile { get; set; }
    }
}
