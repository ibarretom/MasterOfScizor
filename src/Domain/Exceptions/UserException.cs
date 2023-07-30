namespace Domain.Exceptions;

internal class UserException : MasterOfScizorException
{
    public UserException(string message) : base(message)
    {
    }
}
