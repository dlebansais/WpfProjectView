namespace WpfProjectView;

using System.Threading.Tasks;
using FolderView;

/// <summary>
/// Represents a file.
/// </summary>
/// <param name="SourceFile">The source file.</param>
internal abstract record File(FolderView.IFile SourceFile) : IFile
{
    /// <inheritdoc cref="IFile.Path"/>
    public IPath Path => SourceFile.Path;

    /// <inheritdoc cref="IFile.LoadAsync(IFolder)"/>
    public abstract Task LoadAsync(IFolder rootFolder);

    /// <inheritdoc cref="IFile.IsParsed"/>
    public abstract bool IsParsed { get; }

    /// <inheritdoc cref="IFile.Parse"/>
    public abstract void Parse();
}
