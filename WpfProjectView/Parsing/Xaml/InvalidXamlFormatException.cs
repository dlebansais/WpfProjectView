namespace WpfProjectView;

using System;
using System.Runtime.Serialization;

/// <summary>
/// Exception class for invalid xaml format.
/// </summary>
[Serializable]
public class InvalidXamlFormatException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="InvalidXamlFormatException"/> class.
    /// </summary>
    public InvalidXamlFormatException()
        : base()
    {
    }

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

    /// <summary>
    /// Initializes a new instance of the <see cref="InvalidXamlFormatException"/> class.
    /// </summary>
    /// <param name="info">The <see cref="SerializationInfo"/> that holds the serialized object data about the exception being thrown.</param>
    /// <param name="context">The <see cref="StreamingContext"/> that contains contextual information about the source or destination.</param>
    protected InvalidXamlFormatException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}
