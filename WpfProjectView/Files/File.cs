namespace WpfProjectView;

using System.Threading.Tasks;
using FolderView;

/// <summary>
/// Represents a file.
/// </summary>
internal abstract record File(FolderView.IFile SourceFile) : IFile
{
    /// <inheritdoc/>
    public IPath Path => SourceFile.Path;

    /// <inheritdoc/>
    public abstract Task LoadAsync(IFolder rootFolder);

    /// <inheritdoc/>
    public abstract bool IsParsed { get; }

    /// <inheritdoc/>
    public abstract void Parse();
}
