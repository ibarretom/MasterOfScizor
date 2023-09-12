namespace Domain.Exceptions;

internal class OrderException : MasterOfScizorException
{
    public OrderException(string message) : base(message)
    {
    }
}
