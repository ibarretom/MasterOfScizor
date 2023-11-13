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

        return GetTimesThatFitInTheRequestedOrder(day, order, scheduleTimesWithStatus, order.Branch.Configuration);
    }

    public static HashSet<ScheduleTimeStatus> ProcessSchedule(DateTime day, OrderBase order, List<Order> orders)
    {
        var schedules = order.Branch.GetScheduleFor(day.DayOfWeek) ?? throw new CompanyException(CompanyExceptionMessagesResource.BRANCH_IS_NOT_OPENNED_THIS_DAY);

        HashSet<ScheduleTimeStatus> allPossibleTimes = GenerateTimes(day, schedules, order.Branch.Configuration);

        if (!allPossibleTimes.Any())
            return new HashSet<ScheduleTimeStatus>();

        allPossibleTimes = HandleOrdersConflict(orders, allPossibleTimes, order.Worker.Services).ToHashSet();

        var lunchIntervalForThisDay = order.Worker.LunchInterval.Where(lunch => schedules.Any(schedule => schedule.Includes(lunch.WeekDay))).ToHashSet();

        if (!lunchIntervalForThisDay.Any())
            return allPossibleTimes;

        return RemoveLunchInterval(day, allPossibleTimes, lunchIntervalForThisDay, order.Branch.Configuration);
    }

    private static HashSet<ScheduleTimeStatus> GenerateTimes(DateTime day, HashSet<Schedule> schedule, Configuration configuration)
    {
        return MergeTimes(day, schedule, configuration);
    }

    private static HashSet<ScheduleTimeStatus> MergeTimes(DateTime day, HashSet<Schedule> schedules, Configuration configuration, bool markAsAvailable = true, bool lastIsAvailable = false)
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

        possiblesTimes.Add(new ScheduleTimeStatus(startTime, markAsAvailable));

        do
        {
            var nextTime = startTime.AddMinutes(configuration.ScheduleDefaultInterval.TotalMinutes);

            var endTimeHasBeenReached = nextTime >= endTime;
            if (!endTimeHasBeenReached)
            {
                possiblesTimes.Add(new ScheduleTimeStatus(nextTime, markAsAvailable));

                startTime = nextTime;

                continue;
            }

            possiblesTimes.Add(new ScheduleTimeStatus(endTime, lastIsAvailable));

            if (!scheduleTimesQueue.TryDequeue(out startTime))
                break;

            if (!scheduleTimesQueue.TryDequeue(out endTime))
                break;

            possiblesTimes.Add(new ScheduleTimeStatus(startTime, markAsAvailable));

        } while (startTime != default);

        return possiblesTimes;
    }

    private static HashSet<ScheduleTimeStatus> HandleOrdersConflict(List<Order> orders, HashSet<ScheduleTimeStatus> times, HashSet<Service> services)
    {
        var timesToRemove = new HashSet<ScheduleTimeStatus>();

        var (TimesOfOrders, TimesOfOrderPlusServices) = GetTimesOfOrderAndOrderPlusServices(orders);

        timesToRemove.UnionWith(TimesOfOrders);

        timesToRemove.UnionWith(GetTimesBasedOnTheLastSmallerTime(TimesOfOrderPlusServices, times));

        var servicesTimesToAdd = GetOrderTimesBasedOnTheOverflowingTime(TimesOfOrderPlusServices, times, services);

        return timesToRemove.Union(servicesTimesToAdd).Union(times).ToHashSet();
    }

    private static (HashSet<ScheduleTimeStatus> TimesOfOrders, HashSet<ScheduleTimeStatus> TimesOfOrderPlusServices) GetTimesOfOrderAndOrderPlusServices(List<Order> orders)
    {
        var timesOfOrder = new HashSet<ScheduleTimeStatus>();

        var timesOfOrderPlusServices = new HashSet<ScheduleTimeStatus>();

        foreach (var order in orders)
        {
            var orderDate = new DateTime(order.RelocatedSchedule.Year, order.RelocatedSchedule.Month, order.RelocatedSchedule.Day,
                                         order.RelocatedSchedule.Hour, order.RelocatedSchedule.Minute, 0, order.RelocatedSchedule.Kind);

            timesOfOrder.Add(new ScheduleTimeStatus(orderDate, false));

            var servicesTotal = order.Services.Sum(service => service.Duration.TotalMinutes);
            var orderPlusServicesTime = orderDate.AddMinutes(servicesTotal);

            timesOfOrderPlusServices.Add(new ScheduleTimeStatus(orderPlusServicesTime, true));
        }

        return (timesOfOrder, timesOfOrderPlusServices);
    }
    private static HashSet<ScheduleTimeStatus> GetTimesBasedOnTheLastSmallerTime(HashSet<ScheduleTimeStatus> timesToBeRemoved, HashSet<ScheduleTimeStatus> referenceTimes)
    {
        var desiredTimes = new HashSet<ScheduleTimeStatus>();

        foreach (var discardTime in timesToBeRemoved)
        {
            var timeToRemove = referenceTimes.LastOrDefault(time => time.Time < discardTime.Time);

            if (timeToRemove == default)
                continue;

            desiredTimes.Add(new ScheduleTimeStatus(timeToRemove.Time, false));
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
        var possibleTimesInLunchInterval = MergeTimes(day, lunchIntervals, configuration, markAsAvailable: false, lastIsAvailable: true) ;

        return possibleTimesInLunchInterval.Union(times).ToHashSet();
    }

    private static HashSet<DateTime> GetTimesThatFitInTheRequestedOrder(DateTime requestedDay, OrderBase order, HashSet<ScheduleTimeStatus> times, Configuration configuration)
    {
        var timesThatTheServicesRequested = new HashSet<DateTime>();

        var servicesTotal = order.Services.Sum(service => service.Duration.TotalMinutes);

        var orderedTimes = times.OrderBy(time => time.Time).ToHashSet();
        var orderedTimesToSearch = new HashSet<ScheduleTimeStatus>(orderedTimes);

        foreach (var time in orderedTimes)
        {
            if (!time.IsAvailable || !IsValidTime(time.Time, configuration.ScheduleDelayTime, requestedDay))
            {
                orderedTimesToSearch.Remove(time);
                continue;
            }

            var timePlusServices = time.Time.AddMinutes(servicesTotal);

            var timeThatFitThisSum = orderedTimesToSearch.LastOrDefault(time => time.Time <= timePlusServices) 
                ?? throw new ScheduleException(ScheduleExceptionMessagesResource.NO_LAST_TIME_FOUND);

            var isEqualAndIsAvailable = timePlusServices == timeThatFitThisSum.Time && timeThatFitThisSum.IsAvailable;
            var isGratherAndIsAvailable = timePlusServices > timeThatFitThisSum.Time && timeThatFitThisSum.IsAvailable;

            if ((!isEqualAndIsAvailable && !isGratherAndIsAvailable) || HasOrderLockedInBetween(time, timeThatFitThisSum, orderedTimesToSearch))
                continue;

            timesThatTheServicesRequested.Add(time.Time);
            orderedTimesToSearch.Remove(time);
        }

        return timesThatTheServicesRequested;
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

    private static bool HasOrderLockedInBetween(ScheduleTimeStatus startTime, ScheduleTimeStatus endTime, HashSet<ScheduleTimeStatus> times)
    {
        var timesToLookAt = new HashSet<ScheduleTimeStatus>(times);
        timesToLookAt.Remove(startTime);

        foreach (var time in timesToLookAt)
        {
            if(time.Time >= endTime.Time)
                break;

            if (time.IsAvailable)
                continue;

            return true;
        };

        return false;
    }

}
