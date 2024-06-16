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
    /// Gets the namespace.
    /// </summary>
    string Namespace { get; }

    /// <summary>
    /// Gets the assembly name.
    /// </summary>
    string AssemblyName { get; }

    /// <summary>
    /// Gets the assembly path.
    /// </summary>
    string AssemblyPath { get; }
}
