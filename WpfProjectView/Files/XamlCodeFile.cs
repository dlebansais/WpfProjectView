namespace WpfProjectView;

using System.Diagnostics;
using System.Threading.Tasks;
using FolderView;
using Microsoft.CodeAnalysis;

/// <summary>
/// Represents a Xaml file with code behind.
/// </summary>
/// <param name="XamlSourceFile">The Xaml source file.</param>
/// <param name="CodeSourceFile">The code behind file.</param>
[DebuggerDisplay("{XamlSourceFile.Name,nq}[.cs] (path: {((FolderView.Path)XamlSourceFile.Path).Combined,nq}[.cs])")]
internal record XamlCodeFile(FolderView.IFile XamlSourceFile, FolderView.IFile CodeSourceFile) : File(XamlSourceFile), IXamlCodeFile
{
    /// <summary>
    /// Gets the code behind file.
    /// </summary>
    public IPath CodeBehindPath { get; } = CodeSourceFile.Path;

    /// <summary>
    /// Gets the Xaml file parsing result.
    /// </summary>
    public IXamlParsingResult? XamlParsingResult { get; private set; }

    /// <summary>
    /// Gets the code behind file parsing result.
    /// </summary>
    public SyntaxTree? SyntaxTree { get; private set; }

    /// <inheritdoc/>
    public override async Task LoadAsync(IFolder rootFolder)
    {
        await XamlSourceFile.LoadAsync().ConfigureAwait(false);
        await CodeSourceFile.LoadAsync().ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public override bool IsParsed => XamlParsingResult is not null && SyntaxTree is not null;

    /// <inheritdoc/>
    public override void Parse()
    {
        XamlParsingResult = XamlParser.Parse(XamlSourceFile.Content);
        SyntaxTree = CodeParser.Parse(CodeSourceFile.Content);
    }
}
