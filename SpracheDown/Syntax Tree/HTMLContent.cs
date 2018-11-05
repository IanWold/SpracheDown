namespace SpracheDown
{
    /// <summary>
    /// Represents plaintext in HTML.
    /// </summary>
    public class HTMLContent : HTMLItem
    {
        public string Text { get; set; }

        public HTMLContent() { }

        public HTMLContent(string text) =>
            Text = text;

        public override string ToString() =>
            Text;
    }
}
