using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Runtime.Serialization.Json;
using System.Xml.Linq;
using System.Net;
using Dulcet.Twitter.Credential;
using Dulcet.Util;

namespace Dulcet.Twitter.Streaming
{
    /// <summary>
    /// Using streaming interface type
    /// </summary>
    public enum StreamingType
    {
        /// <summary>
        /// firehose, All public information (limited)<para/>
        /// Arguments:<para/>
        ///   count: backlog length<para/>
        ///   delimited: data length(byte)
        /// </summary>
        firehose,
        /// <summary>
        /// sample, Partial public information (public)<para/>
        /// Arguments:<para/>
        ///   delimited: data length(byte)
        /// </summary>
        sample,
        /// <summary>
        /// filter, Filtered public information<para/>
        /// Filter implies user id or keywords.<para/>
        /// User id: max 400, Keyword: max 200(restricted: 10000 partner: 200000)<para/>
        /// Arguments:<para/>
        ///   count: backlog length<para/>
        ///   delimited: data length(byte)<para/>
        ///   follow: following user's id (delimiter: space)<para/>
        ///   track: tracking keyword(each keywords max 60 bytes, separated comma.)<para/>
        ///   locations: location area of tweet<para/>
        /// you must set follow or track argument.
        /// </summary>
        filter,
        /// <summary>
        /// link, tweets contains http(s) (private)<para/>
        /// Arguments:<para/>
        ///   delimited: data length(byte)
        /// </summary>
        links,
        /// <summary>
        /// retweet, retweeted tweets (private)<para/>
        /// Arguments:<para/>
        ///   delimited: data length(byte)
        /// </summary>
        retweet,
        /// <summary>
        /// User streaming (preview)<para />
        /// Arguments:<para />
        ///   count: backlog length<para/>
        ///   delimited: data length(byte)<para/>
        ///   track: tracking keyword(each keywords max 60 bytes, separated comma.)<para/>
        ///   locations: location area of tweet<para/>
        ///   replies: if you set this parameter as &quot;all&quot; twitter deriver all mentions of followings.<para />
        ///   with: if you set this parameter as user, twitter deriver limited events. See API document.
        /// </summary>
        user
    }

    /// <summary>
    /// Streaming connection core
    /// </summary>
    public class StreamingCore : IDisposable
    {
        /// <summary>
        /// Event handler on exception
        /// </summary>
        public event Action<Exception> OnExceptionThrown = _ => { };

        /// <summary>
        /// 接続が切断されました。<br />
        /// パラメータがfalseの時、予期しない切断であることを示します。
        /// </summary>
        public event Action<CredentialProvider, bool> OnDisconnected = (cp, expected) => { };

        /// <summary>
        /// Constructor
        /// </summary>
        public StreamingCore()
        {
            jsonParseWaiter = new ManualResetEvent(false);
            waiterQueue = new Queue<Tuple<CredentialProvider,string>>();
        }

        /// <summary>
        /// ストリーミング接続を行います。
        /// </summary>
        public StreamingConnection ConnectNew(
            CredentialProvider provider,
            StreamingDescription desc)
        {
            HttpWebRequest request;
            var streaming = provider.RequestStreamingAPI(
                GetStreamingUri(desc.Type),
                GetStreamingMethod(desc.Type),
                BuildArguments(desc), out request);
            if (streaming == null)
                throw new InvalidOperationException("接続に失敗しました。");
            var con = new StreamingConnection(this, provider, request, streaming);
            if (con == null)
                throw new InvalidOperationException("受信開始に失敗しました。");
            lock (conLock)
            {
                connections.Add(con);
            }
            return con;
        }

        #region Connection builder
        
        const string SapiV1 = "http://stream.twitter.com/1/statuses/{0}.json";
        private string GetStreamingUri(StreamingType type)
        {
            switch (type)
            {
                case StreamingType.firehose:
                case StreamingType.sample:
                case StreamingType.links:
                case StreamingType.retweet:
                case StreamingType.filter:
                    return String.Format(SapiV1, type.ToString());
                case StreamingType.user:
                    return "https://userstream.twitter.com/2/user.json";
                default:
                    throw new ArgumentOutOfRangeException("Invalid streaming value");
            }
        }

