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
    public IXamlElement? RootElement { get; private set; }

    /// <inheritdoc/>
    public SyntaxTree? SyntaxTree { get; private set; }

    /// <inheritdoc/>
    public override async Task LoadAsync(IFolder rootFolder)
    {
        await SourceFile.LoadAsync();
        await CodeSourceFile.LoadAsync();
    }

    /// <inheritdoc/>
    public override void Parse()
    {
        RootElement = XamlParser.Parse(SourceFile.Content);
        SyntaxTree = CodeParser.Parse(CodeSourceFile?.Content);
    }
}
