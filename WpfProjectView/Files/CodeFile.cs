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
    }

    /// <inheritdoc/>
    public override void Parse()
    {
        SyntaxTree = CodeParser.Parse(SourceFile.Content);
    }
}
