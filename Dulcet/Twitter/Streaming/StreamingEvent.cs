using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Dulcet.Util;

namespace Dulcet.Twitter.Streaming
{
    /// <summary>
    /// Element kind
    /// </summary>
    public enum ElementKind
    {
        /// <summary>
        /// Status
        /// </summary>
        Status,
        /// <summary>
        /// Delete operation
        /// </summary>
        Delete,
        /// <summary>
        /// User enumerations
        /// </summary>
        UserEnumerations,
        /// <summary>
        /// Following
        /// </summary>
        Follow,
        /// <summary>
        /// Unfollow (remove) [CURRENTLY DISABLED]
        /// </summary>
        Unfollow,
        /// <summary>
        /// Favorite
        /// </summary>
        Favorite,
        /// <summary>
        /// Unfavorite
        /// </summary>
        Unfavorite,
        /// <summary>
        /// Direct message
        /// </summary>
        DirectMessage,
        /// <summary>
        /// List created
        /// </summary>
        ListCreated,
        /// <summary>
        /// List deleted
        /// </summary>
        ListDeleted,
        /// <summary>
        /// List information updated
        /// </summary>
        ListUpdated,
        /// <summary>
        /// Added list subscription
        /// </summary>
        ListSubscribed,
        /// <summary>
        /// Deleted list subscription
        /// </summary>
        ListUnsubscribed,
        /// <summary>
        /// List member added
        /// </summary>
        ListMemberAdded,
        /// <summary>
        /// List member removed
        /// </summary>
        ListMemberRemoved,
        /// <summary>
        /// User blocked
        /// </summary>
        Blocked,
        /// <summary>
        /// User unblocked
        /// </summary>
        Unblocked,
        /// <summary>
        /// Undefined
        /// </summary>
        Undefined
    }

    public class StreamingEvent
    {

        /// <summary>
        /// Purse xml and create element
        /// </summary>
        public static StreamingEvent FromNode(XElement node)
        {
            return new StreamingEvent(node);
        }

        /// <summary>
        /// Create StreamingEvent
        /// </summary>
        public StreamingEvent(TwitterStatusBase tbase)
        {
            this.Status = tbase;
            this.SourceUser = tbase.User;
            var tstatus = tbase as TwitterStatus;
            if (tstatus != null)
            {
                this.Kind = ElementKind.Status;
            }
            else
            {
                this.Kind = ElementKind.DirectMessage;
            }
        }

        /// <summary>
        /// Create StreamingEvent
        /// </summary>
        public StreamingEvent(TwitterStatus tstatus)
        {
            this.Kind = ElementKind.Status;
            this.Status = tstatus;
            this.SourceUser = tstatus.User;
        }

        /// <summary>
        /// Create StreamingEvent
        /// </summary>
        public StreamingEvent(TwitterDirectMessage dmsg)
        {
            this.Kind = ElementKind.DirectMessage;
            this.Status = dmsg;
            this.SourceUser = dmsg.User;
        }

        /// <summary>
        /// constructor
        /// </summary>
        protected StreamingEvent(XElement node)
        {
            Kind = ElementKind.Undefined;
            RawXElement = node;
            var eventstr = node.Element("event").ParseString();
            if (String.IsNullOrWhiteSpace(eventstr))
            {
                // Status, Delete, UserEnumerations, or Undefined.
                if (node.Element("delete") != null)
                {
                    // delete
                    ParseDelete(node);
                }
                else if (node.Element("text") != null && node.Element("user") != null)
                {
                    // status
                    ParseStatus(node);
                }
                else if (node.Element("direct_message") != null)
                {
                    // direct message
                    ParseDirectMessage(node);
                }
                else if (node.Element("friends") != null)
                {
                    // user enumerations
                    ParseUserEnumerations(node);
                }
                else
                {
                    // undefined
                    ParseUndefined(node);
                }
            }
            else
            {
                // Follow, Favorite, Retweet, ListMemberAdded, or Undefined.
                switch (eventstr)
                {
                    case "follow":
                        // follow
                        ParseFollowChanged(node, true);
                        break;
                    case "unfollow":
                    case "remove":
                        // unfollow(remove)
                        // currently unsupported
                        ParseFollowChanged(node, false);
                        break;
                    case "favorite":
                        // favorite
                        ParseFavoriteChanged(node, true);
                        break;
                    case "unfavorite":
                        // unfavorite
                        ParseFavoriteChanged(node, false);
                        break;
                    case "list_user_subscribed":
                        // list user subscribed
                        ParseListSubscribeChanged(node, true);
                        break;
                    case "list_user_unsubscribed":
                        // list user unsubscribed
                        ParseListSubscribeChanged(node, false);
                        break;
                    case "list_created":
                        // list created
                        ParseListCreateChanged(node, true);
                        break;
                    case "list_destroyed":
                        ParseListCreateChanged(node, false);
                        break;
                    case "list_updated":
                        ParseListUpdated(node);
                        break;
                    case "list_member_added":
                        // list member added
                        ParseListMemberChanged(node, true);
                        break;
                    case "list_member_removed":
                        // list member removed
                        ParseListMemberChanged(node, false);
                        break;
                    case "block":
                        // blocked
                        ParseBlockChanged(node, true);
                        break;
                    case "unblock":
                        // unblocked
                        ParseBlockChanged(node, false);
                        break;
                    default:
                        ParseUndefined(node);
                        break;
                }
            }
        }

