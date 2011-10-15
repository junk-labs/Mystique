
namespace Inscribe.Configuration.Settings
{
    public class KernelProperty
    {
        public int ImageGCInitialDelay = 1000 * 60 * 3;

        public int ImageGCInterval = 1000 * 60;

        public int ImageLifetime = 1000 * 60 * 10;

        public int ImageCacheMaxCount = 200;

        public double ImageCacheSurviveDensity = 0.5;

        public double LastWriteVersion = 0;

        public string TwitterApiEndpoint = null;
    }
}
