namespace YA.UserWorker.Application.Exceptions;

public class CommandExecutionException : Exception
{
    public CommandExecutionException()
    {
    }

    public CommandExecutionException(string message) : base(message)
    {
    }

    public CommandExecutionException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
