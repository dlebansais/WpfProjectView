namespace WpfProjectView;

/// <summary>
/// Abstraction of a xaml element attribute.
/// </summary>
public interface IXamlAttribute
{
    /// <summary>
    /// Gets the attribute name.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the attribute value.
    /// </summary>
    object? Value { get; }
}
