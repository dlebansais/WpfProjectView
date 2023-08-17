namespace WpfProjectView;

using System.Threading.Tasks;
using FolderView;
using Microsoft.CodeAnalysis;

/// <summary>
/// Represents a Xaml file with code behind.
/// </summary>
internal record XamlCodeFile(FolderView.IFile SourceFile) : File(SourceFile), IXamlCodeFile
{
    /// <inheritdoc/>
    public IPath CodeBehindPath { get; } = new Path(SourceFile.Path.Ancestors, SourceFile.Path.Name + ".cs");

    /// <inheritdoc/>
    public byte[]? Content { get; private set; }

    /// <inheritdoc/>
    public SyntaxTree? SyntaxTree { get; private set; }

    /// <inheritdoc/>
    public override async Task LoadAsync(IFolder rootFolder)
    {
        await SourceFile.LoadAsync();
        Content = SourceFile.Content;

        FolderView.IFile CodeSourceFile = FolderView.Path.GetRelativeFile(rootFolder, CodeBehindPath);
        await CodeSourceFile.LoadAsync();
        SyntaxTree = CodeFile.LoadCodeSyntaxTreeAsync(CodeSourceFile.Content);
    }
}
