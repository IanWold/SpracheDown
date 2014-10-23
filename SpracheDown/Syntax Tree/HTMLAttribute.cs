using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpracheDown
{
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
