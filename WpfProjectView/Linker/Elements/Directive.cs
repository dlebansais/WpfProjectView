namespace WpfProjectView;

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Forms;

/// <summary>
/// Represents a directive for a XAML element.
/// </summary>
/// <param name="Name">The directive name.</param>
public record Directive(string Name)
{
    /// <summary>
    /// The Class directive.
    /// </summary>
    private static readonly Directive ClassDirective = new("Class");

    /// <summary>
    /// The ClassModifier directive.
    /// </summary>
    private static readonly Directive ClassModifierDirective = new("ClassModifier");

    /// <summary>
    /// The DeferLoadStrategy directive.
    /// </summary>
    private static readonly Directive DeferLoadStrategyDirective = new("DeferLoadStrategy");

    /// <summary>
    /// The FieldModifier directive.
    /// </summary>
    private static readonly Directive FieldModifierDirective = new("FieldModifier");

    /// <summary>
    /// The Name directive.
    /// </summary>
    private static readonly Directive NameDirective = new("Name");

    /// <summary>
    /// The Key directive.
    /// </summary>
    private static readonly Directive KeyDirective = new("Key");

    /// <summary>
    /// The Subclass directive.
    /// </summary>
    private static readonly Directive SubclassDirective = new("Subclass");

    /// <summary>
    /// The Uid directive.
    /// </summary>
    private static readonly Directive UidDirective = new("Uid");

    /// <summary>
    /// The list of supported directives.
    /// </summary>
    private static readonly List<Directive> SupportedDirectives = new()
    {
        ClassDirective,
        ClassModifierDirective,
        DeferLoadStrategyDirective,
        FieldModifierDirective,
        NameDirective,
        KeyDirective,
        SubclassDirective,
        UidDirective,
    };

    /// <summary>
    /// Tries to parse a name into a directive.
    /// </summary>
    /// <param name="name">The string to parse.</param>
    /// <param name="directive">The directive upon return, if found.</param>
    public static bool TryParse(string name, out Directive directive)
    {
        if (SupportedDirectives.Find(x => x.Name == name) is Directive Found)
        {
            directive = Found;
            return true;
        }

        directive = null!;
        return false;
    }
}
