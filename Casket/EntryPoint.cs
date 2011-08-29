using System.ComponentModel.Composition;
using System.Reflection;
using Acuerdo.Plugin;
using Inscribe.Plugin;

namespace Casket
{
    [Export(typeof(IPlugin))]
    public class EntryPoint : IPlugin
    {
        public void Loaded()
        {
            UploaderManager.RegisterAllUploadersInAsm(Assembly.GetExecutingAssembly());
            ShortenManager.RegisterAllShortenersInAsm(Assembly.GetExecutingAssembly());
        }

        public string Name
        {
            get { return "Krile basic plugin"; }
        }

        public double Version
        {
            get { return 1.0; }
        }

        public IConfigurator ConfigurationInterface
        {
            get { return null; }
        }
    }
}