        private IEnumerable<KeyValuePair<string, string>> BuildArguments(StreamingDescription desc)
        {
            List<KeyValuePair<string, string>> args = new List<KeyValuePair<string, string>>();
            if (desc.Delimited != null)
                args.Add(new KeyValuePair<string, string>("delimited", desc.Delimited.Value.ToString()));
            if (desc.Count != null)
                args.Add(new KeyValuePair<string, string>("count", desc.Count.Value.ToString()));
            if (!String.IsNullOrWhiteSpace(desc.Follow))
                args.Add(new KeyValuePair<string, string>("follow", HttpUtility.UrlEncodeStrict(desc.Follow, Encoding.UTF8, true)));
            if (!String.IsNullOrWhiteSpace(desc.Track))
                args.Add(new KeyValuePair<string, string>("track", HttpUtility.UrlEncodeStrict(desc.Track, Encoding.UTF8, true)));
            if (!String.IsNullOrWhiteSpace(desc.Locations))
                args.Add(new KeyValuePair<string, string>("locations", HttpUtility.UrlEncodeStrict(desc.Locations, Encoding.UTF8, true)));
            if (desc.RepliesAll.GetValueOrDefault())
                args.Add(new KeyValuePair<string, string>("replies", "all"));
            if (!String.IsNullOrWhiteSpace(desc.With))
                args.Add(new KeyValuePair<string, string>("with", HttpUtility.UrlEncodeStrict(desc.With, Encoding.UTF8, true)));
            return args;
        }

        private CredentialProvider.RequestMethod GetStreamingMethod(StreamingType type)
        {
            switch (type)
            {
                case StreamingType.filter:
                    return CredentialProvider.RequestMethod.POST;
                default:
                    return CredentialProvider.RequestMethod.GET;
            }
        }

        #endregion

        #region Connection control

        private object conLock = new object();
        private List<StreamingConnection> connections = new List<StreamingConnection>();

        /// <summary>
        /// 生存している接続
        /// </summary>
        public IEnumerable<StreamingConnection> AliveConnections
        {
            get
            {
                lock (conLock)
                {
                    return this.connections.ToArray();
                }
            }
        }

        internal void UnregisterConnection(StreamingConnection connection)
        {
            lock (conLock)
            {
                connections.Remove(connection);
            }
        }

        internal void RaiseOnExceptionThrown(Exception exception)
        {
            this.OnExceptionThrown(exception);
        }

        internal void RaiseOnDisconnected(CredentialProvider provider, bool expected)
        {
            OnDisconnected(provider, expected);
        }

        #endregion

        #region JSON parsing

        ManualResetEvent jsonParseWaiter;
        Queue<Tuple<CredentialProvider, string>> waiterQueue;

        internal void EnqueueReceivedObject(CredentialProvider source, string json)
        {
            lock (waiterQueue)
            {
                waiterQueue.Enqueue(new Tuple<CredentialProvider, string>(source, json));
            }
            jsonParseWaiter.Set();
        }

        /// <summary>
        /// Enumerate received elements
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Tuple<CredentialProvider, StreamingEvent>> EnumerateStreamingElements()
        {
            return EnumerateQueuedStrings().Select(s =>
                {
                    using (var json = JsonReaderWriterFactory.CreateJsonReader(Encoding.UTF8.GetBytes(s.Item2),
                         System.Xml.XmlDictionaryReaderQuotas.Max))
                        return new Tuple<CredentialProvider, StreamingEvent>(
                            s.Item1,
                            StreamingEvent.FromNode(XElement.Load(json)));
                });
        }

        private IEnumerable<Tuple<CredentialProvider, string>> EnumerateQueuedStrings()
        {
            while (true)
            {
                jsonParseWaiter.WaitOne();
                while(waiterQueue.Count > 0)
                {
                    jsonParseWaiter.Reset();
                    Tuple<CredentialProvider, string> item = null;
                    lock (waiterQueue)
                    {
                        item = waiterQueue.Dequeue();
                    }
                    if (String.IsNullOrWhiteSpace(item.Item2))
                        continue;
                    yield return item;
                }
            }
        }

