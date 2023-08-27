namespace WpfProjectView;

using System.Diagnostics;

/// <summary>
/// Implements a xaml element attribute.
/// </summary>
[DebuggerDisplay("{Name,nq}")]
internal record XamlAttributeMember(string Name, object? Value) : IXamlAttribute
{
}
