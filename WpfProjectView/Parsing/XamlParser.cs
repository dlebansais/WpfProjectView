namespace WpfProjectView;

using System;
using System.IO;
using System.Text;
using System.Xaml;

/// <summary>
/// Implements a xaml parser.
/// </summary>
public static partial class XamlParser
{
    /// <summary>
    /// Parses xaml content and returns the node tree.
    /// </summary>
    /// <param name="content">The content to parse.</param>
    public static IXamlParsingResult Parse(byte[]? content)
    {
        XamlParsingResult Result = new();

        if (content is not null)
        {
            using MemoryStream Stream = new(content);
            using XamlXmlReader Reader = new(Stream, new XamlXmlReaderSettings() { ProvideLineInfo = true });

            if (!Reader.Read())
                throw new NotImplementedException();

            XamlParsingContext Context = new(Reader, new XamlNamespaceCollection());
            Result.Root = ParseElement(Context);
        }

        return Result;
    }

    /// <summary>
    /// Prints a diagnostic of a parsing result.
    /// </summary>
    /// <param name="parsingResult">The parsing result.</param>
    public static StringBuilder Print(IXamlParsingResult parsingResult)
    {
        StringBuilder Builder = new();

        if (parsingResult.Root is not null)
        {
            XamlPrintingContext Context = new(Builder, parsingResult.Root, Indentation: 0);
            Print(Context);
        }

        return Builder;
    }
}
