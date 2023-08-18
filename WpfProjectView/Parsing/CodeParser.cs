namespace WpfProjectView;

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
    public static SyntaxTree? Parse(byte[]? content)
    {
        if (content is null)
            return null;

        string SourceCode = Encoding.UTF8.GetString(content);
        CSharpParseOptions Options = new CSharpParseOptions(LanguageVersion.CSharp11, DocumentationMode.None);

        SyntaxTree Result = CSharpSyntaxTree.ParseText(SourceCode, Options);

        return Result;
    }
}
