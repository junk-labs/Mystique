using System;

namespace Inscribe.Configuration.Settings
{
    public class ExternalServiceProperty
    {
        public ExternalServiceProperty()
        {
            UploaderService = String.Empty;
        }

        public string UploaderService { get; set; }
    }
}
