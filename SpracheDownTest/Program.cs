using System;
using System.IO;
using SpracheDown;

namespace SpracheDownTest
{
    class Program
    {
        static void Main(string[] args)
        {
            string toParse;

            using (var reader = new StreamReader("TestFile.md"))
            {
                toParse = reader.ReadToEnd();
            }

            var Parsed = MarkdownParser.ParseDocument(toParse);

            using (var writer = new StreamWriter("ParsedFile.html"))
            {
                writer.Write(Parsed.ToString());
            }
        }
    }
}
