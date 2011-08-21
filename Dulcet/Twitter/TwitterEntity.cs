using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace Dulcet.Twitter
{
    public abstract class TwitterEntity
    {
        public static TwitterEntityNode Parse(XElement entityNode)
        {
            if (entityNode == null || entityNode.Name != "entities")
                return null;
            else
                return GetEntity(entityNode) as TwitterEntityNode;
        }

        public static TwitterEntity GetEntity(XElement node)
        {
            List<TwitterEntity> CurrentNode = new List<TwitterEntity>();
            if (node.HasAttributes)
            {
                node.Attributes().Select(a => new TwitterEntityValue(a.Name.LocalName, a.Value));
            }
            if (node.HasElements)
            {
                return new TwitterEntityNode(node.Name.LocalName, node.Elements().Select(s => GetEntity(s)));
            }
            else
            {
                return new TwitterEntityValue(node.Name.LocalName, node.Value);
            }
        }

        public string Name { get; private set; }

        public TwitterEntity(string name)
        {
            this.Name = name;
        }
    }

    public class TwitterEntityNode : TwitterEntity
    {
        public IEnumerable<TwitterEntity> Children { get; private set; }

        public TwitterEntityNode(string nodeName, IEnumerable<TwitterEntity> children) : base(nodeName)
        {
            this.Children = children.ToArray();
        }

        public bool IsExisted(string name)
        {
            return this.GetChild(name) != null;
        }

        public IEnumerable<TwitterEntity> GetChildren(string name)
        {
            return this.Children.Where(f => f.Name == name);
        }

        public TwitterEntity GetChild(string name)
        {
            return this.Children.FirstOrDefault(f => f.Name == name);
        }

        public TwitterEntityNode GetChildNode(string name)
        {
            return this.GetChild(name) as TwitterEntityNode;
        }

        public IEnumerable<TwitterEntityNode> GetChildNodes(string name)
        {
            return this.GetChildren(name).OfType<TwitterEntityNode>();
        }

        public TwitterEntityValue GetChildValue(string name)
        {
            return this.GetChild(name) as TwitterEntityValue;
        }

        public IEnumerable<TwitterEntityValue> GetChildValues(string name)
        {
            return this.GetChildren(name).OfType<TwitterEntityValue>();
        }

        public override string ToString()
        {
            return "[" + this.Name + Environment.NewLine +
                String.Join(Environment.NewLine, this.Children.SelectMany(s => s.ToString().Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)).Select(s => "  " + s)) +
                Environment.NewLine + "]";
        }
    }

    public class TwitterEntityValue : TwitterEntity
    {
        public string Value { get; private set; }

        public TwitterEntityValue(string nodeName, string value) : base(nodeName)
        {
            this.Value = value;
        }

        public override string ToString()
        {
            return this.Name + ": " + this.Value;
        }
    }
}
