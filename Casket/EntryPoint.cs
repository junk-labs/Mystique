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
            get { return "Image uploader supporting plugin"; }
        }

        public double Version
        {
            get { return 0.0; }
        }

        public IConfigurator ConfigurationInterface
        {
            get { return null; }
        }
    }
}
