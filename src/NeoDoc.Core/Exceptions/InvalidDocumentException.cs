namespace NeoDoc.Core.Exceptions;

public sealed class InvalidDocumentException : NeoDocException
{
    public InvalidDocumentException(string message)
        : base(message) { }

    public InvalidDocumentException(string message, Exception innerException)
        : base(message, innerException) { }
}
