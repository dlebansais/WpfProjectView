namespace WpfProjectView;

/// <summary>
/// Implements a xaml namespace.
/// </summary>
/// <inheritdoc/>
internal record XamlNamespace(string Prefix, string AssemblyPath) : IXamlNamespace
{
}
