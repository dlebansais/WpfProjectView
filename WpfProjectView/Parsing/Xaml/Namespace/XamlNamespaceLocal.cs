namespace WpfProjectView;

/// <summary>
/// Implements a xaml namespace in the local assembly.
/// </summary>
internal record XamlNamespaceLocal(string Prefix, string Namespace) : IXamlNamespace
{
    /// <summary>
    /// Gets the assembly path.
    /// </summary>
    public string AssemblyPath => $"{XamlNamespace.ClrNamespaceHeader}{Namespace}";
}
