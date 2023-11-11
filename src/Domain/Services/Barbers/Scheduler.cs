using Domain.Entities;
using Domain.Entities.Barbers;
using Domain.Entities.Orders;
using Domain.Exceptions;
using Domain.Exceptions.Messages;

namespace Domain.Services.Barbers;

internal class Scheduler : IScheduler
{
    public HashSet<DateTime> GetAvailable(DateTime day, OrderBase order, List<Order> orders)
    {
        var schedules = order.Branch.GetScheduleFor(day.DayOfWeek) ?? throw new CompanyException(CompanyExceptionMessagesResource.BRANCH_IS_NOT_OPENNED_THIS_DAY);

        HashSet<DateTime> allPossibleTimes = GenerateTimes(day, schedules, order.Branch.Configuration);

        if (allPossibleTimes.FirstOrDefault() == default)
            return new HashSet<DateTime>();

        var lunchIntervalForThisDay = order.Worker.LunchInterval.Where(lunch => schedules.Any(schedule => schedule.Includes(lunch.WeekDay))).ToHashSet();

        allPossibleTimes = RemoveLunchInterval(day, allPossibleTimes, lunchIntervalForThisDay, order.Branch.Configuration).ToHashSet();

        return RemoveDateWithOrdersConflict(day, orders, allPossibleTimes, order.Branch.Configuration).ToHashSet();
    }

    private static HashSet<DateTime> GenerateTimes(DateTime day, HashSet<Schedule> schedule, Configuration configuration)
    {
        return MergeTimes(day, schedule, configuration);
    }

    private static HashSet<DateTime> MergeTimes(DateTime day, HashSet<Schedule> schedules, Configuration configuration)
    {
        var possiblesTimes = new HashSet<DateTime>();

        var scheduleTimesQueue = new Queue<DateTime>();

        foreach (var schedule in schedules)
        {
            var (StartTime, EndTime) = Schedule.GetScheduleDateTime(schedule, schedule.GetDateToBeReference(day));

            scheduleTimesQueue.Enqueue(StartTime);
            scheduleTimesQueue.Enqueue(EndTime);
        }

        if (!scheduleTimesQueue.TryDequeue(out var startTime))
            return possiblesTimes;

        if (!scheduleTimesQueue.TryDequeue(out var endTime))
            return possiblesTimes;

        if (IsValidTime(startTime, configuration.ScheduleDelayTime, day))
            possiblesTimes.Add(startTime);

        do
        {
            var nextTime = startTime.AddMinutes(configuration.ScheduleDefaultInterval.TotalMinutes);

            var endTimeHasBeenReached = nextTime >= endTime;
            if (!endTimeHasBeenReached)
            {
                if (IsValidTime(nextTime, configuration.ScheduleDelayTime, day))
                    possiblesTimes.Add(nextTime);

                startTime = nextTime;
                continue;
            }

            if (!scheduleTimesQueue.TryDequeue(out startTime))
                break;

            if (!scheduleTimesQueue.TryDequeue(out endTime))
                break;

            if (IsValidTime(startTime, configuration.ScheduleDelayTime, day))
                possiblesTimes.Add(startTime);

        } while (startTime != default);

        return possiblesTimes;
    }

    private static bool IsValidTime(DateTime generatedTime, TimeSpan delay, DateTime requestedDay)
    {
        
        var dDay = generatedTime.AddMinutes(delay.TotalMinutes);
        dDay = new DateTime(dDay.Year, dDay.Month, dDay.Day, dDay.Hour, dDay.Minute, 0, DateTimeKind.Utc);

        var grather = IsGratherThenNow(dDay);
        var included = IsIncludedInRequestedDay(generatedTime, requestedDay);
        return grather && included;
    }

    private static bool IsGratherThenNow(DateTime desiredDay)
    {
        var now = DateTime.UtcNow;

        return desiredDay >= new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0, DateTimeKind.Utc);
    }

    private static bool IsIncludedInRequestedDay(DateTime generatedTime, DateTime requestedTime)
    {
        var schedule = new Schedule(new TimeOnly(0, 0, 0), new TimeOnly(23, 59, 59), requestedTime.DayOfWeek);

        return schedule.Includes(generatedTime);
    }

    private static HashSet<DateTime> RemoveLunchInterval(DateTime day, HashSet<DateTime> times, HashSet<Schedule> lunchIntervals, Configuration configuration)
    {
        var possibleTimesInLunchInterval = MergeTimes(day, lunchIntervals, configuration);

        return times.Except(possibleTimesInLunchInterval).ToHashSet();
    }

    private static HashSet<DateTime> RemoveDateWithOrdersConflict(DateTime requestedDay, List<Order> orders, HashSet<DateTime> times, Configuration configuration)
    {
        var timesToRemove = new HashSet<DateTime>();

        var timesOfOrder = new HashSet<DateTime>();
        var timesOfOrderPlusServices = new HashSet<DateTime>();

        foreach (var order in orders)
        {
            var orderDate = new DateTime(order.RelocatedSchedule.Year, order.RelocatedSchedule.Month, order.RelocatedSchedule.Day,
                                         order.RelocatedSchedule.Hour, order.RelocatedSchedule.Minute, 0, DateTimeKind.Utc);

            if (!IsValidTime(orderDate, configuration.ScheduleDelayTime, requestedDay))
                continue;

            timesOfOrder.Add(orderDate);

            var servicesTotal = order.Services.Sum(service => service.Duration.TotalMinutes);
            timesOfOrderPlusServices.Add(orderDate.AddMinutes(servicesTotal));
        }

        timesToRemove.UnionWith(timesOfOrder);

        foreach (var timeOfOrder in timesOfOrderPlusServices)
        {
            var timeToRemove = times.LastOrDefault(time => time < timeOfOrder);

            if (timeToRemove == default && !IsValidTime(timeToRemove, configuration.ScheduleDelayTime, requestedDay))
                continue;

            timesToRemove.Add(timeToRemove);
        }

        return times.Except(timesToRemove).ToHashSet();
    }
}
