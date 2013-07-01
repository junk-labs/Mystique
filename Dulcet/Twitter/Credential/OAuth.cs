using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Security.Cryptography;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using Dulcet.Network;
using Dulcet.Util;

namespace Dulcet.Twitter.Credential
{
    /// <summary>
    /// OAuth abstracted class (version 2)
    /// </summary>
    /// <remarks>
    /// You MUST get api token and secret for your application.
    /// </remarks>
    public abstract class OAuth : CredentialProvider
    {
        /// <summary>
        /// OAuth credential provider
        /// </summary>
        public OAuth() : this(null, null) { }

        /// <summary>
        /// OAuth credential provider
        /// </summary>
        /// <param name="token">user token</param>
        /// <param name="secret">user secret</param>
        public OAuth(string token, string secret)
        {
            this.Token = token;
            this.Secret = secret;
        }

        private enum DocumentTypes { Invalid, Xml, Json };

        /// <summary>
        /// RequestAPI implementation
        /// </summary>
        /// <param name="uri">full uri</param>
        /// <param name="method">using method for HTTP negotiation</param>
        /// <param name="param">additional parameters</param>
        /// <param name="request">used request</param>
        /// <returns>XML documents</returns>
        public sealed override XDocument RequestAPI(string uri, RequestMethod method, IEnumerable<KeyValuePair<string, string>> param, out HttpWebRequest request)
        {
            // validate arguments
            if (String.IsNullOrEmpty(uri))
                throw new ArgumentNullException(uri);
            else if (uri.Length < 5)
                throw new ArgumentException("uri is too short.");
            string target = uri;

            // detection return type of api
            var docType = DocumentTypes.Invalid;

            if (target.EndsWith("xml"))
                docType = DocumentTypes.Xml;
            else if (target.EndsWith("json"))
                docType = DocumentTypes.Json;
            else
                throw new ArgumentException("format can't identify. uriPartial is must ends with .xml or .json.");

            // pre-validation authentication
            if (String.IsNullOrEmpty(Token) || String.IsNullOrEmpty(Secret))
            {
                throw new WebException("OAuth Not Validated", WebExceptionStatus.ProtocolError);
            }
            var reg = GetHeader(target, method, param);
            try
            {
                var ps = JoinParamAsUrl(param);
                byte[] body = null;
                if (!String.IsNullOrWhiteSpace(ps))
                {
                    if (method == RequestMethod.GET)
                        target += "?" + ps;
                    if (method == RequestMethod.POST)
                        body = Encoding.ASCII.GetBytes(ps);
                }
                request = Http.CreateRequest(new Uri(target), method.ToString(), contentType: "application/x-www-form-urlencoded");
                request.Headers.Add("Authorization", "OAuth " + reg);
                var ret = Http.WebConnect<XDocument>(request,
                    responseconv: res =>
                    {
                        switch (docType)
                        {
                            case DocumentTypes.Xml:
                                return this.XDocumentGenerator(res);
                            case DocumentTypes.Json:
                                return this.XDocumentGenerator(res, (s) => JsonReaderWriterFactory.CreateJsonReader(s, XmlDictionaryReaderQuotas.Max));
                            default:
                                throw new NotSupportedException("Invalid format.");
                        }
                    }, senddata: body);
                if (ret.Succeeded && ret.Data != null)
                {
                    return ret.Data;
                }
                else
                {
                    if (ret.ThrownException != null)
                        throw ret.ThrownException;
                    else
                        throw new WebException();
                }
            }
            catch (WebException)
            {
                throw;
            }
            catch (XmlException)
            {
                throw;
            }
            catch (IOException)
            {
                throw;
            }
        }

