using Domain.Entities.Barbers;
using Domain.Entities.Barbers.Service;
using Domain.Entities.Orders;
using Domain.Exceptions;
using Domain.Exceptions.Messages;
using Domain.ValueObjects.Enums;
using System.Diagnostics;
using Domain.Services.Barbers;

namespace Application.Services.Orders;

internal class OrderPolicy : IOrderPolicy
{
    private readonly IScheduler _scheduler;

    public OrderPolicy(IScheduler scheduler)
    {
        _scheduler = scheduler;
    }

    public bool IsAllowed(Order order, List<Order> allOrders, out string reason)
    {

        var isWorkingInThisDay = order.Branch.GetScheduleFor(order.RelocatedSchedule) is not null;

        if (!isWorkingInThisDay)
        {
            reason = OrderExceptionResourceMessages.ORDER_TIME_IS_NOT_IN_COMMERCIAL_TIME;
            return false;
        }

        if (order.Branch.Configuration.OrderQueueType.Equals(OrderQueueType.Queue))
            return HandleOrderQueue(order, allOrders, order.Branch, out reason);

        return HandleOrderSchedule(order, allOrders, order.Branch, out reason);
    }

    private static bool HandleOrderQueue(Order order, List<Order> allOrders, Branch branch, out string reason)
    {
        if (!branch.IsOpened)
            throw new CompanyException(CompanyExceptionMessagesResource.BRANCH_IS_NOT_OPENNED_THIS_DAY);

        if (IsBeforeDateNowWith20MinutesOfMargin(order))
        {
            reason = OrderExceptionResourceMessages.ORDER_TIME_IS_BEFORE_DATE_NOW;
            return false;
        }

        if (branch.Configuration.QueueAccordingToVirtualQueue)
            return HandleByVirtualQueue(order, allOrders, branch, out reason);

        if (branch.Configuration.QueueLimit is not null)
        { 
            return HandleByQueueLimit(allOrders, branch.Configuration.QueueLimit ?? 999, out reason);
        }

        reason = string.Empty;
        return true;
    }

    private static bool IsBeforeDateNowWith20MinutesOfMargin(Order order)
    {
        return order.RelocatedSchedule < DateTime.UtcNow.AddMinutes(-20);
    }

    private static bool HandleByVirtualQueue(Order order, List<Order> allOrders, Branch branch, out string reason)
    {
        var workingTime = branch.GetScheduleFor(order.RelocatedSchedule) ?? throw new CompanyException(CompanyExceptionMessagesResource.BRANCH_IS_NOT_OPENNED_THIS_DAY);

        var ordersOrderedByTime = allOrders.OrderBy(order => order.RelocatedSchedule);

        var lastOrder = ordersOrderedByTime.LastOrDefault();

        if (lastOrder is not null)
            return HandleWithLastOrder(order, lastOrder, workingTime, out reason);

        if (!IsLessThenClosingTime(order, branch, order.RelocatedSchedule))
        {
            reason = OrderExceptionResourceMessages.ORDER_TIME_IS_GREATER_THEN_COMMERCIAL_TIME;
            return false;
        }

        reason = string.Empty;
        return true;
    }

    private static bool HandleWithLastOrder(Order order, Order lastOrder, Schedule workingTime, out string reason)
    {
        var lastOrderRelocatedTimeWithoutSeconds = new DateTime(lastOrder.RelocatedSchedule.Year, lastOrder.RelocatedSchedule.Month, lastOrder.RelocatedSchedule.Day,
                                                        lastOrder.RelocatedSchedule.Hour, lastOrder.RelocatedSchedule.Minute, 0);

        var lastOrderTimeServicesTime = lastOrder.Services.Aggregate(new TimeSpan(0), (acc, current) => acc + current.Duration);

        var lastOrderTimePlusServiceDuration = lastOrderRelocatedTimeWithoutSeconds.Add(lastOrderTimeServicesTime);

        var orderTimeServices = order.Services.Aggregate(new TimeSpan(0), (acc, current) => acc + current.Duration);

        var (_, EndTime) = Schedule.GetScheduleDateTime(workingTime, workingTime.GetDateToBeReference(order.RelocatedSchedule));

        if (DateTime.Compare(lastOrderTimePlusServiceDuration, DateTime.UtcNow) >= 0)
        {
            lastOrderTimePlusServiceDuration = lastOrderTimePlusServiceDuration.Add(orderTimeServices);

            var isLessThenTheClosingTime = DateTime.Compare(lastOrderTimePlusServiceDuration, EndTime) <= 0;

            if (!isLessThenTheClosingTime)
            {
                reason = OrderExceptionResourceMessages.ORDER_TIME_IS_GREATER_THEN_COMMERCIAL_TIME;
                return false;
            }

            reason = string.Empty;
            return true;
        }

        var orderTime = order.RelocatedSchedule;

        var orderTimePlusServices = orderTime.Add(orderTimeServices);

        var isLessThenClosingTime = orderTimePlusServices <= EndTime;

        if (!isLessThenClosingTime)
        {
            reason = OrderExceptionResourceMessages.ORDER_TIME_IS_GREATER_THEN_COMMERCIAL_TIME;
            return false;
        }

        reason = string.Empty;
        return true;
    }
    private static bool HandleByQueueLimit(List<Order> orders, int queueLimit, out string reason)
    {
        var queueIsNotFull = orders.Count < queueLimit;

        if (queueIsNotFull)
        {
            reason = string.Empty;
            return true;
        }

        reason = OrderExceptionResourceMessages.ORDER_QUEUE_IS_FULL;
        return queueIsNotFull;
    }

