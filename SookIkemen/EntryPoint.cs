using System.ComponentModel.Composition;
using System.Linq;
using Acuerdo.Plugin;
using Inscribe.Communication.Posting;

namespace SookIkemen
{
    [Export(typeof(IPlugin))]
    public class EntryPoint : IPlugin
    {
        public string Name
        {
            get { return "スークイケメンﾅｰｰｰｰｰｰｰｰｰｰｰｰｰｰｰｰｰｰｰｰｰｰｰｰｰ"; }
        }

        public double Version
        {
            get { return 0; }
        }

        public void Loaded()
        {
            Inscribe.Core.Initializer.OnStandbyApp += () => Inscribe.Storage.AccountStorage.Accounts.ForEach(a => PostOffice.UpdateTweet(a, "スークイケメンﾅｰｰｰｰｰｰｰｰｰｰｰｰｰｰｰｰｰｰｰｰｰｰｰｰｰｰｰｰｰｰｰｰ #sookikemen"));
        }

        public IConfigurator ConfigurationInterface
        {
            get { return null; }
        }
    }
}
