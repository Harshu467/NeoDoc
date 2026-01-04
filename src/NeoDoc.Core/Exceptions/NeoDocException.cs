namespace NeoDoc.Core.Exceptions;

public class NeoDocException : Exception
{
    public NeoDocException(string message)
        : base(message) { }

    public NeoDocException(string message, Exception innerException)
        : base(message, innerException) { }
}
