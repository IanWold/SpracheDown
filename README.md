SpracheDown
===========

SpracheDown is a MarkDown parser written in [Sprache](https://github.com/sprache/Sprache) for C#.

About a year ago at the [Iowa Code Camp](http://www.iowacodecamp.com), someone asked if it was possible to make a MarkDown parser in Sprache, so this is to show it can be done. It's only a proof of concept, so a few features aren't implemented (i.e. reference-style links and inline HTML).

Even so, it's pretty cool. If you want to help make this more substantial, pull requests are always welcome!

How To
======

You can pass in your MarkDown text, and you'll get an HTMLDocument object in return:

```C#
HTMLDocument Parsed = MarkdownParser.ParseDocument(toParse);
```

Then you can write the HTMLDocument as a string, and voi-la, perfectly formatted HTML:

```C#
Console.Write(Parsed.ToString());
```

Not every MarkDown feature is implemented, but most are. The file TestFile.md in SpracheDownTest has examples of most anything you might want to mark down.

Pull Requests
=============

Of course, any comments and/or pull requests are always welcome. If you add any features, please be sure to throw a line into TestFile.md so you can show it off!
