namespace WpfProjectView;

using System.Collections.Generic;
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
    /// Gets the list of files in the project.
    /// </summary>
    IReadOnlyList<IFile> Files { get; }
}
