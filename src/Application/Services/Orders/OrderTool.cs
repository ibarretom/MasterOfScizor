using Domain.Entities.Barbers;
using Domain.Entities.Orders;
using Domain.Exceptions.Messages;
using Domain.Exceptions;
using Domain.ValueObjects.Enums;
using Domain.Entities;

namespace Application.Services.Orders;

internal class OrderTool : IOrderTool
{
    public DateTime RelocateTime(Order order, List<Order> allOrders, Branch branch)
    {
        if (!(branch.Configuration.OrderQueueType == OrderQueueType.Queue))
            return order.ScheduleTime;

        var orderEmployee = branch.Barber.FirstOrDefault(employee => employee.Id == order.Worker.Id) ?? throw new OrderException(OrderExceptionResourceMessages.EMPLOYEE_DOES_NOT_EXISTS);

        var relocatedOrder = new Order(order.Branch, order.Worker, order.Services, order.User, order.Status, order.ScheduleTime);

        var servicesTotalDuration = order.Services.Aggregate(TimeSpan.Zero, (total, service) => total + service.Duration);

        var orderTimePlusServiceDuration = order.RelocatedSchedule.Add(servicesTotalDuration - branch.Configuration.ScheduleDelayTime);

        if (orderEmployee.LunchInterval is not null && orderEmployee.LunchInterval.Includes(orderTimePlusServiceDuration))
            relocatedOrder = SkipLunchInterval(order, orderEmployee);

        if (!allOrders.Any())
            return new DateTime(relocatedOrder.RelocatedSchedule.Year, relocatedOrder.RelocatedSchedule.Month, relocatedOrder.RelocatedSchedule.Day,
                                relocatedOrder.RelocatedSchedule.Hour, relocatedOrder.RelocatedSchedule.Minute, 0, relocatedOrder.RelocatedSchedule.Kind);

        var lastOrder = allOrders.Last();

        var lastOrderServiceDuration = lastOrder.Services.Aggregate(TimeSpan.Zero, (total, service) => total + service.Duration);

        var lastOrderTimePlusServiceDuration = lastOrder.RelocatedSchedule.Add(lastOrderServiceDuration);

        var relocatedScheduleWithoutSeconds = new DateTime(relocatedOrder.RelocatedSchedule.Year, relocatedOrder.RelocatedSchedule.Month, relocatedOrder.RelocatedSchedule.Day,
                                                                      relocatedOrder.RelocatedSchedule.Hour, relocatedOrder.RelocatedSchedule.Minute, 0, relocatedOrder.RelocatedSchedule.Kind);

        var lastOrderIsBeforeRelocatedOrder = relocatedScheduleWithoutSeconds > lastOrderTimePlusServiceDuration;
        
        if (lastOrderIsBeforeRelocatedOrder)
            return relocatedScheduleWithoutSeconds;

        return new DateTime(lastOrderTimePlusServiceDuration.Year, lastOrderTimePlusServiceDuration.Month, lastOrderTimePlusServiceDuration.Day,
                            lastOrderTimePlusServiceDuration.Hour, lastOrderTimePlusServiceDuration.Minute, 0, lastOrderTimePlusServiceDuration.Kind);
    }

    private static Order SkipLunchInterval(Order order, Employee employee)
    {
        var lunchInterval = employee.LunchInterval ?? throw new OrderException(UserExceptionMessagesResource.EMPLOYEE_DOES_NOT_HAVE_LUNCH_INTERVAL);

        var orderRelocated = new Order(order.Branch, order.Worker, order.Services, order.User, order.Status, order.ScheduleTime)
        {
            RelocatedSchedule = new DateTime(order.ScheduleTime.Year, order.ScheduleTime.Month, order.ScheduleTime.Day,
                                            lunchInterval.EndTime.Hour, lunchInterval.EndTime.Minute, 0, order.ScheduleTime.Kind)
        };

        return orderRelocated;
    }
}
