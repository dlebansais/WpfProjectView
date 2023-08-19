namespace WpfProjectView;

using System.Threading.Tasks;
using FolderView;

/// <summary>
/// Represents a Xaml file with no code behind.
/// </summary>
internal record XamlResourceFile(FolderView.IFile SourceFile) : File(SourceFile), IXamlResourceFile
{
    /// <inheritdoc/>
    public IXamlElement? RootElement { get; private set; }

    /// <inheritdoc/>
    public override async Task LoadAsync(IFolder rootFolder)
    {
        await SourceFile.LoadAsync();
    }

    /// <inheritdoc/>
    public override void Parse()
    {
        RootElement = XamlParser.Parse(SourceFile.Content);
    }
}
