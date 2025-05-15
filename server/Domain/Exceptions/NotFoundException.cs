namespace Core.Exceptions;

public sealed class NotFoundException : BusinessException
{
    public NotFoundException(string message) : base(message, "NOT_FOUND")
    {
    }
}
