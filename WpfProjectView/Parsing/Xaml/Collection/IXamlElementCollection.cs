namespace WpfProjectView;

using System.Collections.Generic;

/// <summary>
/// Abstraction of a collection of xaml elements.
/// </summary>
public interface IXamlElementCollection : IReadOnlyList<IXamlElement>
{
}
