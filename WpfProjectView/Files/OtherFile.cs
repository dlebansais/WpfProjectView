namespace WpfProjectView;

using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using FolderView;

/// <summary>
/// Represents a file of unknown type.
/// </summary>
/// <inheritdoc/>
[DebuggerDisplay("{SourceFile.Name,nq} (path: {((FolderView.Path)SourceFile.Path).Combined,nq})")]
internal record OtherFile(FolderView.IFile SourceFile) : File(SourceFile), IOtherFile
{
    /// <inheritdoc/>
    public Stream? Content { get; private set; }

    /// <inheritdoc/>
    public override async Task LoadAsync(IFolder rootFolder)
    {
        await SourceFile.LoadAsync().ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public override bool IsParsed { get => Content is not null; }

    /// <inheritdoc/>
    public override void Parse()
    {
        Content = SourceFile.Content;
    }
}
