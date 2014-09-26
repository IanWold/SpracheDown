using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpracheDown
{
    public class HTMLDocument
    {
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

    public class HTMLItem { }

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

    public class HTMLNode : HTMLItem
    {
        public string Name { get; set; }
        public IEnumerable<HTMLAttribute> Attributes { get; set; }
        public IEnumerable<HTMLItem> Children { get; set; }

        public HTMLNode(string name)
        {
            Name = name;
        }

        public HTMLNode(string name, params HTMLItem[] children)
        {
            Name = name;
            Children = children;

            SortChildren();
        }

        public HTMLNode(string name, params HTMLAttribute[] attributes)
        {
            Name = name;
            Attributes = attributes;
        }

        public HTMLNode(string name, IEnumerable<HTMLItem> children, IEnumerable<HTMLAttribute> attributes = null)
        {
            Name = name;
            Children = children;
            Attributes = attributes;

            SortChildren();
        }

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

        string Tabify(string toTab)
        {
            var lines = toTab.Split('\n');
            var toReturn = "";

            foreach (var l in lines) toReturn += l + "\n" + "\t";

            return toReturn.Substring(0, toReturn.Length - 2);
        }

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

    public class HTMLAttribute
    {
        public string Name { get; set; }
        public string Value { get; set; }

        public HTMLAttribute() { }

        public HTMLAttribute(string name, string value)
        {
            Name = name;
            Value = value;
        }

        public override string ToString()
        {
            if ((Name != "" && Name != null) &&
                (Value != "" && Value != null)) return Name + "=\"" + Value + "\"";
            else return "";
        }
    }
}
