
namespace Inscribe.Configuration.Elements
{
    /// <summary>
    /// アカウント情報に付随する設定
    /// </summary>
    public class AccountProperty
    {
        public AccountProperty()
        {
            this.IsSelectedDefault = false;
            this.UserStreamsRepliesAll = false;
            this.UseUserStreams = true;
            this.AutoCruiseDefaultMu = 0.5;
            this.AutoCruiseApiConsumeRate = 0.8;
            this.FooterString = null;
            this.AccountDependentQuery = null;
        }

        public bool IsSelectedDefault { get; set; }

        public bool UserStreamsRepliesAll { get; set; }

        public bool UseUserStreams { get; set; }

        public double AutoCruiseDefaultMu { get; set; }

        public double AutoCruiseApiConsumeRate { get; set; }

        public string FooterString { get; set; }

        public string[] AccountDependentQuery { get; set; }

        public string FallbackAccount { get; set; }
    }
}
