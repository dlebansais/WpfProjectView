namespace WpfProjectView;

using System;

/// <summary>
/// Exception class for invalid xaml format.
/// </summary>
public class InvalidXamlFormatException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="InvalidXamlFormatException"/> class.
    /// </summary>
    /// <param name="message">The message.</param>
    public InvalidXamlFormatException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="InvalidXamlFormatException"/> class.
    /// </summary>
    /// <param name="message">The message.</param>
    /// <param name="innerException">The inner exception.</param>
    public InvalidXamlFormatException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
