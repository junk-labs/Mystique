using System;

namespace Acuerdo.Plugin
{
    [Serializable]
    public class KrilePluginException : Exception
    {
        public IPlugin Plugin { get; private set; }

        public KrilePluginException(IPlugin plugin)
        {
            this.Plugin = plugin;
        }
        public KrilePluginException() { }
        public KrilePluginException(string message) : base(message) { }
        public KrilePluginException(IPlugin plugin, string message)
            : base(message)
        {
            this.Plugin = plugin;
        }
        public KrilePluginException(string message, Exception inner) : base(message, inner) { }
        public KrilePluginException(IPlugin plugin, string message, Exception inner)
            : base(message, inner)
        {
            this.Plugin = plugin;
        }
        protected KrilePluginException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }
}
