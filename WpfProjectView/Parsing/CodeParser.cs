namespace WpfProjectView;

using System.IO;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

/// <summary>
/// Implements a C# code parser.
/// </summary>
internal static class CodeParser
{
    /// <summary>
    /// Parses C# code content and returns the syntax tree.
    /// </summary>
    /// <param name="content">The content to parse.</param>
    public static SyntaxTree? Parse(Stream? content)
    {
        if (content is null)
            return null;

        using StreamReader Reader = new(content, Encoding.UTF8);
        string SourceCode = Reader.ReadToEnd();
        CSharpParseOptions Options = new CSharpParseOptions(LanguageVersion.CSharp11, DocumentationMode.None);

        SyntaxTree Result = CSharpSyntaxTree.ParseText(SourceCode, Options);

        return Result;
    }
}
