namespace WpfProjectView;

using System.Diagnostics;
using System.Threading.Tasks;
using FolderView;

/// <summary>
/// Represents a Xaml file with no code behind.
/// </summary>
/// <inheritdoc/>
[DebuggerDisplay("{SourceFile.Name,nq} (path: {((FolderView.Path)SourceFile.Path).Combined,nq})")]
internal record XamlResourceFile(FolderView.IFile SourceFile) : File(SourceFile), IXamlResourceFile
{
    /// <inheritdoc cref="IXamlResourceFile.XamlParsingResult" />
    public IXamlParsingResult? XamlParsingResult { get; private set; }

    /// <inheritdoc />
    public override async Task LoadAsync(IFolder rootFolder)
    {
        await SourceFile.LoadAsync().ConfigureAwait(false);
    }

    /// <inheritdoc />
    public override bool IsParsed => XamlParsingResult is not null && XamlParsingResult.Root is not null;

    /// <inheritdoc />
    public override void Parse()
    {
        XamlParsingResult = XamlParser.Parse(SourceFile.Content);
    }
}
