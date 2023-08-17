namespace WpfProjectView;

using System.Text;
using System.Threading.Tasks;
using FolderView;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

/// <summary>
/// Represents a C# code file.
/// </summary>
internal record CodeFile(FolderView.IFile SourceFile) : File(SourceFile), ICodeFile
{
    /// <inheritdoc/>
    public SyntaxTree? SyntaxTree { get; private set; }

    /// <inheritdoc/>
    public override async Task LoadAsync(IFolder rootFolder)
    {
        await SourceFile.LoadAsync();
        SyntaxTree = LoadCodeSyntaxTreeAsync(SourceFile.Content);
    }

    /// <summary>
    /// Parses a C# code file and returns the syntax tree.
    /// </summary>
    /// <param name="content">The content to parse.</param>
    public static SyntaxTree? LoadCodeSyntaxTreeAsync(byte[]? content)
    {
        if (content is null)
            return null;

        string SourceCode = Encoding.UTF8.GetString(content);
        CSharpParseOptions Options = new CSharpParseOptions(LanguageVersion.CSharp11, DocumentationMode.None);

        SyntaxTree Result = CSharpSyntaxTree.ParseText(SourceCode, Options);

        return Result;
    }
}
