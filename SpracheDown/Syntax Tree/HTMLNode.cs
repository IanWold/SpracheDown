using System.Collections.Generic;
using System.Linq;

namespace SpracheDown
{
    /// <summary>
    /// Represents an HTML node (or tag).
    /// </summary>
    public class HtmlNode : IHtmlItem
    {
        /// <summary>
        /// The name of the tag.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The attributes of the tag, if any.
        /// </summary>
        public Dictionary<string, string> Attributes { get; set; }

        /// <summary>
        /// All the children HTMLItems of the node. Can be a mixture of content and nodes.
        /// </summary>
        public IEnumerable<IHtmlItem> Children { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name">The name of the node.</param>
        public HtmlNode(string name)
            : this(name, new List<IHtmlItem>(), new Dictionary<string, string>())
        { }

        public HtmlNode(string name, IHtmlItem child)
            : this(name, new List<IHtmlItem> { child }, new Dictionary<string, string>())
        { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name">The name of the node.</param>
        /// <param name="children">The children of the node.</param>
        public HtmlNode(string name, IEnumerable<IHtmlItem> children)
            : this(name, children, new Dictionary<string, string>())
        { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name">The name of the node.</param>
        /// <param name="attributes">The attributes of the node.</param>
        public HtmlNode(string name, Dictionary<string, string> attributes)
            : this(name, new List<IHtmlItem>(), attributes)
        { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name">The name of the node.</param>
        /// <param name="children">The children of the node.</param>
        /// <param name="attributes">The attributes of the node (null by default).</param>
        public HtmlNode(string name, IEnumerable<IHtmlItem> children, Dictionary<string, string> attributes)
        {
            Name = name;
            Children = children;
            Attributes = attributes;

            SortChildren();
        }

        /// <summary>
        /// Sort through all the elements in Children, and combine any sequential HtmlValue items into single HtmlValue items.
        /// </summary>
        void SortChildren()
        {
            var sortList = new List<IHtmlItem>();

            foreach (var c in Children)
            {
                if (sortList.Count == 0
                    || c is HtmlNode
                    || sortList.ElementAt(sortList.Count - 1) is HtmlNode)
                {
                    sortList.Add(c);
                }
                else if (c is HtmlValue currentValue
                    && sortList.ElementAt(sortList.Count - 1) is HtmlValue nextValue)
                {
                    nextValue.Value += currentValue.Value;
                }
            }

            Children = sortList;
        }

        /// <summary>
        /// Gets the attributes in string form.
        /// </summary>
        /// <returns>Returns a string representing the attributes, to be inserted in an HTML tag.</returns>
        string GetAttributes()
        {
            var toReturn = "";

            if (Attributes != null)
            {
                foreach (var a in Attributes)
                {
                    if (a.ToString() != "") toReturn += " " + a.ToString();
                }
            }

            return toReturn;
        }

        /// <summary>
        /// Breaks a string into separate lines and inserts a tab character in front of each
        /// </summary>
        /// <param name="toTab">The string to be tabified</param>
        /// <returns>A string with tab characters following each newline</returns>
        string Tabify(string toTab)
        {
            var lines = toTab.Split('\n');
            var toReturn = "";

            foreach (var l in lines)
            {
                toReturn += l + "\n" + "\t";
            }

            return toReturn.Substring(0, toReturn.Length - 2);
        }

        /// <summary>
        /// Returns the string form of the node in HTML form.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            var toReturn = "";

            if (Children != null)
            {
                toReturn += "<" + Name + GetAttributes() + ">";
                foreach (var c in Children)
                {
                    toReturn += "\r\n" + "\t" + Tabify(c.ToString());
                }
                toReturn += "\r\n" + "</" + Name + ">";
            }
            else
            {
                toReturn = Attributes != null
                    ? "<" + Name + GetAttributes() + "/>"
                    : toReturn = "<" + Name + "/>";
            }

            return toReturn;
        }
    }
}