        /// <summary>
        /// RequestStreamAPI implementation
        /// </summary>
        /// <param name="uri">full uri</param>
        /// <param name="method">using method for HTTP negotiation</param>
        /// <param name="param">parameters</param>
        /// <returns>Callback stream</returns>
        public sealed override Stream RequestStreamingAPI(string uri, RequestMethod method, IEnumerable<KeyValuePair<string, string>> param, out HttpWebRequest request)
        {
            if (String.IsNullOrEmpty(uri))
                throw new ArgumentNullException(uri);
            else if (uri.Length < 5)
                throw new ArgumentException("uri is too short.");
            string target = uri;

            if (String.IsNullOrEmpty(Token) || String.IsNullOrEmpty(Secret))
            {
                throw new WebException("OAuth Not Validated", WebExceptionStatus.ProtocolError);
            }

            var reg = GetHeader(target, method, param);
            try
            {
                var ps = JoinParamAsUrl(param);
                System.Diagnostics.Debug.WriteLine("streaming params:" + ps);
                byte[] body = null;
                if (!String.IsNullOrWhiteSpace(ps))
                {
                    if (method == RequestMethod.GET)
                        target += "?" + ps;
                    if (method == RequestMethod.POST)
                        body = Encoding.ASCII.GetBytes(ps);
                }
                request = Http.CreateRequest(new Uri(target), method.ToString(), contentType: "application/x-www-form-urlencoded");
                request.Headers.Add("Authorization", "OAuth " + reg);
                request.Timeout = 8000;
                request.AutomaticDecompression = DecompressionMethods.None; // due to delaying streaming receiving.

                var ret = Http.WebConnect<Stream>(req: request, responseconv: Http.ResponseConverters.GetStream, senddata: body);
                if (ret.Succeeded)
                {
                    return ret.Data;
                }
                else
                {
                    if (ret.Data != null)
                        ret.Data.Close();
                    if (ret.ThrownException != null)
                        throw ret.ThrownException;
                    else
                        throw new WebException();
                }
            }
            catch (WebException)
            {
                throw;
            }
            catch (IOException)
            {
                throw;
            }
        }

        /// <summary>
        /// Add header for OAuth-Echo credential
        /// </summary>
        public void MakeOAuthEchoRequest(ref HttpWebRequest request, IEnumerable<KeyValuePair<string, string>> param = null, string providerUri = ProviderEchoAuthorizeUrl)
        {
            // pre-validation authentication
            if (String.IsNullOrEmpty(Token) || String.IsNullOrEmpty(Secret))
            {
                throw new WebException("OAuth Not Validated", WebExceptionStatus.ProtocolError);
            }

            request.Headers["X-Auth-Service-Provider"] = providerUri;
            var header = GetHeader(providerUri, RequestMethod.GET, param);
            request.Headers["X-Verify-Credentials-Authorization"] = "OAuth realm=\"http://api.twitter.com/\"," + header;
        }

        #region User property

        /// <summary>
        /// OAuth token
        /// </summary>
        public string Token { get; set; }

        /// <summary>
        /// OAuth secret
        /// </summary>
        public string Secret { get; set; }

        #endregion

        /// <summary>
        /// Url encoding some string
        /// </summary>
        /// <param name="value">target</param>
        /// <param name="encoding">using encode</param>
        /// <param name="upper">helix cast to upper</param>
        /// <returns>encoded string</returns>
        public static string UrlEncode(string value, Encoding encoding, bool upper)
        {
            StringBuilder result = new StringBuilder();
            byte[] data = encoding.GetBytes(value);
            int len = data.Length;

            for (int i = 0; i < len; i++)
            {
                int c = data[i];
                if (c < 0x80 && AllowedChars.IndexOf((char)c) != -1)
                {
                    result.Append((char)c);
                }
                else
                {
                    if (upper)
                        result.Append('%' + String.Format("{0:X2}", (int)data[i]));
                    else
                        result.Append('%' + String.Format("{0:x2}", (int)data[i]));
                }
            }
            return result.ToString();
        }

        #region Getting access token

