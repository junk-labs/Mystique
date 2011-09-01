using System;

namespace Inscribe.Configuration.Settings
{
    public class ExternalServiceProperty
    {
        public ExternalServiceProperty()
        {
            ShortenerService = String.Empty;
            UploaderService = String.Empty;
        }

        public string ShortenerService { get; set; }

        public string UploaderService { get; set; }
    }
}
