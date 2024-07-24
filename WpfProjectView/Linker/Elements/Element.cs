namespace WpfProjectView;

using System.Collections.Generic;

/// <summary>
/// Represents an element.
/// </summary>
/// <param name="NamedType">The element type.</param>
/// <param name="Parent">The element parent.</param>
/// <param name="Directives">Directives applying to the element.</param>
/// <param name="Properties">The element properties.</param>
/// <param name="Events">The element events.</param>
/// <param name="Children">The element children.</param>
public record Element(NamedType NamedType, Element? Parent, IReadOnlyList<Directive> Directives, IReadOnlyList<Property> Properties, IReadOnlyList<Event> Events, IReadOnlyList<Element> Children)
{
}
