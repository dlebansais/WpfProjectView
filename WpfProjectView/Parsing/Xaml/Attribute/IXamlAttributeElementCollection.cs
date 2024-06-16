namespace WpfProjectView;

/// <summary>
/// Abstraction of a xaml element attribute with children.
/// </summary>
public interface IXamlAttributeElementCollection : IXamlAttribute
{
    /// <summary>
    /// Gets the attribute name.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the collection of children elements.
    /// </summary>
    IXamlElementCollection Children { get; }
}
