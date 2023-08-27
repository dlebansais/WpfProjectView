namespace WpfProjectView;

using System.Collections.Generic;

/// <summary>
/// Implements a collection of xaml namespaces.
/// </summary>
internal class XamlNamespaceCollection : List<IXamlNamespace>, IXamlNamespaceCollection
{
    /// <summary>
    /// Initializes a new instance of the <see cref="XamlNamespaceCollection"/> class.
    /// </summary>
    public XamlNamespaceCollection()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="XamlNamespaceCollection"/> class.
    /// </summary>
    /// <param name="collection">The initializing collection.</param>
    public XamlNamespaceCollection(IEnumerable<IXamlNamespace> collection)
        : base(collection)
    {
    }
}
