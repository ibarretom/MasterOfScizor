using Domain.Entities.Barbers;
using Domain.Entities.Barbers.Service;
using Domain.Entities.Orders;
using Domain.Exceptions;
using Domain.Exceptions.Messages;
using Domain.ValueObjects.Barbers;

namespace Domain.Services.Barbers;

internal class Scheduler : IScheduler
{
    public HashSet<DateTime> GetAvailable(DateTime day, OrderBase order, List<Order> orders)
    {
        var scheduleTimesWithStatus = ProcessSchedule(day, order, orders);

        return scheduleTimesWithStatus
                .Where(scheduleTime => scheduleTime.IsAvailable)
                .Select(scheduleTime => scheduleTime.Time).ToHashSet();
    }

    public static HashSet<ScheduleTimeStatus> ProcessSchedule(DateTime day, OrderBase order, List<Order> orders)
    {
        var schedules = order.Branch.GetScheduleFor(day.DayOfWeek) ?? throw new CompanyException(CompanyExceptionMessagesResource.BRANCH_IS_NOT_OPENNED_THIS_DAY);

        HashSet<ScheduleTimeStatus> allPossibleTimes = GenerateTimes(day, schedules, order.Branch.Configuration);

        if (!allPossibleTimes.Any())
            return new HashSet<ScheduleTimeStatus>();

        allPossibleTimes = HandleOrdersConflict(day, orders, allPossibleTimes, order.Branch.Configuration, order.Worker.Services).ToHashSet();

        var lunchIntervalForThisDay = order.Worker.LunchInterval.Where(lunch => schedules.Any(schedule => schedule.Includes(lunch.WeekDay))).ToHashSet();

        if (!lunchIntervalForThisDay.Any())
            return allPossibleTimes;

        return RemoveLunchInterval(day, allPossibleTimes, lunchIntervalForThisDay, order.Branch.Configuration);
    }
    private static HashSet<ScheduleTimeStatus> GenerateTimes(DateTime day, HashSet<Schedule> schedule, Configuration configuration)
    {
        return MergeTimes(day, schedule, configuration);
    }

    private static HashSet<ScheduleTimeStatus> MergeTimes(DateTime day, HashSet<Schedule> schedules, Configuration configuration, bool markAsAvailable = true)
    {
        var possiblesTimes = new HashSet<ScheduleTimeStatus>();

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
            possiblesTimes.Add(new ScheduleTimeStatus(startTime, markAsAvailable));

        do
        {
            var nextTime = startTime.AddMinutes(configuration.ScheduleDefaultInterval.TotalMinutes);

            var endTimeHasBeenReached = nextTime >= endTime;
            if (!endTimeHasBeenReached)
            {
                if (IsValidTime(nextTime, configuration.ScheduleDelayTime, day))
                    possiblesTimes.Add(new ScheduleTimeStatus(nextTime, markAsAvailable));

                startTime = nextTime;
                continue;
            }

            if (!scheduleTimesQueue.TryDequeue(out startTime))
                break;

            if (!scheduleTimesQueue.TryDequeue(out endTime))
                break;

            if (IsValidTime(startTime, configuration.ScheduleDelayTime, day))
                possiblesTimes.Add(new ScheduleTimeStatus(startTime, markAsAvailable));

        } while (startTime != default);

        return possiblesTimes;
    }

    private static bool IsValidTime(DateTime generatedTime, TimeSpan delay, DateTime requestedDay)
    {

        var dDay = generatedTime.AddMinutes(delay.TotalMinutes);
        dDay = new DateTime(dDay.Year, dDay.Month, dDay.Day, dDay.Hour, dDay.Minute, 0, dDay.Kind);

        var grather = IsGratherThenNow(dDay);
        var included = IsIncludedInRequestedDay(generatedTime, requestedDay);
        return grather && included;
    }

