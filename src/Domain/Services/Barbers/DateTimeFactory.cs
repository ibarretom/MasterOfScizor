namespace Domain.Services.Barbers;

internal class DateTimeFactory
{
    public static DateTime BuildWithoutSeconds(DateTime dateTime)
    {
        return new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, dateTime.Minute, 0, dateTime.Kind);
    }
}
