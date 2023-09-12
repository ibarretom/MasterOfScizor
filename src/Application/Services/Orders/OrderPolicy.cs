using Domain.Entities.Barbers;
using Domain.Entities.Barbers.Service;
using Domain.Entities.Orders;
using Domain.Exceptions;
using Domain.Exceptions.Messages;
using Domain.ValueObjects.Enums;

namespace Application.Services.Orders;

internal class OrderPolicy : IOrderPolicy
{
    public bool IsAllowed(Order order, List<Order> allOrders, Branch branch)
    {
        var isWorkingInThisDay = branch.GetScheduleFor(order.ScheduleTime.DayOfWeek) is not null;

        if (!isWorkingInThisDay)
            throw new CompanyException(CompanyExceptionMessagesResource.BRANCH_IS_NOT_OPENNED_THIS_DAY);

        if (!IsInCommercialTime(order, branch))
            throw new OrderException(OrderExceptionResourceMessages.ORDER_TIME_IS_NOT_IN_COMMERCIAL_TIME);

        if (branch.Configuration.OrderQueueType.Equals(OrderQueueType.Queue))
            return HandleOrderQueue(order, allOrders, branch);

        return HandleOrderSchedule(order, allOrders, branch);
    }

    private static bool IsInCommercialTime(Order order, Branch branch)
    {
        var workingTime = branch.GetScheduleFor(order.ScheduleTime.DayOfWeek) ?? throw new CompanyException(CompanyExceptionMessagesResource.BRANCH_IS_NOT_OPENNED_THIS_DAY);

        var orderTime = TimeSpan.FromTicks(order.ScheduleTime.Ticks);

        var workingTimeStart = TimeSpan.FromTicks(workingTime.StartTime.Ticks);

        if (orderTime < workingTimeStart)
            return false;

        var workingTimeEnd = TimeSpan.FromTicks(workingTime.EndTime.Ticks);

        if (orderTime > workingTimeEnd)
            return false;

        return true;
    }

    private static bool HandleOrderQueue(Order order, List<Order> allOrders, Branch branch)
    {
        if (!branch.IsOpened)
            throw new CompanyException(CompanyExceptionMessagesResource.BRANCH_IS_NOT_OPENNED_THIS_DAY);

        if(IsBeforeDateNowWith20MinutesOfMargin(order))
            throw new OrderException(OrderExceptionResourceMessages.ORDER_TIME_IS_BEFORE_DATE_NOW);

        if (branch.Configuration.QueueAccordingToVirtualQueue)
            return HandleByVirtualQueue(order, allOrders, branch);

        if (branch.Configuration.QueueLimit is not null)
            return HandleByQueueLimit(allOrders, branch.Configuration.QueueLimit ?? 999);

        return true;
    }

    private static bool IsBeforeDateNowWith20MinutesOfMargin(Order order)
    {
        return order.ScheduleTime < DateTime.UtcNow.AddMinutes(-20);
    }

    private static bool HandleByVirtualQueue(Order order, List<Order> allOrders, Branch branch)
    {
        var workingTime = branch.GetScheduleFor(order.ScheduleTime.DayOfWeek) ?? throw new CompanyException(CompanyExceptionMessagesResource.BRANCH_IS_NOT_OPENNED_THIS_DAY);

        var ordersOrderedByTime = allOrders.OrderBy(order => order.ScheduleTime);

        var lastOrder = ordersOrderedByTime.LastOrDefault();

        if (lastOrder is not null)
            return HandleWithLastOrder(order, lastOrder, workingTime);


        return IsLessThenClosingTime(order, branch, TimeSpan.FromTicks(order.ScheduleTime.Ticks));
    }

    private static bool HandleWithLastOrder(Order order, Order lastOrder, Schedule workingTime)
    {
        var lastOrderTime = TimeSpan.FromTicks(lastOrder.RelocatedSchedule.Ticks);

        var lastOrderTimeServicesTime = lastOrder.Services.Aggregate(new TimeSpan(0), (acc, current) => acc + current.Duration);

        var lastOrderTimePlusServiceDuration = lastOrderTime.Add(lastOrderTimeServicesTime);

        var orderTimeServices = order.Services.Aggregate(new TimeSpan(0), (acc, current) => acc + current.Duration);

        lastOrderTimePlusServiceDuration = lastOrderTimePlusServiceDuration.Add(orderTimeServices);

        var endingTime = TimeSpan.FromTicks(workingTime.EndTime.Ticks);

        var isLessThenTheClosingTime = lastOrderTimePlusServiceDuration <= endingTime;

        if (!isLessThenTheClosingTime)
            return false;

        return true;
    }
    private static bool HandleByQueueLimit(List<Order> orders, int queueLimit)
    {
        return orders.Count < queueLimit;
    }

    private static bool HandleOrderSchedule(Order order, List<Order> allOrders, Branch branch)
    {
        var allowedTimes = GetAllowedTimes(order, branch);

        var desiredTimeForThisOrder = allowedTimes.FirstOrDefault(time => time == TimeSpan.FromTicks(order.ScheduleTime.Ticks));

        if (desiredTimeForThisOrder == default)
            throw new OrderException(OrderExceptionResourceMessages.ORDER_TIME_NOT_ALLOWED);

        if(IsInLunchInterval(order, desiredTimeForThisOrder, branch))
            throw new OrderException(OrderExceptionResourceMessages.ORDER_TIME_IS_IN_LUNCH_INTERVAL);

        if (!IsLessThenClosingTime(order, branch, desiredTimeForThisOrder))
            return false;

        var firstOrderBeforeRequestedOrder = allOrders.LastOrDefault(order => TimeSpan.FromTicks(order.ScheduleTime.Ticks) <= desiredTimeForThisOrder);

        if (HasConflictWithThePreviousOrder(desiredTimeForThisOrder, firstOrderBeforeRequestedOrder, allowedTimes))
            return false;
        
        var firstOrderAfterRequestedOrder = allOrders.FirstOrDefault(order => TimeSpan.FromTicks(order.ScheduleTime.Ticks) >= desiredTimeForThisOrder);

        if (HasConflictWithTheNextOrder(desiredTimeForThisOrder, order.Services, firstOrderAfterRequestedOrder, allowedTimes))
            return false;

        return true;
    }

