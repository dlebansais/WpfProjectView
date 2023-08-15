namespace WpfProjectView;

using FolderView;

/// <summary>
/// Represents a Xaml file with no code behind.
/// </summary>
internal record XamlResourceFile(IPath Path) : File(Path)
{
}
