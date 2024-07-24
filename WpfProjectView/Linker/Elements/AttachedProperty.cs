namespace WpfProjectView;

using System;

/// <summary>
/// Represents a property of an element.
/// </summary>
/// <param name="Owner">The element to which the property belongs.</param>
/// <param name="DeclaringType">The type that declared the property.</param>
public record AttachedProperty(Element Owner, Type DeclaringType)
{
}
