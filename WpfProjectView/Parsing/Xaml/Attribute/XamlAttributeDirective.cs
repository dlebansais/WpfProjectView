namespace WpfProjectView;

using System.Diagnostics;

/// <summary>
/// Implements a xaml element attribute.
/// </summary>
[DebuggerDisplay("{Namespace.Prefix,nq}:{Name,nq}")]
internal record XamlAttributeDirective(IXamlNamespace Namespace, string Name, object? Value) : IXamlAttribute
{
}
