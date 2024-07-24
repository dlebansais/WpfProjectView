namespace WpfProjectView;

/// <summary>
/// Represents a event of an element.
/// </summary>
/// <param name="NamedEvent">The event object.</param>
/// <param name="Owner">The element with this event.</param>
/// <param name="IsAttached">True if the event is an attached event.</param>
/// <param name="Handler">The event handler.</param>
public record Event(NamedEvent NamedEvent, Element Owner, bool IsAttached, object Handler)
{
}
