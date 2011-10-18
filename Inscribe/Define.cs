using System;
using System.Diagnostics;

namespace Inscribe
{
    public static class Define
    {
        public static string ExeFilePath
        {
            get
            {
                return Process.GetCurrentProcess().MainModule.FileName;
            }
        }

        public static FileVersionInfo GetVersion()
        {
            return FileVersionInfo.GetVersionInfo(ExeFilePath);
        }

        public static string GetFormattedVersion()
        {
            var ver = GetVersion();
            return ver.FileMajorPart + "." + ver.FileMinorPart + "." + ver.FileBuildPart + FileKind(ver.FilePrivatePart);
        }

        private static string FileKind(int value)
        {
            switch (value)
            {
                case 0:
                    return String.Empty;
                case 1:
                    return " rc";
                case 2:
                    return " beta";
                case 3:
                    return " daybreak";
                case 4:
                    return " twilight";
                case 5:
                    return " midnight";
                default:
                    return " Special build " + value;
            }
        }

        public static double GetNumericVersion()
        {
            var lvobj = GetVersion();
            var lvstr = (lvobj.FileMajorPart * 1000 + lvobj.FileMinorPart).ToString() + "." + lvobj.FileBuildPart.ToString();
            return Double.Parse(lvstr);
        }

        public static readonly string ApplicationName = "Krile";

        public static readonly string SettingFileName = "krile.xml";

        public static readonly string KeyAssignDirectory = "assigns";

        public static readonly string MediaDirectory = "media";

        public static readonly string PluginDirectory = "plugins";

        public static readonly string FeedbackAppName = "reporter.exe";

        public static readonly string UpdateFileName = "kup.exe";

        public static readonly string DefaultStatusMessage = "完了";

        public static readonly string RemoteVersionXml = "http://krile.starwing.net/update/update.xml";

        public static readonly string PublicKeyFile = "kup.pub";

        public static readonly string MentionWavFile = "mention.wav";

        public static readonly string NewReceiveWavFile = "new.wav";

        public static readonly string DirectMessageWavFile = "directmessage.wav";

        public static readonly string EventWavFile = "event.wav";

        public static readonly string ReleaseNoteUrl = "http://krile.starwing.net/updates.html";

        public static readonly string FaqUrl = "http://krile.starwing.net/faq.html";

        public static readonly string FeedbackUrl = "https://github.com/karno/Mystique/issues";

        public static string KampaUrl = "http://krile.starwing.net/kampa.html";

        public static readonly int SafeModeThreshold = 3;

    }
}
