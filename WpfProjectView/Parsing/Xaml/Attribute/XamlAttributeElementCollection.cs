namespace WpfProjectView;

using System.Diagnostics;

/// <summary>
/// Implements a xaml element attribute.
/// </summary>
[DebuggerDisplay("{Name,nq}")]
internal record XamlAttributeElementCollection(string Name, IXamlElementCollection Children, bool IsVisible, bool IsOneLine) : IXamlAttributeElementCollection
{
    /// <inheritdoc/>
    public object? Value { get => Children; }
}