    private static bool IsGratherThenNow(DateTime desiredDay)
    {
        var now = DateTime.UtcNow;

        return desiredDay >= new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0, now.Kind);
    }

    private static bool IsIncludedInRequestedDay(DateTime generatedTime, DateTime requestedTime)
    {
        var schedule = new Schedule(new TimeOnly(0, 0, 0), new TimeOnly(23, 59, 59), requestedTime.DayOfWeek);

        return schedule.Includes(generatedTime);
    }

    private static HashSet<ScheduleTimeStatus> HandleOrdersConflict(DateTime requestedDay, List<Order> orders, HashSet<ScheduleTimeStatus> times, Configuration configuration, HashSet<Service> services)
    {
        var timesToRemove = new HashSet<ScheduleTimeStatus>();

        var (TimesOfOrders, TimesOfOrderPlusServices) = GetTimesOfOrderAndOrderPlusServices(requestedDay, orders, configuration);

        timesToRemove.UnionWith(TimesOfOrders);

        timesToRemove.UnionWith(GetTimesBasedOnTheLastSmallerTime(requestedDay, TimesOfOrderPlusServices, times, configuration));

        var servicesTimesToAdd = GetOrderTimesBasedOnTheOverflowingTime(TimesOfOrderPlusServices, times, services);

        return times.Except(timesToRemove).Union(servicesTimesToAdd).ToHashSet();
    }

    private static (HashSet<ScheduleTimeStatus> TimesOfOrders, HashSet<ScheduleTimeStatus> TimesOfOrderPlusServices) GetTimesOfOrderAndOrderPlusServices(DateTime requestedDay, List<Order> orders, Configuration configuration)
    {
        var timesOfOrder = new HashSet<ScheduleTimeStatus>();

        var timesOfOrderPlusServices = new HashSet<ScheduleTimeStatus>();

        foreach (var order in orders)
        {
            var orderDate = new DateTime(order.RelocatedSchedule.Year, order.RelocatedSchedule.Month, order.RelocatedSchedule.Day,
                                         order.RelocatedSchedule.Hour, order.RelocatedSchedule.Minute, 0, order.RelocatedSchedule.Kind);

            if (IsValidTime(orderDate, configuration.ScheduleDelayTime, requestedDay))
                timesOfOrder.Add(new ScheduleTimeStatus(orderDate, false));


            var servicesTotal = order.Services.Sum(service => service.Duration.TotalMinutes);
            var orderPlusServicesTime = orderDate.AddMinutes(servicesTotal);

            if (IsValidTime(orderPlusServicesTime, configuration.ScheduleDelayTime, requestedDay))
                timesOfOrderPlusServices.Add(new ScheduleTimeStatus(orderPlusServicesTime, true));
        }

        return (timesOfOrder, timesOfOrderPlusServices);
    }
    private static HashSet<ScheduleTimeStatus> GetTimesBasedOnTheLastSmallerTime(DateTime requestedDay, HashSet<ScheduleTimeStatus> timesToBeRemoved, HashSet<ScheduleTimeStatus> referenceTimes, Configuration configuration)
    {
        var desiredTimes = new HashSet<ScheduleTimeStatus>();

        foreach (var discardTime in timesToBeRemoved)
        {
            var timeToRemove = referenceTimes.LastOrDefault(time => time.Time < discardTime.Time);

            if (timeToRemove == default)
                continue;

            if (!IsValidTime(timeToRemove.Time, configuration.ScheduleDelayTime, requestedDay))
                continue;

            desiredTimes.Add(timeToRemove);
        }

        return desiredTimes;
    }

    private static HashSet<ScheduleTimeStatus> GetOrderTimesBasedOnTheOverflowingTime(HashSet<ScheduleTimeStatus> timesOfServices, HashSet<ScheduleTimeStatus> referenceTimes, HashSet<Service> services)
    {
        var timesToAdd = new HashSet<ScheduleTimeStatus>();
        if (!services.Any())
            throw new ScheduleException(ScheduleExceptionMessagesResource.WORKER_HAS_NO_SERVICES);

        var minimumServiceTime = services.Min(service => service.Duration);

        foreach (var timeOfServices in timesOfServices)
        {
            var nextTimeAfterTimeServices = referenceTimes.FirstOrDefault(time => time.Time > timeOfServices.Time);

            if (nextTimeAfterTimeServices == default)
                continue;

            var timeOfServicesMinuesNextTime = (nextTimeAfterTimeServices.Time - timeOfServices.Time).TotalMinutes;

            if (timeOfServicesMinuesNextTime < minimumServiceTime.TotalMinutes)
                continue;

            timesToAdd.Add(timeOfServices);
        }

        return timesToAdd;
    }

    private static HashSet<ScheduleTimeStatus> RemoveLunchInterval(DateTime day, HashSet<ScheduleTimeStatus> times, HashSet<Schedule> lunchIntervals, Configuration configuration)
    {
        var possibleTimesInLunchInterval = MergeTimes(day, lunchIntervals, configuration);

        return times.Except(possibleTimesInLunchInterval).ToHashSet();
    }
}
