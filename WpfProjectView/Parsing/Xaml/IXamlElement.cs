namespace WpfProjectView;

/// <summary>
/// Abstraction of a xaml element.
/// </summary>
public interface IXamlElement
{
    /// <summary>
    /// Gets the element name.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the collection of namespaces available in the scope of this element.
    /// </summary>
    IXamlNamespaceCollection Namespaces { get; }

    /// <summary>
    /// Gets the list of children elements.
    /// </summary>
    IXamlElementCollection Children { get; }

    /// <summary>
    /// Gets the list of attributes.
    /// </summary>
    IXamlAttributeCollection Attributes { get; }
}
