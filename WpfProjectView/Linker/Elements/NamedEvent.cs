namespace WpfProjectView;

using System;
using System.Reflection;
using Microsoft.CodeAnalysis;

/// <summary>
/// Represents an event of a type.
/// </summary>
/// <param name="Name">The event name.</param>
/// <param name="FromEventInfo">The <see cref="EventInfo"/> object if coming from a <see cref="Type.GetEvent(string)"/> call.</param>
/// <param name="FromEventSymbol">The <see cref="IEventSymbol"/> object if coming from code parsing.</param>
public record NamedEvent(string Name, EventInfo? FromEventInfo, IEventSymbol? FromEventSymbol)
{
}
