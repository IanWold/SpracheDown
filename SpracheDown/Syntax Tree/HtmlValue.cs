namespace SpracheDown
{
    /// <summary>
    /// Represents plaintext in HTML.
    /// </summary>
    public class HtmlValue : IHtmlItem
    {
        public string Value { get; set; }

        public HtmlValue() { }

        public HtmlValue(string value) =>
            Value = value;

        public override string ToString() =>
            Value;
    }
}
