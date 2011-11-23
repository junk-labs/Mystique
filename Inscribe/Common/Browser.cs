using Inscribe.Configuration;

namespace Inscribe.Common
{
    public static class Browser
    {
        public static void Start(string navigate)
        {
            try
            {
                string path = Setting.Instance.ExternalProperty.WebBrowserPath;
                if (string.IsNullOrEmpty(path))
                {
                    System.Diagnostics.Process.Start(navigate);
                }
                else
                {
                    string param = Setting.Instance.ExternalProperty.WebBrowserParam;
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
