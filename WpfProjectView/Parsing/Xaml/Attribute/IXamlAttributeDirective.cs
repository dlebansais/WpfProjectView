namespace WpfProjectView;

/// <summary>
/// Abstraction of a xaml element attribute.
/// </summary>
public interface IXamlAttributeDirective : IXamlAttribute
{
    /// <summary>
    /// Gets the namespace.
    /// </summary>
    IXamlNamespace Namespace { get; }
}