        private void ParseUndefined(XElement node)
        {
            Kind = ElementKind.Undefined;
        }

        #region Implicit switch

        private void ParseDelete(XElement node)
        {
            Kind = ElementKind.Delete;
            DeletedStatusId = node.Element("id").ParseLong();
        }

        private void ParseStatus(XElement node)
        {
            Kind = ElementKind.Status;
            Status = TwitterStatus.FromNode(node);
        }

        private void ParseDirectMessage(XElement node)
        {
            Kind = ElementKind.DirectMessage;
            Status = TwitterDirectMessage.FromNode(node.Element("direct_message"));
        }
        
        private void ParseUserEnumerations(XElement node)
        {
            Kind = ElementKind.UserEnumerations;
            UserEnumerations = from item in node.Elements("item")
                               select item.ParseLong();
        }

        #endregion

        #region Explicit switch

        private void ParseFollowChanged(XElement node, bool follow)
        {
            Kind = follow ? ElementKind.Follow : ElementKind.Unfollow;
            ParseSourceTargetUsers(node);
        }

        private void ParseFavoriteChanged(XElement node, bool created)
        {
            Kind = created ? ElementKind.Favorite : ElementKind.Unfavorite;
            ParseSourceTargetUsers(node);
            ParseTargetStatus(node);
        }

        private void ParseListSubscribeChanged(XElement node, bool subscribed)
        {
            Kind = subscribed ? ElementKind.ListSubscribed : ElementKind.ListUnsubscribed;
            ParseSourceTargetUsers(node);
            ParseTargetList(node);
        }

        private void ParseListUpdated(XElement node)
        {
            Kind = ElementKind.ListUpdated;
            ParseSourceTargetUsers(node);
            ParseTargetList(node);
        }

        private void ParseListCreateChanged(XElement node, bool created)
        {
            Kind = created ? ElementKind.ListCreated : ElementKind.ListDeleted;
            ParseSourceTargetUsers(node);
            ParseTargetList(node);
        }

        private void ParseListMemberChanged(XElement node, bool added)
        {
            Kind = added ? ElementKind.ListMemberAdded : ElementKind.ListMemberRemoved;
            ParseSourceTargetUsers(node);
            ParseTargetList(node);
        }

        private void ParseBlockChanged(XElement node, bool blocked)
        {
            Kind = blocked ? ElementKind.Blocked : ElementKind.Unblocked;
            ParseSourceTargetUsers(node);
        }

        private void ParseSourceTargetUsers(XElement node)
        {
            var source = node.Element("source");
            var target = node.Element("target");
            if (source == null || target == null)
                ParseUndefined(node);
            SourceUser = TwitterUser.FromNode(source);
            TargetUser = TwitterUser.FromNode(target);
        }

        private void ParseTargetStatus(XElement node)
        {
            var to = node.Element("target_object");
            if (to == null)
                return;
            Status = TwitterStatus.FromNode(to);
        }

        private void ParseTargetList(XElement node)
        {
            var to = node.Element("target_object");
            if (to == null)
                return;
            TargetList = TwitterList.FromNode(to);
        }

        #endregion

        
        /// <summary>
        /// Raw XElement object
        /// </summary>
        public XElement RawXElement { get; private set; }

        /// <summary>
        /// Kind of this element
        /// </summary>
        public ElementKind Kind { get; private set; }

        /// <summary>
        /// Status ID (uses notifing deleted status)
        /// </summary>
        public long DeletedStatusId { get; private set; }

        /// <summary>
        /// Status instance
        /// </summary>
        public TwitterStatusBase Status { get; private set; }

        /// <summary>
        /// User enumerations
        /// </summary>
        public IEnumerable<long> UserEnumerations { get; private set; }

        /// <summary>
        /// Source user
        /// </summary>
        public TwitterUser SourceUser { get; private set; }

        /// <summary>
        /// Target user
        /// </summary>
        public TwitterUser TargetUser { get; private set; }

        /// <summary>
        /// Target list
        /// </summary>
        public TwitterList TargetList { get; private set; }
    }
}
