namespace WpfProjectView;

/// <summary>
/// Abstraction of a xaml element attribute.
/// </summary>
public interface IXamlAttributeMember : IXamlAttribute
{
    /// <summary>
    /// Gets the attribute name.
    /// </summary>
    string Name { get; }
}
