namespace WpfProjectView;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using FolderView;

/// <summary>
/// Represents a project.
/// </summary>
[DebuggerDisplay("{Location,nq}")]
public record Project : IProject
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Project"/> class.
    /// </summary>
    private Project()
    {
    }

    /// <inheritdoc/>
    public required ILocation Location { get; init; }

    /// <inheritdoc/>
    public required IFolder RootFolder { get; init; }

    /// <inheritdoc/>
    public required IReadOnlyList<IFile> Files { get; init; }

    /// <summary>
    /// Returns an instance of <see cref="Project"/> for the provided location.
    /// </summary>
    /// <param name="location">The project location.</param>
    public static async Task<Project> CreateAsync(ILocation location)
    {
        List<IFile> Files = new();

        IFolder RootFolder = await Path.RootFolderFromAsync(location).ConfigureAwait(false);
        FillFileList(RootFolder, Files);

        Project Result = new() { Location = location, RootFolder = RootFolder, Files = Files };

        return Result;
    }

    private static void FillFileList(IFolder folder, List<IFile> files)
    {
        List<FolderView.IFile> XamlFileList = new();
        List<FolderView.IFile> CodeFileList = new();
        List<FolderView.IFile> OtherFileList = new();

        // Split files in 3 categories.
        foreach (var Item in folder.Files)
            if (Item.Path.Name.EndsWith(".xaml", StringComparison.Ordinal))
                XamlFileList.Add(Item);
            else if (Item.Path.Name.EndsWith(".cs", StringComparison.Ordinal))
                CodeFileList.Add(Item);
            else
                OtherFileList.Add(Item);

        // Find xaml files with associated code behind.
        List<(FolderView.IFile DotXaml, FolderView.IFile DotCs)> XamlWithCodeBehindList = new();

        foreach (var XamlItem in XamlFileList)
            if (CodeFileList.Find(item => item.Path.Name == XamlItem.Path.Name + ".cs") is FolderView.IFile CodeItem)
                XamlWithCodeBehindList.Add((XamlItem, CodeItem));

        // Move xaml files with code behind in their own list.
        List<FolderView.IFile> XamlCodeFileList = new();

        foreach (var Item in XamlWithCodeBehindList)
        {
            XamlCodeFileList.Add(Item.DotXaml);

            _ = XamlFileList.Remove(Item.DotXaml);
            _ = CodeFileList.Remove(Item.DotCs);
        }

        // Finally fill the list of files.
        XamlWithCodeBehindList.ForEach(item => FillFileList(new XamlCodeFile(item.DotXaml, item.DotCs), files));
        XamlFileList.ForEach(item => FillFileList(new XamlResourceFile(item), files));
        CodeFileList.ForEach(item => FillFileList(new CodeFile(item), files));
        OtherFileList.ForEach(item => FillFileList(new OtherFile(item), files));

        List<string> IgnoredFolders = new() { "bin", "obj" };
        bool IsRootFolder = folder.IsRoot;

        // Proceed with sub-folders recursively.
        foreach (IFolder Item in folder.Folders)
            if (!IsRootFolder || !IgnoredFolders.Contains(Item.Name))
                FillFileList(Item, files);
    }

    private static void FillFileList<T>(T newFile, List<IFile> files)
        where T : File
    {
        files.Add(newFile);
    }

    /*
    /// <summary>
    /// Links all files in the projects.
    /// </summary>
    public void Link()
    {
    }
    */
}