        private string GetRequestToken()
        {
            var reg = GetHeader(ProviderRequestTokenUrl, RequestMethod.GET, null, gettingRequestToken: true);
            try
            {
                var req = Http.CreateRequest(new Uri(ProviderRequestTokenUrl), "GET", contentType: "application/x-www-form-urlencoded");
                req.Headers.Add("Authorization", "OAuth " + reg);
                var ret = Http.WebConnectDownloadString(req);
                if (ret.ThrownException != null)
                    throw ret.ThrownException;
                if (!ret.Succeeded)
                    throw new Exception();
                var query = SplitParam(ret.Data);
                foreach (var q in query)
                {
                    if (q.Key == "oauth_token")
                    {
                        return q.Value;
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
            return null;
        }

        private string GetProviderAuthUrl(string token)
        {
            return ProviderAuthorizeUrl + "?oauth_token=" + token;
        }

        /// <summary>
        /// Get provider's authorization url
        /// </summary>
        /// <param name="reqToken">request token string</param>
        /// <returns>access uri</returns>
        public Uri GetProviderAuthUrl(out string reqToken)
        {
            reqToken = GetRequestToken();
            return new Uri(GetProviderAuthUrl(reqToken));
        }

        /// <summary>
        /// Get access token from request token
        /// </summary>
        /// <param name="token">request token</param>
        /// <param name="pin">personal identify code</param>
        /// <param name="userId">user id</param>
        /// <returns>succeed authorization</returns>
        public bool GetAccessToken(string token, string pin, out long userId, out string userScreenName)
        {

            //Generate param
            string paramName = TokenKey + "=";
            int idx = token.IndexOf(paramName);

            if (idx > 0)
                Token = token.Substring(idx + paramName.Length);
            else
                Token = token;

            var reg = GetHeader(ProviderAccessTokenUrl, RequestMethod.GET, null, pin);
            try
            {
                var req = Http.CreateRequest(new Uri(ProviderAccessTokenUrl), contentType: "application/x-www-form-urlencoded");
                req.Headers.Add("Authorization", "OAuth " + reg);
                var ret = Http.WebConnectDownloadString(req);
                if (ret.ThrownException != null)
                    throw ret.ThrownException;
                if (!ret.Succeeded)
                {
                    userId = 0;
                    userScreenName = null;
                    return false;
                }
                var rd = SplitParamDict(ret.Data);
                if (rd.ContainsKey("oauth_token") && rd.ContainsKey("oauth_token_secret") && Int64.TryParse(rd["user_id"], out userId))
                {
                    Token = rd["oauth_token"];
                    Secret = rd["oauth_token_secret"];
                    userScreenName = rd["screen_name"];
                    return true;
                }
                else
                {
                    userId = 0;
                    userScreenName = null;
                    return false;
                }
            }
            catch (WebException)
            {
                throw;
            }
        }

        #endregion

        #region Abstracted property

        /// <summary>
        /// Consumer key
        /// </summary>
        protected abstract string ConsumerKey { get; }

        /// <summary>
        /// Consumer secret
        /// </summary>
        protected abstract string ConsumerSecret { get; }

        #endregion

        #region OAuth system property and constant values

        const string Version = "1.0";
        const string ParamPrefix = "oauth_";

        #region Key values
        const string ConsumerKeyKey = "oauth_consumer_key";
        const string CallbackKey = "oauth_callback";
        const string VersionKey = "oauth_version";
        const string SignatureMethodKey = "oauth_signature_method";
        const string SignatureKey = "oauth_signature";
        const string TimestampKey = "oauth_timestamp";
        const string NonceKey = "oauth_nonce";
        const string TokenKey = "oauth_token";
        const string TokenSecretKey = "oauth_token_secret";
        const string VerifierKey = "oauth_verifier";
        #endregion

        const string ProviderRequestTokenUrl = "https://api.twitter.com/oauth/request_token";
        const string ProviderAccessTokenUrl = "https://api.twitter.com/oauth/access_token";
        const string ProviderAuthorizeUrl = "https://api.twitter.com/oauth/authorize";
        const string ProviderEchoAuthorizeUrl = "https://api.twitter.com/1.1/account/verify_credentials.json";

        /// <summary>
        /// Signature method for OAuth
        /// </summary>
        public enum OAuthSignatureMethod
        {
            /// <summary>
            /// HMAC SHA1
            /// </summary>
            Hmac_Sha1,
            /// <summary>
            /// Plain text
            /// </summary>
            PlainText,
            /// <summary>
            /// RSA SHA1
            /// </summary>
            Rsa_Sha1
        }

        private string GetOAuthSignatureMethodValue(OAuthSignatureMethod sigmethod)
        {
            switch (sigmethod)
            {
                case OAuthSignatureMethod.Hmac_Sha1:
                    return "HMAC-SHA1";
                case OAuthSignatureMethod.PlainText:
                    return "PLAINTEXT";
                case OAuthSignatureMethod.Rsa_Sha1:
                    return "RSA-SHA1";
                default:
                    return null;
            }
        }

        const OAuthSignatureMethod SignatureMethod = OAuthSignatureMethod.Hmac_Sha1;

        const string AllowedChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-_.~";

        #endregion

        #region Algorithms

        /// <summary>
        /// Get Authentication header text
        /// </summary>
        protected string GetHeader(string uri, RequestMethod method, IEnumerable<KeyValuePair<string, string>> param, string pin = null, bool gettingRequestToken = false)
        {
            var oap = GetOAuthParams(
                ConsumerKey, Token,
                GetTimestamp(), GetNonce(),
                SignatureMethod, pin, gettingRequestToken);
            var sig = GetSignature(
                new Uri(uri), ConsumerSecret, Secret,
                JoinParamAsUrl(param == null ? oap : oap.Concat(param)),
                SignatureMethod, method.ToString());
            return JoinParamAsHeader(
                oap.Concat(new[] { new KeyValuePair<string, string>(SignatureKey, sig) }).ToArray());
        }

        private string GetSignature(
            Uri uri, string consumerSecret, string tokenSecret,
            string joinedParam, OAuthSignatureMethod sigMethod,
            string requestMethod)
        {
            switch (sigMethod)
            {
                case OAuthSignatureMethod.PlainText:
                    return Util.HttpUtility.UrlEncode(consumerSecret + "&" + tokenSecret);
                case OAuthSignatureMethod.Hmac_Sha1:
                    if (String.IsNullOrEmpty(requestMethod))
                        throw new ArgumentNullException("requestMethod");

                    // formatting URI
                    var regularUrl = uri.Scheme + "://" + uri.Host;
                    if (!((uri.Scheme == "http" && uri.Port == 80) || (uri.Scheme == "https" && uri.Port == 443)))
                        regularUrl += ":" + uri.Port;
                    regularUrl += uri.AbsolutePath;

                    // Generate signature
                    StringBuilder SigSource = new StringBuilder();
                    SigSource.Append(UrlEncode(requestMethod.ToUpper(), Encoding.UTF8, true) + "&");
                    SigSource.Append(UrlEncode(regularUrl, Encoding.UTF8, true) + "&");
                    SigSource.Append(UrlEncode(joinedParam, Encoding.UTF8, true));

                    // Calcuate hash
                    using (HMACSHA1 hmacsha1 = new HMACSHA1())
                    {
                        hmacsha1.Key = Encoding.ASCII.GetBytes(string.Format("{0}&{1}", UrlEncode(consumerSecret, Encoding.UTF8, true), string.IsNullOrEmpty(tokenSecret) ? "" : UrlEncode(tokenSecret, Encoding.UTF8, true)));
                        return UrlEncode(ComputeHash(hmacsha1, SigSource.ToString()), Encoding.UTF8, false);
                    }
                case OAuthSignatureMethod.Rsa_Sha1:
                    throw new NotImplementedException();
                default:
                    throw new ArgumentException("Unknown signature type", "signatureType");
            }
        }

        private string GetNonce()
        {
            //Use guid
            return Guid.NewGuid().ToString("N");
        }

        private string GetTimestamp()
        {
            //Timestamp
            return UnixEpoch.GetUnixEpochByDateTime(DateTime.Now).ToString();
        }
        #endregion

        #region Utilities

        /// <summary>
        /// Split parameters into dictionary
        /// </summary>
        protected Dictionary<string, string> SplitParamDict(string param)
        {
            var retdict = new Dictionary<string, string>();
            foreach (var p in SplitParam(param))
            {
                if (retdict.ContainsKey(p.Key))
                    throw new InvalidOperationException();
                retdict.Add(p.Key, p.Value);
            }
            return retdict;
        }

        /// <summary>
        /// Split parameters and enumerate results
        /// </summary>
        protected IEnumerable<KeyValuePair<string, string>> SplitParam(string paramstring)
        {
            paramstring.TrimStart('?');
            if (String.IsNullOrEmpty(paramstring))
                yield break;
            var parray = paramstring.Split(new[] { "&" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var s in parray)
            {
                int idx = -1;
                if ((idx = s.IndexOf('=')) >= 0)
                {
                    yield return new KeyValuePair<string, string>(
                        s.Substring(0, idx), s.Substring(idx + 1));
                }
                else
                {
                    yield return new KeyValuePair<string, string>(s, String.Empty);
                }
            }
        }

        /// <summary>
        /// Join parameters as header format
        /// </summary>
        protected string JoinParamAsHeader(IEnumerable<KeyValuePair<string, string>> param)
        {
            if (param == null) return String.Empty;
            return String.Join(", ", from p in param
                                     orderby p.Key
                                     select p.Key + "=\"" + p.Value + "\"");
        }

        /// <summary>
        /// Join parameters as url format
        /// </summary>
        protected string JoinParamAsUrl(IEnumerable<KeyValuePair<string, string>> param)
        {
            if (param == null) return String.Empty;
            return String.Join("&", from p in param
                                    orderby p.Key
                                    select p.Key + "=" + p.Value);
        }

        private IEnumerable<KeyValuePair<string, string>> GetOAuthParams(
            string consumerKey, string token, string timeStamp, string nonce,
            OAuthSignatureMethod sigMethod, string verifier, bool gettingRequestToken)
        {
            if (String.IsNullOrEmpty(consumerKey))
                throw new ArgumentNullException("consumerKey");
            var np = new List<KeyValuePair<string, string>>();
            // original parameter
            np.Add(new KeyValuePair<string, string>(VersionKey, Version));
            np.Add(new KeyValuePair<string, string>(NonceKey, nonce));
            np.Add(new KeyValuePair<string, string>(TimestampKey, timeStamp));
            np.Add(new KeyValuePair<string, string>(SignatureMethodKey, GetOAuthSignatureMethodValue(sigMethod)));
            np.Add(new KeyValuePair<string, string>(ConsumerKeyKey, consumerKey));
            if (!String.IsNullOrEmpty(verifier))
                np.Add(new KeyValuePair<string, string>(VerifierKey, verifier));
            if (gettingRequestToken)
                np.Add(new KeyValuePair<string, string>(CallbackKey, "oob")); // out of band
            else if (!String.IsNullOrEmpty(token))
                np.Add(new KeyValuePair<string, string>(TokenKey, token));
            return np;
        }

        private string ComputeHash(HashAlgorithm algorithm, string raw)
        {
            if (algorithm == null)
                throw new ArgumentNullException("algorithm");
            if (String.IsNullOrEmpty(raw))
                throw new ArgumentNullException("raw");
            byte[] dat = Encoding.ASCII.GetBytes(raw);
            byte[] hash = algorithm.ComputeHash(dat);
            return Convert.ToBase64String(hash);
        }

        #endregion
    }
}
