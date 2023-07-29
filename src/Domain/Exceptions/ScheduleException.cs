namespace Domain.Exceptions;

internal class ScheduleException : MasterOfScizorException
{
    public ScheduleException(string message) : base(message)
    {
    }
}
