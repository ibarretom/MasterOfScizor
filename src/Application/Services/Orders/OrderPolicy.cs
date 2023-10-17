﻿using Domain.Entities.Barbers;
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

        var isWorkingInThisDay = branch.GetScheduleFor(order.RelocatedSchedule) is not null;

        if (!isWorkingInThisDay)
            throw new OrderException(OrderExceptionResourceMessages.ORDER_TIME_IS_NOT_IN_COMMERCIAL_TIME);

        if (branch.Configuration.OrderQueueType.Equals(OrderQueueType.Queue))
            return HandleOrderQueue(order, allOrders, branch);

        return HandleOrderSchedule(order, allOrders, branch);
    }

    private static bool HandleOrderQueue(Order order, List<Order> allOrders, Branch branch)
    {
        if (!branch.IsOpened)
            throw new CompanyException(CompanyExceptionMessagesResource.BRANCH_IS_NOT_OPENNED_THIS_DAY);

        if (IsBeforeDateNowWith20MinutesOfMargin(order))
            throw new OrderException(OrderExceptionResourceMessages.ORDER_TIME_IS_BEFORE_DATE_NOW);

        if (branch.Configuration.QueueAccordingToVirtualQueue)
            return HandleByVirtualQueue(order, allOrders, branch);

        if (branch.Configuration.QueueLimit is not null)
            return HandleByQueueLimit(allOrders, branch.Configuration.QueueLimit ?? 999);

        return true;
    }

    private static bool IsBeforeDateNowWith20MinutesOfMargin(Order order)
    {
        return order.RelocatedSchedule < DateTime.UtcNow.AddMinutes(-20);
    }

    private static bool HandleByVirtualQueue(Order order, List<Order> allOrders, Branch branch)
    {
        var workingTime = branch.GetScheduleFor(order.RelocatedSchedule) ?? throw new CompanyException(CompanyExceptionMessagesResource.BRANCH_IS_NOT_OPENNED_THIS_DAY);

        var ordersOrderedByTime = allOrders.OrderBy(order => order.RelocatedSchedule);

        var lastOrder = ordersOrderedByTime.LastOrDefault();

        if (lastOrder is not null)
            return HandleWithLastOrder(order, lastOrder, workingTime);


        return IsLessThenClosingTime(order, branch, order.RelocatedSchedule);
    }

    private static bool HandleWithLastOrder(Order order, Order lastOrder, Schedule workingTime)
    {
        var lastOrderRealocatedTimeWithoutSeconds = new DateTime(lastOrder.RelocatedSchedule.Year, lastOrder.RelocatedSchedule.Month, lastOrder.RelocatedSchedule.Day,
                                                        lastOrder.RelocatedSchedule.Hour, lastOrder.RelocatedSchedule.Minute, 0);

        var lastOrderTimeServicesTime = lastOrder.Services.Aggregate(new TimeSpan(0), (acc, current) => acc + current.Duration);

        var lastOrderTimePlusServiceDuration = lastOrderRealocatedTimeWithoutSeconds.Add(lastOrderTimeServicesTime);

        var orderTimeServices = order.Services.Aggregate(new TimeSpan(0), (acc, current) => acc + current.Duration);

        var (_, EndTime) = Schedule.GetScheduleDateTime(workingTime, workingTime.GetDateToBeReference(order.RelocatedSchedule));

        if (DateTime.Compare(lastOrderTimePlusServiceDuration, DateTime.UtcNow) >= 0)
        {
            lastOrderTimePlusServiceDuration = lastOrderTimePlusServiceDuration.Add(orderTimeServices);

            var isLessThenTheClosingTime = DateTime.Compare(lastOrderTimePlusServiceDuration, EndTime) <= 0;

            if (!isLessThenTheClosingTime)
                return false;

            return true;
        }

        var orderTime = order.RelocatedSchedule;

        var orderTimePlusServices = orderTime.Add(orderTimeServices);

        var isLessThenClosingTime = orderTimePlusServices <= EndTime;

        if (!isLessThenClosingTime)
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

        var orderTimeWithoutSeconds = new DateTime(order.RelocatedSchedule.Year, order.RelocatedSchedule.Month,
                        order.RelocatedSchedule.Day, order.RelocatedSchedule.Hour, order.RelocatedSchedule.Minute, 0);

        var desiredTimeForThisOrder = allowedTimes.FirstOrDefault(time => DateTime.Compare(time, orderTimeWithoutSeconds) == 0);

        if (desiredTimeForThisOrder == default)
            throw new OrderException(OrderExceptionResourceMessages.ORDER_TIME_NOT_ALLOWED);

        if (IsInLunchInterval(order, desiredTimeForThisOrder, branch))
            throw new OrderException(OrderExceptionResourceMessages.ORDER_TIME_IS_IN_LUNCH_INTERVAL);

        if (!IsLessThenClosingTime(order, branch, desiredTimeForThisOrder))
            return false;

        var firstOrderBeforeRequestedOrder = allOrders.LastOrDefault(order => DateTime.Compare(new DateTime(order.RelocatedSchedule.Year, order.RelocatedSchedule.Month,
                        order.RelocatedSchedule.Day, order.RelocatedSchedule.Hour, order.RelocatedSchedule.Minute, 0), desiredTimeForThisOrder) <= 0);

        if (HasConflictWithThePreviousOrder(desiredTimeForThisOrder, firstOrderBeforeRequestedOrder, GetAllowedTimes(order, branch, true)))
            return false;

        var firstOrderAfterRequestedOrder = allOrders.FirstOrDefault(order => DateTime.Compare(new DateTime(order.RelocatedSchedule.Year, order.RelocatedSchedule.Month,
                        order.RelocatedSchedule.Day, order.RelocatedSchedule.Hour, order.RelocatedSchedule.Minute, 0), desiredTimeForThisOrder) >= 0);

        if (HasConflictWithTheNextOrder(desiredTimeForThisOrder, order.Services, firstOrderAfterRequestedOrder, GetAllowedTimes(order,branch, true)))
            return false;

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

        if (employee.LunchInterval is null)
            return false;

        var (StartTime, EndTime) = Schedule.GetScheduleDateTime(employee.LunchInterval, order.RelocatedSchedule);

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

    private static bool HasConflictWithThePreviousOrder(DateTime desiredTime, Order? previousOrder, List<DateTime> allowedTimes)
    {
        if (previousOrder is null)
            return false;

        var previousOrderTimeWithoutSeconds = new DateTime(previousOrder.RelocatedSchedule.Year, previousOrder.RelocatedSchedule.Month, previousOrder.RelocatedSchedule.Day,
                                                           previousOrder.RelocatedSchedule.Hour, previousOrder.RelocatedSchedule.Minute, 0);

        if (DateTime.Compare(desiredTime, previousOrderTimeWithoutSeconds) == 0)
            throw new OrderException(OrderExceptionResourceMessages.ORDER_TIME_ALREADY_ALOCATED);

        var totalTimeService = previousOrder.Services.Aggregate(new TimeSpan(0), (acc, current) => acc + current.Duration);

        var previousOrderTimePlusServiceDuration = previousOrderTimeWithoutSeconds.Add(totalTimeService);

        var previousOrderEndTimeFittedOnAvailableTimes = allowedTimes.FirstOrDefault(time => DateTime.Compare(time, previousOrderTimePlusServiceDuration) >= 0);

        return DateTime.Compare(desiredTime, previousOrderEndTimeFittedOnAvailableTimes) < 0;
    }

    private static bool HasConflictWithTheNextOrder(DateTime desiredTime, List<Service> desiredOrderServices, Order? nextOrder, List<DateTime> allowedTimes)
    {
        if (nextOrder is null)
            return false;

        if (DateTime.Compare(desiredTime, nextOrder.RelocatedSchedule) == 0)
            throw new OrderException(OrderExceptionResourceMessages.ORDER_TIME_ALREADY_ALOCATED);

        var totalTimeService = desiredOrderServices.Aggregate(new TimeSpan(0), (acc, current) => acc + current.Duration);

        var desiredTimePlusServiceDuration = desiredTime.Add(totalTimeService);

        var desiredTimeEndFittedOnAvailableTimes = allowedTimes.FirstOrDefault(time => DateTime.Compare(time, desiredTimePlusServiceDuration) >= 0);

        var nextOrderTimeWithoutSeconds = new DateTime(nextOrder.RelocatedSchedule.Year, nextOrder.RelocatedSchedule.Month, nextOrder.RelocatedSchedule.Day,
                                                       nextOrder.RelocatedSchedule.Hour, nextOrder.RelocatedSchedule.Minute, 0);
        
        return DateTime.Compare(desiredTimeEndFittedOnAvailableTimes, nextOrderTimeWithoutSeconds) > 0;
    }
}
