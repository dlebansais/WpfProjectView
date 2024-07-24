namespace WpfProjectView;

using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using FolderView;
using Microsoft.CodeAnalysis;

/// <summary>
/// Represents a C# code file.
/// </summary>
/// <inheritdoc/>
[DebuggerDisplay("{SourceFile.Name,nq} (path: {((FolderView.Path)SourceFile.Path).Combined,nq})")]
internal record CodeFile(FolderView.IFile SourceFile) : File(SourceFile), ICodeFile
{
    /// <inheritdoc/>
    public SyntaxTree? SyntaxTree { get; private set; }

    /// <inheritdoc/>
    public override async Task LoadAsync(IFolder rootFolder)
    {
        await SourceFile.LoadAsync().ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public override bool IsParsed { get => SyntaxTree is not null; }

    /// <inheritdoc/>
    public override void Parse()
    {
        SyntaxTree = CodeParser.Parse(SourceFile.Content);
    }
}