    private bool HandleOrderSchedule(Order order, List<Order> allOrders, Branch branch, out string reason)
    {
        var allowedTimes = GetAllowedTimes(order, branch);

        var orderTimeWithoutSeconds = new DateTime(order.RelocatedSchedule.Year, order.RelocatedSchedule.Month,
                        order.RelocatedSchedule.Day, order.RelocatedSchedule.Hour, order.RelocatedSchedule.Minute, 0);

        var desiredTimeForThisOrder = allowedTimes.FirstOrDefault(time => DateTime.Compare(time, orderTimeWithoutSeconds) == 0);

        if (desiredTimeForThisOrder == default)
        {
            reason = OrderExceptionResourceMessages.ORDER_TIME_NOT_ALLOWED;
            return false;
        }

        if (IsInLunchInterval(order, desiredTimeForThisOrder, branch))
        {
            reason = OrderExceptionResourceMessages.ORDER_TIME_IS_IN_LUNCH_INTERVAL;
            return false;
        }

        if (!IsLessThenClosingTime(order, branch, desiredTimeForThisOrder))
        {
            reason = OrderExceptionResourceMessages.ORDER_TIME_IS_GREATER_THEN_COMMERCIAL_TIME;
            return false;
        }

        var firstOrderBeforeRequestedOrder = allOrders.LastOrDefault(order => DateTime.Compare(new DateTime(order.RelocatedSchedule.Year, order.RelocatedSchedule.Month,
                        order.RelocatedSchedule.Day, order.RelocatedSchedule.Hour, order.RelocatedSchedule.Minute, 0), desiredTimeForThisOrder) <= 0);

        var availableTimes = _scheduler.GetAvailable(order.RelocatedSchedule, order, allOrders);

        if (!availableTimes.Contains(order.RelocatedSchedule))
        {
            reason = OrderExceptionResourceMessages.ORDER_TIME_ALREADY_ALOCATED;
            return false;
        }

        reason = string.Empty;
        return true;
    }

    private static List<DateTime> GetAllowedTimes(Order order, Branch branch, bool withLastTime = false)
    {
        var interval = branch.Configuration.ScheduleDefaultInterval;

        var scheduleForOrderDay = branch.GetScheduleFor(order.RelocatedSchedule) ?? throw new CompanyException(CompanyExceptionMessagesResource.BRANCH_IS_NOT_OPENNED_THIS_DAY);

        var (StartTime, EndTime) = Schedule.GetScheduleDateTime(scheduleForOrderDay, scheduleForOrderDay.GetDateToBeReference(order.RelocatedSchedule));

        var allowedTimes = new List<DateTime>();

        for (var currentTime = StartTime;
                withLastTime ? DateTime.Compare(currentTime, EndTime) <= 0 : DateTime.Compare(currentTime, EndTime) < 0;
                currentTime = currentTime.Add(interval))
        {
            allowedTimes.Add(currentTime);
        }

        return allowedTimes;
    }

    private static bool IsInLunchInterval(Order order, DateTime desiredTime, Branch branch)
    {
        var employee = branch.Barber.FirstOrDefault(barber => barber.Id == order.Worker.Id) ?? throw new OrderException(OrderExceptionResourceMessages.EMPLOYEE_DOES_NOT_EXISTS);

        var scheduleForScheduleOrderDate = branch.GetScheduleFor(order.RelocatedSchedule) ?? throw new CompanyException(CompanyExceptionMessagesResource.BRANCH_IS_NOT_OPENNED_THIS_DAY);

        var employeeLunchInterval = employee.LunchInterval.FirstOrDefault(interval => scheduleForScheduleOrderDate.Includes(interval));

        if (employeeLunchInterval is null)
            return false;

        var (StartTime, EndTime) = Schedule.GetScheduleDateTime(employeeLunchInterval, 
                                                                employeeLunchInterval.GetDateToBeReference(order.RelocatedSchedule));

        var desiredTimePlusServiceDuration = desiredTime.Add(order.Services.Aggregate(new TimeSpan(0), (acc, current) => acc + current.Duration));

        if (DateTime.Compare(desiredTimePlusServiceDuration, StartTime) > 0 && DateTime.Compare(desiredTime, EndTime) < 0)
            return true;

        if (TotalOrderTimeOverflowLunchInterval(order, desiredTime, StartTime, EndTime))
            return true;

        return false;
    }

    private static bool TotalOrderTimeOverflowLunchInterval(Order order, DateTime desiredTime, DateTime lunchStart, DateTime lunchEnd)
    {
        if (desiredTime > lunchEnd)
            return false;

        var totalTimeServices = order.Services.Aggregate(new TimeSpan(0), (acc, current) => acc + current.Duration);

        var orderTimePlusTotalTimeServices = desiredTime.Add(totalTimeServices);

        if (DateTime.Compare(orderTimePlusTotalTimeServices, lunchStart) > 0)
            return true;

        return false;
    }

    private static bool IsLessThenClosingTime(Order order, Branch branch, DateTime desiredTimeForThisOrder)
    {
        var desiredDay = branch.GetScheduleFor(order.RelocatedSchedule) ?? throw new CompanyException(CompanyExceptionMessagesResource.BRANCH_IS_NOT_OPENNED_THIS_DAY);

        var (StartTime, EndTime) = Schedule.GetScheduleDateTime(desiredDay, desiredDay.GetDateToBeReference(order.RelocatedSchedule));

        var totalTimeServices = order.Services.Aggregate(new TimeSpan(0), (acc, current) => acc + current.Duration);

        var desiredTimeWithoutSeconds = new DateTime(desiredTimeForThisOrder.Year, desiredTimeForThisOrder.Month, desiredTimeForThisOrder.Day,
                                                     desiredTimeForThisOrder.Hour, desiredTimeForThisOrder.Minute, 0);

        return DateTime.Compare(desiredTimeWithoutSeconds.Add(totalTimeServices), EndTime) <= 0;
    }
}
