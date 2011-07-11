using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Inscribe.Configuration;
using System.Windows;
using Inscribe.Threading;

namespace Mystique.Core
{
    internal static class Initializer
    {
        internal static void Init()
        {
            Dulcet.Network.Http.Expect100Continue = false;
            Dulcet.Network.Http.MaxConnectionLimit = Int32.MaxValue;
            Setting.Initialize();
            Application.Current.Exit += new ExitEventHandler(AppExit);
        }

        static void AppExit(object sender, ExitEventArgs e)
        {
            Setting.Instance.Save();
        }
    }
}
