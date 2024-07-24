namespace WpfProjectView;

using System;
using Microsoft.CodeAnalysis;

/// <summary>
/// Represents a directive for a XAML element.
/// </summary>
/// <param name="Name">The directive name.</param>
public record Directive(string Name)
{
}