        #endregion

        /// <summary>
        /// Dispose core and all children.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// finalizer
        /// </summary>
        ~StreamingCore()
        {
            Dispose(false);
        }

        bool disposed = false;
        /// <summary>
        /// Disposed this core
        /// </summary>
        internal bool IsDisposed
        {
            get { return this.disposed; }
        }
        /// <summary>
        /// disposing this
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (disposed) return;
            disposed = true;
            if (waiterQueue != null)
                waiterQueue.Clear();
            if (jsonParseWaiter != null)
                jsonParseWaiter.Dispose();
            // 参照を固定しないとコレクションが変わる
            foreach (var child in AliveConnections.ToArray())
                child.Dispose();
        }


    }

    /// <summary>
    /// Streaming API description class
    /// </summary>
    public class StreamingDescription
    {
        /// <summary>
        /// Delimited parameter
        /// </summary>
        public int? Delimited { get; private set; }
        /// <summary>
        /// Count parameter
        /// </summary>
        public int? Count { get; private set; }
        /// <summary>
        /// Follow parameter
        /// </summary>
        public string Follow { get; private set; }
        /// <summary>
        /// Track parameter
        /// </summary>
        public string Track { get; private set; }
        /// <summary>
        /// Locations parameter
        /// </summary>
        public string Locations { get; private set; }
        /// <summary>
        /// RepliesAll parameter
        /// </summary>
        public bool? RepliesAll { get; private set; }
        /// <summary>
        /// StreamingType parameter
        /// </summary>
        public StreamingType Type { get; private set; }
        /// <summary>
        /// With parameter
        /// </summary>
        public string With { get; private set; }

        private StreamingDescription(
            StreamingType type, int? count = null, int? delimited = null, string follow = null,
            string track = null, string locations = null, bool? repliesAll = null, string with = null)
        {
            this.Type = type;
            this.Delimited = delimited;
            this.Count = count;
            this.Follow = follow;
            this.Track = track;
            this.Locations = locations;
            this.RepliesAll = repliesAll;
            this.With = with;
        }

        /// <summary>
        /// Parameters for firehose.
        /// </summary>
        public static StreamingDescription ForFirehose(
            int? count = null, int? delimited = null, string follow = null,
            string locations = null, string track = null)
        {
            return new StreamingDescription(StreamingType.firehose,
                count: count, delimited: delimited, follow: follow,
                locations: locations, track: track);
        }

        /// <summary>
        /// Parameters for filter.
        /// </summary>
        public static StreamingDescription ForFilter(
            int? count = null, int? delimited = null, string follow = null,
            string locations = null, string track = null)
        {
            return new StreamingDescription(StreamingType.filter,
                count: count, delimited: delimited, follow: follow,
                locations: locations, track: track);
        }

        /// <summary>
        /// Parameters for links.
        /// </summary>
        public static StreamingDescription ForLinks(
            int? count = null, int? delimited = null)
        {
            return new StreamingDescription(StreamingType.links,
                count: count, delimited: delimited);
        }

        /// <summary>
        /// Parameters for retweets.
        /// </summary>
        public static StreamingDescription ForRetweet(
            int? delimited = null)
        {
            return new StreamingDescription(StreamingType.retweet, delimited: delimited);
        }

        /// <summary>
        /// Parameters for sample.
        /// </summary>
        public static StreamingDescription ForSample(
            int? count = null, int? delimited = null)
        {
            return new StreamingDescription(StreamingType.sample,
                count: count, delimited: delimited);
        }

        /// <summary>
        /// Parameters for User Streams.
        /// </summary>
        public static StreamingDescription ForUserStreams(
            int? count = null, int? delimited = null, bool? repliesAll = null,
            string track = null, string with = null)
        {
            return new StreamingDescription(StreamingType.user,
                count: count, delimited: delimited, track: track, with: with, repliesAll: repliesAll);
        }
    }
}
