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

        public HTMLDocument(HTMLNode root) =>
            Root = root;

        public override string ToString() =>
            Root.ToString();
    }
}
