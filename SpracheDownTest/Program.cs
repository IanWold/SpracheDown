using System;
using System.IO;
using SpracheDown;

namespace SpracheDownTest
{
    class Program
    {
        static void Main(string[] args)
        {
            var reader = new StreamReader("TestFile.md");
            var toParse = reader.ReadToEnd();
            reader.Close();

            var Parsed = MarkdownParser.ParseDocument(toParse);

            var writer = new StreamWriter("ParsedFile.html");
            writer.Write(Parsed.ToString());
            writer.Close();

            Console.WriteLine("Parse successful!");

            Console.ReadLine();
        }
    }
}
