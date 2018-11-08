using Sprache;
using System.Collections.Generic;
using System.Linq;

namespace SpracheDown
{
    public static class MarkdownParser
    {
        /// <summary>
        /// Parse markdown text into an HtmlNode.
        /// </summary>
        /// <param name="toParse">The string (markdown text) to be parsed.</param>
        /// <returns>An HtmlNode representing the output HTML.</returns>
        public static HtmlNode ParseDocument(string toParse)
        {
            return new HtmlNode("html", new List<IHtmlItem>() { new HtmlNode("head"), ParseBody(toParse) });

        }

        /// <summary>
        /// Parse markdown text into a single HTML body node.
        /// </summary>
        /// <param name="toParse">The string (markdown text) to be parsed.</param>
        /// <returns>Returns an HtmlNode representing the output HTML body node.</returns>
        public static HtmlNode ParseBody(string toParse)
        {
            return new HtmlNode("body", TermList.Parse(toParse));
        }

        /// <summary>
        /// A parser which establishes several characters the parser regularly cannot encounter when parsing plain text.
        /// </summary>
        static readonly Parser<char> RegularException =
            Parse.Char('\r')
            .Or(Parse.Char('_'))
            .Or(Parse.Char('*'))
            .Or(Parse.Char('['))
            .Or(Parse.Char('`'))
            .Or(Parse.Char('\\'));

        /// <summary>
        /// A parser which parses plain text - any character(s) except those which are specified.
        /// </summary>
        /// <param name="except">The character(s) the parser cannot encounter.</param>
        /// <returns>Returns an HtmlValue object representing the parsed text.</returns>
        static Parser<IHtmlItem> PlainText(Parser<char> except)
        {
            return Parse.AnyChar.Except(except).AtLeastOnce().Text()
                .Select(i => new HtmlValue(i));
        }

        /// <summary>
        /// Parses a line break within a paragraph.
        /// It does not parse two consecutive line breaks, which signifies the input may contain a different element.
        /// </summary>
        static readonly Parser<IHtmlItem> LineBreak =
            from line in Parse.String("\r\n").Except(Parse.String("\r\n\r\n"))
            select new HtmlNode("br");

        /// <summary>
        /// Parses emphasized text.
        /// </summary>
        static readonly Parser<IHtmlItem> EmText =
            from star1 in Parse.String("*").Or(Parse.String("_")).Text()
            from content in PlainText(RegularException)
            from star2 in star1 == "*" ? Parse.String("*") : Parse.String("_")
            select new HtmlNode("em", content);

        /// <summary>
        /// Parses strong text.
        /// </summary>
        static readonly Parser<IHtmlItem> StrongText =
            from star1 in Parse.String("**").Or(Parse.String("__")).Text()
            from content in PlainText(RegularException)
            from star2 in star1 == "**" ? Parse.String("**") : Parse.String("__")
            select new HtmlNode("strong", content);

        /// <summary>
        /// Parses inline code.
        /// </summary>
        static readonly Parser<IHtmlItem> CodeText =
            from star1 in Parse.String("`")
            from content in PlainText(RegularException)
            from star2 in Parse.String("`")
            select new HtmlNode("code", content);

        /// <summary>
        /// Parses an inline image.
        /// </summary>
        static readonly Parser<IHtmlItem> MDInlineImage =
            from exclaim in Parse.String("!")
            from lbracket in Parse.String("[")
            from text in Parse.AnyChar.Except(Parse.Char(']').Or(Parse.Char('\r'))).Many().Text()
            from rbracket in Parse.String("]")
            from lparen in Parse.String("(")
            from link in PlainText(Parse.Char(')').Or(Parse.Char('\r')).Or(Parse.Char('"')).Or(Parse.WhiteSpace))
            from rparen in Parse.String(")")
            select new HtmlNode("img", new Dictionary<string, string> { { "alt", text }, { "src", link.ToString() } });

        /// <summary>
        /// Parses an inline link, which includes an optional title attribute.
        /// </summary>
        static readonly Parser<IHtmlItem> MDInlineLink =
            from lbracket in Parse.String("[")
            from text in Parse.AnyChar.Except(Parse.Char(']').Or(Parse.Char('\r'))).Many().Text()
            from rbracket in Parse.String("]")
            from lparen in Parse.String("(")
            from link in PlainText(Parse.Char(')').Or(Parse.Char('\r')).Or(Parse.Char('"')).Or(Parse.WhiteSpace))
            from title in
                (from ws in Parse.Char(' ').Many()
                 from lquote in Parse.String("\"")
                 from _text in Parse.AnyChar.Except(Parse.Char('"')).Many().Text()
                 from rquote in Parse.String("\"")
                 select _text).Optional()
            from rparen in Parse.String(")")
            select new HtmlNode("a",
                                new List<IHtmlItem> { new HtmlValue(text) },
                                new Dictionary<string, string>
                                {
                                    { "href", link.ToString() },
                                    { "title", title.GetOrDefault() }
                                });
        /// <summary>
        /// Parses a single character which cannot be regularly parsed in plain text.
        /// Requires a backslash to precede the character.
        /// </summary>
        static readonly Parser<IHtmlItem> ForbiddenCharacter =
            from back in Parse.String("\\")
            from character in RegularException
            select new HtmlValue(character.ToString());

