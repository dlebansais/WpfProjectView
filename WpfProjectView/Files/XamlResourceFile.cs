﻿namespace WpfProjectView;

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
    /// <inheritdoc/>
    public IXamlParsingResult? XamlParsingResult { get; private set; }

    /// <inheritdoc/>
    public override async Task LoadAsync(IFolder rootFolder)
    {
        await SourceFile.LoadAsync();
    }

    /// <inheritdoc/>
    public override void Parse()
    {
        XamlParsingResult = XamlParser.Parse(SourceFile.Content);
    }
}
