using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpracheDown
{
    /// <summary>
    /// Represents a document.
    /// </summary>
    public class HTMLDocument
    {
        /// <summary>
        /// The root node of the document.
        /// </summary>
        public HTMLNode Root { get; set; }

        public HTMLDocument() { }

        public HTMLDocument(HTMLNode root)
        {
            Root = root;
        }

        public override string ToString()
        {
            return Root.ToString();
        }
    }

    /// <summary>
    /// Either a node (tag) or content (a string).
    /// </summary>
    public class HTMLItem { }

    /// <summary>
    /// Represents plaintext in HTML.
    /// </summary>
    public class HTMLContent : HTMLItem
    {
        public string Text { get; set; }

        public HTMLContent() { }

        public HTMLContent(string text)
        {
            Text = text;
        }
        public override string ToString()
        {
            return Text;
        }
    }

    /// <summary>
    /// Represents an HTML node (or tag).
    /// </summary>
    public class HTMLNode : HTMLItem
    {
        /// <summary>
        /// The name of the tag.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The attributes of the tag, if any.
        /// </summary>
        public IEnumerable<HTMLAttribute> Attributes { get; set; }

        /// <summary>
        /// All the children HTMLItems of the node. Can be a mixture of content and nodes.
        /// </summary>
        public IEnumerable<HTMLItem> Children { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name">The name of the node.</param>
        public HTMLNode(string name)
        {
            Name = name;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name">The name of the node.</param>
        /// <param name="children">The children of the node.</param>
        public HTMLNode(string name, params HTMLItem[] children)
        {
            Name = name;
            Children = children;

            SortChildren();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name">The name of the node.</param>
        /// <param name="attributes">The attributes of the node.</param>
        public HTMLNode(string name, params HTMLAttribute[] attributes)
        {
            Name = name;
            Attributes = attributes;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name">The name of the node.</param>
        /// <param name="children">The children of the node.</param>
        /// <param name="attributes">The attributes of the node (null by default).</param>
        public HTMLNode(string name, IEnumerable<HTMLItem> children, IEnumerable<HTMLAttribute> attributes = null)
        {
            Name = name;
            Children = children;
            Attributes = attributes;

            SortChildren();
        }

        /// <summary>
        /// This sorts through all the elements in Children, and combines together any HTMLContent items into coherent HTMLContent items.
        /// </summary>
        void SortChildren()
        {
            var sortList = new List<HTMLItem>();

            foreach (var c in Children)
            {
                if (c.GetType().Equals(this.GetType())) sortList.Add(c);
                else
                {
                    if (sortList.Count == 0) sortList.Add(c);
                    else if (sortList.ElementAt(sortList.Count - 1).GetType().Equals(this.GetType())) sortList.Add(c);
                    else (sortList.ElementAt(sortList.Count - 1) as HTMLContent).Text += (c as HTMLContent).Text;
                }
            }

            Children = sortList as IEnumerable<HTMLItem>;
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

            foreach (var l in lines) toReturn += l + "\n" + "\t";

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
                foreach (var c in Children) toReturn += "\r\n" + "\t" + Tabify(c.ToString());
                toReturn += "\r\n" + "</" + Name + ">";
            }
            else if (Attributes != null)
            {
                toReturn = "<" + Name + GetAttributes() + "/>";
            }
            else toReturn = "<" + Name + "/>";

            return toReturn;
        }
    }

    /// <summary>
    /// An attribute for an HTML tag.
    /// </summary>
    public class HTMLAttribute
    {
        /// <summary>
        /// The name of the attribute.
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// The value to which the attribute is set.
        /// </summary>
        public string Value { get; set; }

        public HTMLAttribute() { }

        public HTMLAttribute(string name, string value)
        {
            Name = name;
            Value = value;
        }

        /// <summary>
        /// Returns a string representing the attribute, to be inserted in an HTML tag.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if ((Name != "" && Name != null) &&
                (Value != "" && Value != null)) return Name + "=\"" + Value + "\"";
            else return "";
        }
    }
}
