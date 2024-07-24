namespace WpfProjectView;

/// <summary>
/// Represents a property of an element.
/// </summary>
/// <param name="NamedProperty">The property object.</param>
/// <param name="Owner">The element with this property.</param>
/// <param name="Value">The property value.</param>
public record Property(INamedProperty NamedProperty, Element Owner, object Value)
{
}
