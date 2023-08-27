namespace WpfProjectView;

/// <summary>
/// Abstraction of a xaml namespace.
/// </summary>
public interface IXamlNamespace
{
    /// <summary>
    /// Gets the prefix.
    /// </summary>
    string Prefix { get; }

    /// <summary>
    /// Gets the assembly path.
    /// </summary>
    string AssemblyPath { get; }
}
