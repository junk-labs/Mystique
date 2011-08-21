using Dulcet.Twitter.Credential;
using Dulcet.Util;

namespace Dulcet.Twitter.Rest
{
    public static partial class Api
    {
        /// <summary>
        /// Check twitter is available
        /// </summary>
        /// <param name="provider">credential provider</param>
        /// <returns>If twitter is available, returns true</returns>
        public static bool Test(this CredentialProvider provider)
        {
            var doc = provider.RequestAPIv1("help/test.json", CredentialProvider.RequestMethod.GET, null);
            return doc.Root.Value == "ok";
        }


        /// <summary>
        /// Update rate-limit status explicitly<para />
        /// Sometimes this api returns wrong value, so you may not rely on this api.<para />
        /// Results overwrite provider's value.
        /// </summary>
        /// <param name="provider">credential provider</param>
        public static bool RateLimitStatus(this CredentialProvider provider)
        {
            var doc = provider.RequestAPIv1("account/rate_limit_status.json", CredentialProvider.RequestMethod.GET, null);
            if (doc == null) return false;
            var rate = doc.Element("hash");
            if (rate == null) return false;
            provider.UpdateRateLimit(
                (int)rate.Element("remaining-hits").ParseLong(),
                (int)rate.Element("hourly-limit").ParseLong(),
                rate.Element("reset-time-in-seconds").ParseUnixTime());
            return true;
        }
    }
}
