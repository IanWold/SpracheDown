using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sprache;
using System.Collections.ObjectModel;

namespace SpracheDown
{
    public static class MarkdownParser
    {
        /// <summary>
        /// Parse markdown text into an HTMLDocument.
        /// </summary>
        /// <param name="toParse">The string (markdown text) to be parsed.</param>
        /// <returns>An HTMLDocument representing the output HTML.</returns>
        public static HTMLDocument ParseDocument(string toParse)
        {
            return new HTMLDocument(
                new HTMLNode("html", new HTMLNode("head"), ParseBody(toParse))
                );
        }

        /// <summary>
        /// Parse markdown text into a single HTML body node.
        /// </summary>
        /// <param name="toParse">The string (markdown text) to be parsed.</param>
        /// <returns>Returns an HTMLNode representing the output HTML body node.</returns>
        public static HTMLNode ParseBody(string toParse)
        {
            return new HTMLNode("body", TermList.Parse(toParse));
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
        /// <returns>Returns an HTMLContent object representing the parsed text.</returns>
        static Parser<HTMLItem> PlainText(Parser<char> except)
        {
            return Parse.AnyChar.Except(except).AtLeastOnce().Text()
                .Select(i => new HTMLContent(i));
        }

        /// <summary>
        /// Parses a line break within a paragraph.
        /// It does not parse two consecutive line breaks, which signifies the input may contain a different element.
        /// </summary>
        static readonly Parser<HTMLItem> LineBreak =
            from line in Parse.String("\r\n").Except(Parse.String("\r\n\r\n"))
            select new HTMLNode("br");

        /// <summary>
        /// Parses emphasized text (see MarkDown specification).
        /// </summary>
        static readonly Parser<HTMLItem> EmText =
            from star1 in Parse.String("*").Or(Parse.String("_")).Text()
            from content in PlainText(RegularException)
            from star2 in star1 == "*" ? Parse.String("*") : Parse.String("_")
            select new HTMLNode("em", content);

        /// <summary>
        /// Parses strong text (see MarkDown specification).
        /// </summary>
        static readonly Parser<HTMLItem> StrongText =
            from star1 in Parse.String("**").Or(Parse.String("__")).Text()
            from content in PlainText(RegularException)
            from star2 in star1 == "**" ? Parse.String("**") : Parse.String("__")
            select new HTMLNode("strong", content);

        /// <summary>
        /// Parses inline code (see MarkDown specification).
        /// </summary>
        static readonly Parser<HTMLItem> CodeText =
            from star1 in Parse.String("`")
            from content in PlainText(RegularException)
            from star2 in Parse.String("`")
            select new HTMLNode("code", content);

        /// <summary>
        /// Parses an inline image (see MarkDown specification).
        /// </summary>
        static readonly Parser<HTMLItem> MDInlineImage =
            from exclaim in Parse.String("!")
            from lbracket in Parse.String("[")
            from text in Parse.AnyChar.Except(Parse.Char(']').Or(Parse.Char('\r'))).Many().Text()
            from rbracket in Parse.String("]")
            from lparen in Parse.String("(")
            from link in PlainText(Parse.Char(')').Or(Parse.Char('\r')).Or(Parse.Char('"')).Or(Parse.WhiteSpace))
            from rparen in Parse.String(")")
            select new HTMLNode("img", new HTMLAttribute("alt", text), new HTMLAttribute("src", link.ToString()));

        /// <summary>
        /// Parses an inline link, which includes an optional title attribute.
        /// This may not conform 100% to the MarkDown specification. Refer to TestFile.md.
        /// </summary>
        static readonly Parser<HTMLItem> MDInlineLink =
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
            select new HTMLNode("a",
                                GetEnumerable(new HTMLContent(text)),
                                GetEnumerable(new HTMLAttribute("href", link.ToString()),
                                    (title.IsDefined ? new HTMLAttribute("title", title.Get()) : new HTMLAttribute())
                                ));

        /// <summary>
        /// Accepts a series of objects as inputs, and returns them in one IEnumerable object.
        /// </summary>
        /// <typeparam name="T">The type of object.</typeparam>
        /// <param name="objects">The list of objects of type T to be processed.</param>
        /// <returns>Returns an IEnumerable containing the objects.</returns>
        static IEnumerable<T> GetEnumerable<T>(params T[] objects)
        {
            var toReturn = new List<T>();
            foreach (var o in objects) toReturn.Add(o);
            return toReturn as IEnumerable<T>;
        }

        /// <summary>
        /// Parses a single character which cannot be regularly parsed in plain text.
        /// Requires a backslash to precede the character.
        /// </summary>
        static readonly Parser<HTMLItem> ForbiddenCharacter =
            from back in Parse.String("\\")
            from character in RegularException
            select new HTMLContent(character.ToString());

        /// <summary>
        /// Parses any text which may be stylized by markdown, save for a line break.
        /// </summary>
        static readonly Parser<HTMLItem> MajorFormattedText =
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
        static readonly Parser<IEnumerable<HTMLItem>> FormattedText = LineBreak.Or(MajorFormattedText).Many();

        /// <summary>
        /// Parses FormattedText or an empty line into a paragraph node.
        /// </summary>
        static readonly Parser<HTMLItem> Paragraph =
            from items in FormattedText
            select items.Count() != 0 ? new HTMLNode("p", items) : new HTMLNode("p");

        /// <summary>
        /// Parses a header denoted with "=" signs.
        /// </summary>
        static readonly Parser<HTMLItem> Header1 =
            from text in Parse.AnyChar.Except(Parse.Char('\r')).AtLeastOnce().Text()
            from newline in Parse.String("\r\n")
            from lineBegin in Parse.String("==")
            from lineFinish in Parse.String("=").AtLeastOnce()
            select new HTMLNode("h1", new HTMLContent(text));

        /// <summary>
        /// Parses a header denoted with "-" signs.
        /// </summary>
        static readonly Parser<HTMLItem> Header2 =
            from text in Parse.AnyChar.Except(Parse.Char('\r')).AtLeastOnce().Text()
            from newline in Parse.String("\r\n")
            from lineBegin in Parse.String("--")
            from lineFinish in Parse.String("-").AtLeastOnce()
            select new HTMLNode("h2", new HTMLContent(text));

        /// <summary>
        /// Parses any header denoted by using "#" signs.
        /// </summary>
        static readonly Parser<HTMLItem> HashtagHeader =
            from tags in Parse.String("######")
                         .Or(Parse.String("#####"))
                         .Or(Parse.String("####"))
                         .Or(Parse.String("###"))
                         .Or(Parse.String("##"))
                         .Or(Parse.String("#")).Text()
            from text in Parse.AnyChar.Except(Parse.Char('\r')).AtLeastOnce().Text()
            select new HTMLNode("h" + tags.Length.ToString(), new HTMLContent(text));

        /// <summary>
        /// Parses any MarkDown header.
        /// </summary>
        static readonly Parser<HTMLItem> Header = HashtagHeader.Or(Header1).Or(Header2);

        /// <summary>
        /// Parses a "block" of code (see GitHub Flavored Markdown).
        /// </summary>
        static readonly Parser<HTMLNode> CodeBlock =
            from first in Parse.String("```")
            from line in Parse.String("\r\n")
            from text in Parse.AnyChar.Except(Parse.String("```")).Many().Text()
            from last in Parse.String("```")
            select new HTMLNode("code", new HTMLContent(text));

        /// <summary>
        /// Parses a custom list item. Flexible for parsing bulletted and numbered list items, including nested lists.
        /// </summary>
        /// <typeparam name="T">The type of the bullet parser.</typeparam>
        /// <param name="bullet">The parser which parses the bullet of the list.</param>
        /// <param name="nodeName">The name of the node any nested lists will be put into.</param>
        /// <returns>Returns an HTML node representing the list item (or nested list) that was parsed.</returns>
        static Parser<HTMLNode> MDListItem<T>(Parser<T> bullet, string nodeName)
        {
            return
                from ws in Parse.Char(' ').Many()
                from star in bullet
                from space in Parse.Char(' ')
                from text in Parse.AnyChar.Except(Parse.Char('\r')).AtLeastOnce().Text()
                select GetNestedNode(new HTMLContent(text), nodeName, ws.Count());
        }

        /// <summary>
        /// Calculates the nesting of a list.
        /// </summary>
        /// <param name="toNest">The base HTMLItem to be nested.</param>
        /// <param name="nestName">The name of the nodes into which toNest should be nested.</param>
        /// <param name="nestCount">The degree to which toNest should be nested.</param>
        /// <returns>An HTMLNode representing the nested list item.</returns>
        static HTMLNode GetNestedNode(HTMLItem toNest, string nestName, int nestCount)
        {
            if (nestCount == 0) return new HTMLNode("li", toNest);
            else return new HTMLNode(nestName, GetNestedNode(toNest, nestName, nestCount - 1));
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
        static readonly Parser<HTMLNode> MDBulletList =
            MDListItem(ListBullet, "ul").DelimitedBy(Parse.String("\r\n").Once()).Select(i => new HTMLNode("ul", i));

        /// <summary>
        /// Parses a numbered list.
        /// </summary>
        static readonly Parser<HTMLNode> MDNumberList =
            MDListItem(ListDigit, "ol").DelimitedBy(Parse.String("\r\n").Once()).Select(i => new HTMLNode("ol", i));

        /// <summary>
        /// Parses any list.
        /// </summary>
        static readonly Parser<HTMLNode> MDList = MDNumberList.XOr(MDBulletList);

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
        static readonly Parser<HTMLNode> BlockQuote =
            from lines in BlockQuoteLine.DelimitedBy(Parse.String("\r\n").Once())
            select new HTMLNode("blockquote", TermList.Parse(GetConnectedString(lines, "\r\n")));

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

        static readonly Parser<HTMLContent> Content =
            from chars in Parse.CharExcept('<').Many()
            select new HTMLContent(new string(chars.ToArray()));

        static readonly Parser<HTMLNode> FullNode =
            from tag in BeginTag
            from nodes in Parse.Ref(() => Item).Many()
            from end in EndTag(tag)
            select new HTMLNode(tag, nodes);

        static readonly Parser<HTMLNode> ShortNode = Tag(from id in Identifier
                                                     from slash in Parse.Char('/')
                                                     select new HTMLNode(id));

        static readonly Parser<HTMLNode> HtmlNode = ShortNode.Or(FullNode);

        static readonly Parser<HTMLItem> Item = HtmlNode.Select(n => (HTMLItem)n).XOr(Content);

        /// <summary>
        /// Parses a single MarkDown term.
        /// </summary>
        static readonly Parser<HTMLItem> MDTerm = Header.Or(MDList).Or(BlockQuote).Or(CodeBlock).Or(HtmlNode).Or(Paragraph);

        /// <summary>
        /// Parses a list of MarkDown terms, separated by two newlines each.
        /// </summary>
        static readonly Parser<IEnumerable<HTMLItem>> TermList =
            from first in Parse.WhiteSpace.Many().Optional()
            from terms in MDTerm.DelimitedBy(Parse.String("\r\n\r\n"))
            from rest in Parse.WhiteSpace.Many().Optional().End()
            select terms;
    }
}