namespace WpfProjectView;

using System.Diagnostics;

/// <summary>
/// Implements a xaml element attribute.
/// </summary>
[DebuggerDisplay("{StringValue,nq}")]
internal record XamlAttributeSimpleValue(string StringValue) : IXamlAttribute
{
    /// <inheritdoc/>
    public object? Value { get => StringValue; }
}
