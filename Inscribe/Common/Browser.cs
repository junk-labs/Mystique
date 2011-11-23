using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Inscribe.Configuration;

namespace Inscribe.Common
{
    public static class Browser
    {
        public static void Start(string navigate)
        {
            try
            {
                string path = Setting.Instance.ExperienceProperty.WebBrowserPath;
                if (string.IsNullOrEmpty(path))
                {
                    System.Diagnostics.Process.Start(navigate);
                }
                else
                {
                    string param = Setting.Instance.ExperienceProperty.WebBrowserParam;
                    if (string.IsNullOrEmpty(param))
                    {
                        param = navigate;
                    }
                    else
                    {
                        param = param.Replace("{URL}", navigate);
                    }
                    System.Diagnostics.Process.Start(path, param);
                }
            }
            catch { } // 握りつぶす
        }
    }
}
