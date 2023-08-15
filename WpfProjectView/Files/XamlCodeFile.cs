namespace WpfProjectView;

using FolderView;

/// <summary>
/// Represents a Xaml file with code behind.
/// </summary>
internal record XamlCodeFile(IPath Path) : File(Path), IXamlCodeFile
{
    /// <summary>
    /// Gets the path to the code behind file.
    /// </summary>
    public IPath CodeBehindPath { get; } = new Path(Path.Ancestors, Path.Name + ".cs");
}
