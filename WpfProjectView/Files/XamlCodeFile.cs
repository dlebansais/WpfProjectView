namespace WpfProjectView;

using System.Threading.Tasks;
using FolderView;
using Microsoft.CodeAnalysis;

/// <summary>
/// Represents a Xaml file with code behind.
/// </summary>
internal record XamlCodeFile(FolderView.IFile XamlSourceFile, FolderView.IFile CodeSourceFile) : File(XamlSourceFile), IXamlCodeFile
{
    /// <inheritdoc/>
    public IPath CodeBehindPath { get; } = CodeSourceFile.Path;

    /// <inheritdoc/>
    public IXamlParsingResult? XamlParsingResult { get; private set; }

    /// <inheritdoc/>
    public SyntaxTree? SyntaxTree { get; private set; }

    /// <inheritdoc/>
    public override async Task LoadAsync(IFolder rootFolder)
    {
        await XamlSourceFile.LoadAsync();
        await CodeSourceFile.LoadAsync();
    }

    /// <inheritdoc/>
    public override void Parse()
    {
        XamlParsingResult = XamlParser.Parse(XamlSourceFile.Content);
        SyntaxTree = CodeParser.Parse(CodeSourceFile?.Content);
    }
}
