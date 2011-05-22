using System;
using System.Diagnostics;
using System.Reflection;

namespace Inscribe
{
    public static class Define
    {
        public static string GetExecutingPath()
        {
            return Assembly.GetExecutingAssembly().Location;
        }

        public static FileVersionInfo GetVersion()
        {
            return FileVersionInfo.GetVersionInfo(GetExecutingPath());
        }

        public static string GetFormattedVersion()
        {
            return GetVersion().FileVersion;
        }

        public static double GetNumericVersion()
        {
            var lvobj = GetVersion();
            var lvstr = (lvobj.FileMajorPart * 1000 + lvobj.FileMinorPart).ToString() + "." + lvobj.FileBuildPart.ToString();
            return Double.Parse(lvstr);
        }

        public static readonly string SettingFileName = "krile.xml";

        public static readonly string RemoteVersionUrl = "http://update.starwing.net/krile2/update";

        public static readonly string RemoteUpdaterUrl = "http://update.starwing.net/krile2/kup.exe";

        public static readonly string FeedbackAppName = "reporter.exe";

        public static readonly string DefaultStatusMessage = "完了";
    }
}