    private static List<TimeSpan> GetAllowedTimes(Order order, Branch branch)
    {
        var interval = branch.Configuration.ScheduleDefaultInterval;

        var scheduleForOrderDay = branch.GetScheduleFor(order.ScheduleTime.DayOfWeek) ?? throw new CompanyException(CompanyExceptionMessagesResource.BRANCH_IS_NOT_OPENNED_THIS_DAY);

        var allowedTimes = new List<TimeSpan>();

        for (var currentTime = scheduleForOrderDay.StartTime; currentTime < scheduleForOrderDay.EndTime; currentTime = currentTime.AddMinutes(interval.TotalMinutes))
        {
            if (currentTime.Add(interval) > scheduleForOrderDay.EndTime)
                break;

            allowedTimes.Add(TimeSpan.FromTicks(currentTime.Ticks));
        }

        return allowedTimes;
    }

    private static bool IsInLunchInterval(Order order, TimeSpan desiredTime, Branch branch)
    {
        var employee = branch.Barber.FirstOrDefault(barber => barber.Id == order.WorkerId) ?? throw new OrderException(OrderExceptionResourceMessages.EMPLOYEE_DOES_NOT_EXISTS);

        var lunchTime = employee.LunchInterval;

        if (lunchTime is null)
            return false;

        var lunchTimeStart = TimeSpan.FromTicks(lunchTime.StartTime.Ticks);

        var lunchTimeEnd = TimeSpan.FromTicks(lunchTime.EndTime.Ticks);

        var desiredTimePlusServiceDuration = desiredTime.Add(order.Services.Aggregate(new TimeSpan(0), (acc, current) => acc + current.Duration));

        if (desiredTimePlusServiceDuration > lunchTimeStart && desiredTime < lunchTimeEnd)
            return true;

        if (TotalOrderTimeOverflowLunchInterval(order, desiredTime, lunchTimeStart, lunchTimeEnd))
            return true;

        return false;
    }

    private static bool TotalOrderTimeOverflowLunchInterval(Order order, TimeSpan desiredTime, TimeSpan lunchStart, TimeSpan lunchEnd)
    {
        if (desiredTime > lunchEnd)
            return false;

        var totalTimeServices = order.Services.Aggregate(new TimeSpan(0), (acc, current) => acc + current.Duration);

        var orderTimePlusTotalTimeServices = desiredTime.Add(totalTimeServices);

        var lunchIntervalStart = TimeSpan.FromTicks(lunchStart.Ticks);

        if (orderTimePlusTotalTimeServices > lunchIntervalStart)
            return true;

        return false;
    }

    private static bool IsLessThenClosingTime(Order order, Branch branch, TimeSpan desiredTimeForThisOrder)
    {
        var desiredDay = branch.GetScheduleFor(order.ScheduleTime.DayOfWeek) ?? throw new CompanyException(CompanyExceptionMessagesResource.BRANCH_IS_NOT_OPENNED_THIS_DAY);
        
        var totalTimeServices = order.Services.Aggregate(new TimeSpan(0), (acc, current) => acc + current.Duration);

        return desiredTimeForThisOrder.Add(totalTimeServices) <= TimeSpan.FromTicks(desiredDay.EndTime.Ticks);
    }

    private static bool HasConflictWithThePreviousOrder(TimeSpan desiredTime, Order? previousOrder, List<TimeSpan> allowedTimes)
    {
        if(previousOrder is null)
            return false;

        var previousOrderTime = TimeSpan.FromTicks(previousOrder.ScheduleTime.Ticks);

        if (desiredTime == previousOrderTime)
            throw new OrderException(OrderExceptionResourceMessages.ORDER_TIME_ALREADY_ALOCATED);

        var totalTimeService = previousOrder.Services.Aggregate(new TimeSpan(0), (acc, current) => acc + current.Duration);

        var previousOrderTimePlusServiceDuration = previousOrderTime.Add(totalTimeService);

        var previousOrderEndTimeFittedOnAvailableTimes = allowedTimes.FirstOrDefault(time => time >= previousOrderTimePlusServiceDuration);

        return desiredTime < previousOrderEndTimeFittedOnAvailableTimes;
    }

    private static bool HasConflictWithTheNextOrder(TimeSpan desiredTime, List<Service> desiredOrderServices, Order? nextOrder,  List<TimeSpan> allowedTimes)
    {
        if (nextOrder is null)
            return false;

        var nextOrderTime = TimeSpan.FromTicks(nextOrder.ScheduleTime.Ticks);

        if (desiredTime == nextOrderTime)
            throw new OrderException(OrderExceptionResourceMessages.ORDER_TIME_ALREADY_ALOCATED);

        var totalTimeService = desiredOrderServices.Aggregate(new TimeSpan(0), (acc, current) => acc + current.Duration);

        var desiredTimePlusServiceDuration = desiredTime.Add(totalTimeService);

        var desiredTimeEndFittedOnAvailableTimes = allowedTimes.FirstOrDefault(time => time >= desiredTimePlusServiceDuration);
        
        return nextOrderTime < desiredTimeEndFittedOnAvailableTimes;
    }
}
