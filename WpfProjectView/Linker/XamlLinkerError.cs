namespace WpfProjectView;

/// <summary>
/// Represents an error reported by <see cref="XamlLinker"/>.
/// </summary>
/// <param name="Message">The error message.</param>
public record XamlLinkerError(string Message)
{
}
