namespace Core.Exceptions;

public sealed class ValidationException : BusinessException
{
    public ValidationException(string message) : base(message, "VALIDATION_ERROR")
    {
    }
}