        /// <summary>
        /// Parses any text which may be stylized by markdown, save for a line break.
        /// </summary>
        static readonly Parser<IHtmlItem> MajorFormattedText =
            ForbiddenCharacter
            .Or(StrongText)
            .Or(EmText)
            .Or(CodeText)
            .Or(MDInlineLink)
            .Or(MDInlineImage)
            .Or(PlainText(RegularException));

        /// <summary>
        /// Parses any text which may be stylized by markdown, including the line break.
        /// </summary>
        static readonly Parser<IEnumerable<IHtmlItem>> FormattedText = LineBreak.Or(MajorFormattedText).Many();

        /// <summary>
        /// Parses FormattedText or an empty line into a paragraph node.
        /// </summary>
        static readonly Parser<IHtmlItem> Paragraph =
            from items in FormattedText
            select items.Count() != 0 ? new HtmlNode("p", items) : new HtmlNode("p");

        /// <summary>
        /// Parses a header denoted with "=" signs.
        /// </summary>
        static readonly Parser<IHtmlItem> Header1 =
            from text in Parse.AnyChar.Except(Parse.Char('\r')).AtLeastOnce().Text()
            from newline in Parse.String("\r\n")
            from lineBegin in Parse.String("==")
            from lineFinish in Parse.String("=").AtLeastOnce()
            select new HtmlNode("h1", new HtmlValue(text));

        /// <summary>
        /// Parses a header denoted with "-" signs.
        /// </summary>
        static readonly Parser<IHtmlItem> Header2 =
            from text in Parse.AnyChar.Except(Parse.Char('\r')).AtLeastOnce().Text()
            from newline in Parse.String("\r\n")
            from lineBegin in Parse.String("--")
            from lineFinish in Parse.String("-").AtLeastOnce()
            select new HtmlNode("h2", new HtmlValue(text));

        /// <summary>
        /// Parses any header denoted by using "#" signs.
        /// </summary>
        static readonly Parser<IHtmlItem> HashtagHeader =
            from tags in Parse.String("######")
                         .Or(Parse.String("#####"))
                         .Or(Parse.String("####"))
                         .Or(Parse.String("###"))
                         .Or(Parse.String("##"))
                         .Or(Parse.String("#")).Text()
            from text in Parse.AnyChar.Except(Parse.Char('\r')).AtLeastOnce().Text()
            select new HtmlNode("h" + tags.Length.ToString(), new HtmlValue(text));

        /// <summary>
        /// Parses any MarkDown header.
        /// </summary>
        static readonly Parser<IHtmlItem> Header = HashtagHeader.Or(Header1).Or(Header2);

        /// <summary>
        /// Parses a "block" of code.
        /// </summary>
        static readonly Parser<HtmlNode> CodeBlock =
            from first in Parse.String("```")
            from line in Parse.String("\r\n")
            from text in Parse.AnyChar.Except(Parse.String("```")).Many().Text()
            from last in Parse.String("```")
            select new HtmlNode("code", new HtmlValue(text));

        /// <summary>
        /// Parses a custom list item. Flexible for parsing bulletted and numbered list items, including nested lists.
        /// </summary>
        /// <typeparam name="T">The type of the bullet parser.</typeparam>
        /// <param name="bullet">The parser which parses the bullet of the list.</param>
        /// <param name="nodeName">The name of the node any nested lists will be put into.</param>
        /// <returns>Returns an HtmlNode representing the list item (or nested list) that was parsed.</returns>
        static Parser<HtmlNode> MDListItem<T>(Parser<T> bullet, string nodeName)
        {
            return
                from ws in Parse.Char(' ').Many()
                from star in bullet
                from space in Parse.Char(' ')
                from text in Parse.AnyChar.Except(Parse.Char('\r')).AtLeastOnce().Text()
                select GetNestedNode(new HtmlValue(text), nodeName, ws.Count());
        }

        /// <summary>
        /// Calculates the nesting of a list.
        /// </summary>
        /// <param name="toNest">The base IIHtmlItem to be nested.</param>
        /// <param name="nestName">The name of the nodes into which toNest should be nested.</param>
        /// <param name="nestCount">The degree to which toNest should be nested.</param>
        /// <returns>An HtmlNode representing the nested list item.</returns>
        static HtmlNode GetNestedNode(IHtmlItem toNest, string nestName, int nestCount)
        {
            if (nestCount == 0) return new HtmlNode("li", toNest);
            else return new HtmlNode(nestName, GetNestedNode(toNest, nestName, nestCount - 1));
        }

