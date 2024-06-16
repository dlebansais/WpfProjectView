namespace WpfProjectView;

using System.Diagnostics;

/// <summary>
/// Implements a xaml namespace in the local assembly.
/// </summary>
[DebuggerDisplay("{Prefix,nq}={AssemblyPath,nq}")]
internal record XamlNamespaceLocal(string Prefix, string Namespace) : IXamlNamespaceLocal
{
    /// <inheritdoc/>
    public string AssemblyName { get; } = string.Empty;

    /// <summary>
    /// Gets the assembly path.
    /// </summary>
    public string AssemblyPath => $"{XamlNamespace.ClrNamespaceHeader}{Namespace}";
}
