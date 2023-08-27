namespace WpfProjectView;

using System.Diagnostics;

/// <summary>
/// Implements a xaml element attribute.
/// </summary>
[DebuggerDisplay("{Name,nq}")]
internal record XamlAttributeElementCollection(string Name, XamlElementCollection Children, bool IsVisible, bool IsOneLine) : IXamlAttribute
{
    /// <inheritdoc/>
    public object? Value { get => Children; }
}
