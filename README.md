# SpracheDown [![Build Status](https://travis-ci.org/IanWold/SpracheDown.svg?branch=master)](https://travis-ci.org/IanWold/SpracheDown)

SpracheDown is a MarkDown parser written in [Sprache](https://github.com/sprache/Sprache) for C#.

About a year ago at the [Iowa Code Camp](http://www.iowacodecamp.com), someone asked if it was possible to make a MarkDown parser in Sprache, so this is to show it can be done. It's only a proof of concept, so a few features aren't implemented (i.e. reference-style links and inline HTML).

Even so, it's pretty cool. If you want to help make this more substantial, pull requests are always welcome!

## Future

**tl;dr: SpracheDown is alive! I intend to add more to this project so as to keep it lively and relevant.**

As of November 2018 it is my intention to extend this project in two ways: First, I want to reconfigure the AST slightly to strip it down a bit more to make it more approachable and easier to extend. Second, I want to reconfigure the parser to implement the [GitHub Flavored Markdown Specification](https://github.github.com/gfm/).

The reason for all this is to refresh this project and keep it as a good example of a Markdown parser written with Sprache. I have already updated the project to use .NET Standard 2.0, and I've created [Release 1.0](https://github.com/IanWold/SpracheDown/releases/tag/v1.0) to keep the original project archived if you rely on it. I'm particularly eager to update the parser to implement the GitHub specification. Though this may make the parser larger, it will make it easier to understand its structure, and it provides an objective reference for bugs.

## Analysis

It was, overall, incredibly easy to throw this parser together. It took only a combined 6 hours over two days to throw it together from the ground up. The resulting quality of the code also seems relatively high for a proof of concept.

For a little over a year now, I've been using [NMarked](https://github.com/bojanrajkovic/nmarked), which is another MarkDown parser written in C#, but NMarked is more a translation of the original MarkDown parser. NMarked's size is much greater than that of SpracheDown, granted that SpracheDown is missing some important features. That said, I think that a full-fledged MarkDown compiler written with Sprache could be substantially smaller than the vanilla MarkDown parser.

To compare SpracheDown with other MarkDown parsers written with monadic parser combinators, a guy named Gred Hendershott has an interesting [article](http://www.greghendershott.com/2013/11/markdown-parser-redesign.html) talking about making [this parser](https://github.com/greghendershott/markdown) with a parser combinator deal written in Racket. The fact that his parser is, all together, more than three times the size of mine (not counting for comments), I think either I'm doing something terribly wrong, or C# is the most efficient language ever designed. Obviously, the latter would be much harder to prove...

I've a full description of the project on my [blog](http://ianwold.silvrback.com), if you're interested.

## How To

You can pass in your MarkDown text, and you'll get an HTMLDocument object in return:

```C#
HTMLDocument Parsed = MarkdownParser.ParseDocument(toParse);
```

Then you can write the HTMLDocument as a string, and voi-la, perfectly formatted HTML:

```C#
Console.Write(Parsed.ToString());
```

Not every MarkDown feature is implemented, but most are. The file TestFile.md in SpracheDownTest has examples of most anything you might want to mark down.

## Pull Requests

Of course, any comments and/or pull requests are always welcome. If you add any features, please be sure to throw a line into TestFile.md so you can show it off!