        /// <summary>
        /// Parses a bullet for a bulletted list.
        /// </summary>
        static readonly Parser<char> ListBullet =
            Parse.Char('*')
            .Or(Parse.Char('+'))
            .Or(Parse.Char('-'));

        /// <summary>
        /// Parses a bullet for a numbered list (a number followed by a dot).
        /// </summary>
        static readonly Parser<IEnumerable<char>> ListDigit =
            from num in Parse.Digit.AtLeastOnce()
            from dot in Parse.String(".")
            select num;

        /// <summary>
        /// Parses a bulletted list.
        /// </summary>
        static readonly Parser<HtmlNode> MDBulletList =
            MDListItem(ListBullet, "ul").DelimitedBy(Parse.String("\r\n").Once()).Select(i => new HtmlNode("ul", i));

        /// <summary>
        /// Parses a numbered list.
        /// </summary>
        static readonly Parser<HtmlNode> MDNumberList =
            MDListItem(ListDigit, "ol").DelimitedBy(Parse.String("\r\n").Once()).Select(i => new HtmlNode("ol", i));

        /// <summary>
        /// Parses any list.
        /// </summary>
        static readonly Parser<HtmlNode> MDList = MDNumberList.XOr(MDBulletList);

        /// <summary>
        /// Parses one line of a blockquote.
        /// </summary>
        static readonly Parser<string> BlockQuoteLine =
            from first in Parse.String(">").Once()
            from ws in Parse.String(" ").Optional()
            from text in Parse.AnyChar.Except(Parse.Char('\r')).Many().Text()
            select text;

        /// <summary>
        /// Parses an entire line of a blockquote, being sure to parse the contents within the blockquote as regular markdown.
        /// </summary>
        static readonly Parser<HtmlNode> BlockQuote =
            from lines in BlockQuoteLine.DelimitedBy(Parse.String("\r\n").Once())
            select new HtmlNode("blockquote", TermList.Parse(GetConnectedString(lines, "\r\n")));

        /// <summary>
        /// Accepts an IEnumerable of strings and returns one single string having concatenated them all.
        /// </summary>
        /// <param name="strings">The IEnumerable of strings to be concatenated.</param>
        /// <param name="addition">An optional string to be added to the end of each string in strings.</param>
        /// <returns>The string resulting from the concatenation.</returns>
        static string GetConnectedString(IEnumerable<string> strings, string addition = "")
        {
            var toReturn = "";
            foreach (var s in strings) toReturn += s.ToString() + addition;
            return toReturn;
        }

        static readonly Parser<string> Identifier =
            from first in Parse.Letter.Once()
            from rest in Parse.LetterOrDigit.XOr(Parse.Char('-')).XOr(Parse.Char('_')).Many()
            select new string(first.Concat(rest).ToArray());

        static Parser<T> Tag<T>(Parser<T> content)
        {
            return from lt in Parse.Char('<')
                   from t in content
                   from gt in Parse.Char('>').Token()
                   select t;
        }

        static readonly Parser<string> BeginTag = Tag(Identifier);

        static Parser<string> EndTag(string name)
        {
            return Tag(from slash in Parse.Char('/')
                       from id in Identifier
                       where id == name
                       select id).Named("closing tag for " + name);
        }

        static readonly Parser<HtmlValue> Content =
            from chars in Parse.CharExcept('<').Many()
            select new HtmlValue(new string(chars.ToArray()));

        static readonly Parser<HtmlNode> FullNode =
            from tag in BeginTag
            from nodes in Parse.Ref(() => Item).Many()
            from end in EndTag(tag)
            select new HtmlNode(tag, nodes);

        static readonly Parser<HtmlNode> ShortNode = Tag(from id in Identifier
                                                         from slash in Parse.Char('/')
                                                         select new HtmlNode(id));

        static readonly Parser<HtmlNode> HtmlNode = ShortNode.Or(FullNode);

        static readonly Parser<IHtmlItem> Item = HtmlNode.Select(n => (IHtmlItem)n).XOr(Content);

        /// <summary>
        /// Parses a single MarkDown term.
        /// </summary>
        static readonly Parser<IHtmlItem> MDTerm = Header.Or(MDList).Or(BlockQuote).Or(CodeBlock).Or(HtmlNode).Or(Paragraph);

        /// <summary>
        /// Parses a list of MarkDown terms, separated by two newlines each.
        /// </summary>
        static readonly Parser<IEnumerable<IHtmlItem>> TermList =
            from first in Parse.WhiteSpace.Many().Optional()
            from terms in MDTerm.DelimitedBy(Parse.String("\r\n\r\n"))
            from rest in Parse.WhiteSpace.Many().Optional().End()
            select terms;
    }
}
