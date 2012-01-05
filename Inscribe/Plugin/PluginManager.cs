using System;
using System.Collections.Generic;
using Acuerdo.Plugin;

namespace Inscribe.Plugin
{
    public static class PluginManager
    {
        private static object _syncRoot = new object();

        private static List<IPlugin> _plugins = new List<IPlugin>();

        public static IEnumerable<IPlugin> Plugins
        {
            get
            {
                lock (_syncRoot)
                {
                    return _plugins.AsReadOnly();
                }
            }
        }

        public static void Register(IPlugin plugin)
        {
            lock (_syncRoot)
            {
                _plugins.Add(plugin);
            }
            try
            {
                plugin.Loaded();
            }
            catch (Exception e)
            {
                throw new KrilePluginException(plugin, "プラグインのロードに失敗しました。", e);
            }
        }
    }
}
