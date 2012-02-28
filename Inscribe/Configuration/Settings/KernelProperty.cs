
namespace Inscribe.Configuration.Settings
{
    public class KernelProperty
    {
        public int ImageCacheGCInitialDelay = 1000 * 60 * 10;

        public int ImageCacheGCInterval = 1000 * 60 * 5;

        public int ImageLifetime = 1000 * 60 * 10;

        public double LastWriteVersion = 0;

        public string TwitterApiEndpoint = null;
    }
}
