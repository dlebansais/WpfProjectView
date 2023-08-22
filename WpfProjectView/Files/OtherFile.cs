namespace WpfProjectView;

using System.IO;
using System.Threading.Tasks;
using FolderView;

/// <summary>
/// Represents a file of unknown type.
/// </summary>
internal record OtherFile(FolderView.IFile SourceFile) : File(SourceFile), IOtherFile
{
    /// <inheritdoc/>
    public Stream? Content { get; private set; }

    /// <inheritdoc/>
    public override async Task LoadAsync(IFolder rootFolder)
    {
        await SourceFile.LoadAsync();
    }

    /// <inheritdoc/>
    public override void Parse()
    {
        Content = SourceFile.Content;
    }
}
