namespace Domain.Exceptions;

internal class AuthenticationException : MasterOfScizorException
{
    public AuthenticationException(string message) : base(message)
    {
    }
}
