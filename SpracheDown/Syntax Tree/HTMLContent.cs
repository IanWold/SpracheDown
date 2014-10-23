using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpracheDown
{
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
}
