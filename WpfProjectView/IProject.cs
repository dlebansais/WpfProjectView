namespace WpfProjectView;

using System.Collections.Generic;
using System.Threading.Tasks;
using FolderView;

/// <summary>
/// Abstraction of a project.
/// </summary>
public interface IProject
{
    /// <summary>
    /// Gets the project location.
    /// </summary>
    ILocation Location { get; }

    /// <summary>
    /// Gets the project root folder.
    /// </summary>
    IFolder RootFolder { get; }

    /// <summary>
    /// Gets the list of files in the project.
    /// </summary>
    IReadOnlyList<IFile> Files { get; }

    /// <summary>
    /// Gets the list of full path to external DLLs referenced by this project.
    /// </summary>
    IReadOnlyList<string> PathToExternalDlls { get; }

    /// <summary>
    /// Links all files in the projects.
    /// </summary>
    Task LinkAsync();
}
