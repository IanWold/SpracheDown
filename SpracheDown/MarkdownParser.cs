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
        public static HTMLDocument ParseDocument(string toParse)
        {
            return new HTMLDocument(
                new HTMLNode("html", new HTMLNode("head"), ParseBody(toParse))
                );
        }

        public static HTMLNode ParseBody(string toParse)
        {
            return new HTMLNode("body", TermList.Parse(toParse));
        }

        static readonly Parser<char> RegularException =
            Parse.Char('\r')
            .Or(Parse.Char('_'))
            .Or(Parse.Char('*'))
            .Or(Parse.Char('['))
            .Or(Parse.Char('`'))
            .Or(Parse.Char('\\'));

        static Parser<HTMLItem> PlainText(Parser<char> except)
        {
            return Parse.AnyChar.Except(except).AtLeastOnce().Text()
                .Select(i => new HTMLContent(i));
        }

        static readonly Parser<HTMLItem> LineBreak =
            from line in Parse.String("\r\n").Except(Parse.String("\r\n\r\n"))
            select new HTMLNode("br");

        static readonly Parser<HTMLItem> EmText =
            from star1 in Parse.String("*").Or(Parse.String("_")).Text()
            from content in PlainText(RegularException)
            from star2 in star1 == "*" ? Parse.String("*") : Parse.String("_")
            select new HTMLNode("em", content);

        static readonly Parser<HTMLItem> StrongText =
            from star1 in Parse.String("**").Or(Parse.String("__")).Text()
            from content in PlainText(RegularException)
            from star2 in star1 == "**" ? Parse.String("**") : Parse.String("__")
            select new HTMLNode("strong", content);

        static readonly Parser<HTMLItem> CodeText =
            from star1 in Parse.String("`")
            from content in PlainText(RegularException)
            from star2 in Parse.String("`")
            select new HTMLNode("code", content);

        static readonly Parser<HTMLItem> MDInlineImage =
            from exclaim in Parse.String("!")
            from lbracket in Parse.String("[")
            from text in Parse.AnyChar.Except(Parse.Char(']').Or(Parse.Char('\r'))).Many().Text()
            from rbracket in Parse.String("]")
            from lparen in Parse.String("(")
            from link in PlainText(Parse.Char(')').Or(Parse.Char('\r')).Or(Parse.Char('"')).Or(Parse.WhiteSpace))
            from rparen in Parse.String(")")
            select new HTMLNode("img", new HTMLAttribute("alt", text), new HTMLAttribute("src", link.ToString()));

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

        static IEnumerable<T> GetEnumerable<T>(params T[] objects)
        {
            var toReturn = new List<T>();
            foreach (var o in objects) toReturn.Add(o);
            return toReturn as IEnumerable<T>;
        }

        static readonly Parser<HTMLItem> ForbiddenCharacter =
            from back in Parse.String("\\")
            from character in Parse.String("*").Text()
                              .Or(Parse.String("_").Text())
                              .Or(Parse.String("`").Text())
                              .Or(Parse.String("\\").Text())
            select new HTMLContent(character);

        static readonly Parser<HTMLItem> MajorFormattedText =
            ForbiddenCharacter
            .Or(StrongText)
            .Or(EmText)
            .Or(CodeText)
            .Or(MDInlineLink)
            .Or(MDInlineImage)
            .Or(PlainText(RegularException));

        static readonly Parser<IEnumerable<HTMLItem>> FormattedText = LineBreak.Or(MajorFormattedText).Many();

        static readonly Parser<HTMLItem> Paragraph =
            from items in FormattedText
            select items.Count() != 0 ? new HTMLNode("p", items) : new HTMLNode("p");

        static readonly Parser<HTMLItem> Header1 =
            from text in Parse.AnyChar.Except(Parse.Char('\r')).AtLeastOnce().Text()
            from newline in Parse.String("\r\n")
            from lineBegin in Parse.String("==")
            from lineFinish in Parse.String("=").AtLeastOnce()
            select new HTMLNode("h1", new HTMLContent(text));

        static readonly Parser<HTMLItem> Header2 =
            from text in Parse.AnyChar.Except(Parse.Char('\r')).AtLeastOnce().Text()
            from newline in Parse.String("\r\n")
            from lineBegin in Parse.String("--")
            from lineFinish in Parse.String("-").AtLeastOnce()
            select new HTMLNode("h2", new HTMLContent(text));

        static readonly Parser<HTMLItem> HashtagHeader =
            from tags in Parse.String("######")
                         .Or(Parse.String("#####"))
                         .Or(Parse.String("####"))
                         .Or(Parse.String("###"))
                         .Or(Parse.String("##"))
                         .Or(Parse.String("#")).Text()
            from text in Parse.AnyChar.Except(Parse.Char('\r')).AtLeastOnce().Text()
            select new HTMLNode("h" + tags.Length.ToString(), new HTMLContent(text));

        static readonly Parser<HTMLItem> Header = HashtagHeader.Or(Header1).Or(Header2);

        static readonly Parser<HTMLNode> CodeBlock =
            from first in Parse.String("```")
            from line in Parse.String("\r\n")
            from text in Parse.AnyChar.Except(Parse.String("```")).Many().Text()
            from last in Parse.String("```")
            select new HTMLNode("code", new HTMLContent(text));

        static Parser<HTMLNode> MDListItem<T>(Parser<T> bullet, string nodeName)
        {
            return
                from ws in Parse.Char(' ').Many()
                from star in bullet
                from space in Parse.Char(' ')
                from text in Parse.AnyChar.Except(Parse.Char('\r')).AtLeastOnce().Text()
                select GetNestedNode(new HTMLContent(text), nodeName, ws.Count());
        }

        static HTMLNode GetNestedNode(HTMLItem toNest, string nestName, int nestCount)
        {
            if (nestCount == 0) return new HTMLNode("li", toNest);
            else return new HTMLNode(nestName, GetNestedNode(toNest, nestName, nestCount - 1));
        }

        static readonly Parser<char> ListBullet =
            Parse.Char('*')
            .Or(Parse.Char('+'))
            .Or(Parse.Char('-'));

        static readonly Parser<IEnumerable<char>> ListDigit =
            from num in Parse.Digit.AtLeastOnce()
            from dot in Parse.String(".")
            select num;

        static readonly Parser<HTMLNode> MDBulletList =
            MDListItem(ListBullet, "ul").DelimitedBy(Parse.String("\r\n").Once()).Select(i => new HTMLNode("ul", i));

        static readonly Parser<HTMLNode> MDNumberList =
            MDListItem(ListDigit, "ol").DelimitedBy(Parse.String("\r\n").Once()).Select(i => new HTMLNode("ol", i));

        static readonly Parser<HTMLNode> MDList = MDNumberList.XOr(MDBulletList);

        static readonly Parser<string> BlockQuoteLine =
            from first in Parse.String(">").Once()
            from ws in Parse.String(" ").Optional()
            from text in Parse.AnyChar.Except(Parse.Char('\r')).Many().Text()
            select text;

        static readonly Parser<HTMLNode> BlockQuote =
            from lines in BlockQuoteLine.DelimitedBy(Parse.String("\r\n").Once())
            select new HTMLNode("blockquote", TermList.Parse(GetConnectedString(lines, "\r\n")));

        static string GetConnectedString(IEnumerable<string> strings, string addition = "")
        {
            var toReturn = "";
            foreach (var s in strings) toReturn += s.ToString() + addition;
            return toReturn;
        }

        static readonly Parser<HTMLItem> MDTerm = Header.Or(MDList).Or(BlockQuote).Or(CodeBlock).Or(Paragraph);

        static readonly Parser<IEnumerable<HTMLItem>> TermList = MDTerm.DelimitedBy(Parse.String("\r\n\r\n")).End();
    }
}