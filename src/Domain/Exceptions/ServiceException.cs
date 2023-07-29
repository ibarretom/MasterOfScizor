namespace Domain.Exceptions;

internal class ServiceException : MasterOfScizorException
{
    public ServiceException(string message) : base(message)
    {
    }
}
