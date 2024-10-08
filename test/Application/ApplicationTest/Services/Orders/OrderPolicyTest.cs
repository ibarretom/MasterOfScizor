﻿using Application.Services.Orders;
using Bogus;
using Domain.Entities;
using Domain.Entities.Barbers;
using Domain.Entities.Barbers.Service;
using Domain.Entities.Orders;
using Domain.Exceptions;
using Domain.Exceptions.Messages;
using DomainTest.Entities;
using DomainTest.Entities.Barbers;
using DomainTest.Entities.Barbers.Services;
using DomainTest.Entities.Orders;
using System;
using Domain.Services.Barbers;
using Moq;

namespace ApplicationTest.Services.Orders;

public class OrderPolicyTest
{
    [Fact]
    public void ShouldThrowWhenAOrderIsNotOnWorkingDay()
    {
        var configuration = ConfigurationBuilder.BuildRandom();
        var branch = BranchBuilder.Build(configuration);

        var now = DateTime.UtcNow;

        var schedule = new Schedule(new TimeOnly(now.Hour, now.Minute), new TimeOnly(now.Hour, now.Minute).AddHours(2), now.DayOfWeek);
        branch.AddSchedule(schedule);

        var order = OrderBuilder.Build(now.AddDays(2), branch);

        var scheduler = new Mock<IScheduler>();

        var orderPolicy = new OrderPolicy(scheduler.Object);

        var isAllowed = orderPolicy.IsAllowed(order, new List<Order>(), out string reason);

        Assert.False(isAllowed);
        Assert.True(reason.Equals(OrderExceptionResourceMessages.ORDER_TIME_IS_NOT_IN_COMMERCIAL_TIME));
    }

    private static void AddScheduleWithBoundarySchedules(DateTime time, Branch branch)
    {
        var schedule = new Schedule(new TimeOnly(time.Hour, time.Minute).AddHours(-1), new TimeOnly(time.Hour, time.Minute).AddHours(2), time.AddHours(-1).DayOfWeek);
        branch.AddSchedule(schedule);

        var scheduleDayBefore = new Schedule(new TimeOnly(time.AddDays(-1).Hour, time.AddDays(-1).Minute), new TimeOnly(time.AddDays(-1).Hour, time.AddDays(-1).Minute).AddHours(2), time.AddDays(-1).DayOfWeek);
        branch.AddSchedule(scheduleDayBefore);

        var scheduleDayAfter = new Schedule(new TimeOnly(time.AddDays(1).Hour, time.AddDays(1).Minute), new TimeOnly(time.AddDays(1).Hour, time.AddDays(1).Minute).AddHours(2), time.AddDays(1).DayOfWeek);
        branch.AddSchedule(scheduleDayAfter);
    }

    [Fact]
    public void ShouldThrowWhenAOrderIsNotInCommercialTime()
    {
        var configuration = ConfigurationBuilder.BuildRandom();
        var branch = BranchBuilder.Build(configuration, true);

        var now = DateTime.UtcNow;

        AddScheduleWithBoundarySchedules(now, branch);

        var order = OrderBuilder.Build(now.AddHours(3), branch);

        var scheduler = new Mock<IScheduler>();

        var orderPolicy = new OrderPolicy(scheduler.Object);

        var isAllowed1 = orderPolicy.IsAllowed(order, new List<Order>(), out var reason);

        Assert.False(isAllowed1);
        Assert.True(reason.Equals(OrderExceptionResourceMessages.ORDER_TIME_IS_NOT_IN_COMMERCIAL_TIME), "Mensagem divergente. Message: " + isAllowed1);

        order = OrderBuilder.Build(now.AddHours(-2), branch);

        var isAllowed = orderPolicy.IsAllowed(order, new List<Order>(), out reason);

        Assert.True(!isAllowed);
        Assert.True(reason.Equals(OrderExceptionResourceMessages.ORDER_TIME_IS_NOT_IN_COMMERCIAL_TIME));
    }

    [Fact]
    public void ShouldThrowWhenBranchClosedAndConfigurationIsQueue()
    {
        var configuration = ConfigurationBuilder.BuildQueueWithVirtualQueue();
        var branch = BranchBuilder.Build(configuration);

        var now = DateTime.UtcNow;

        AddScheduleWithBoundarySchedules(now, branch);

        var employee = EmployeeBuilder.Build();
        branch.AddEmployee(employee);

        var order = OrderBuilder.Build(now, branch, employee);

        var scheduler = new Mock<IScheduler>();

        var orderPolicy = new OrderPolicy(scheduler.Object);

        var exception = Assert.Throws<CompanyException>(() => orderPolicy.IsAllowed(order, new List<Order>(), out var reason));

        Assert.True(exception.Message.Equals(CompanyExceptionMessagesResource.BRANCH_IS_NOT_OPENNED_THIS_DAY));
    }

    [Fact]
    public void ShouldRejectAOrderWithTimeAndServicesGraterThanClosingTimeVirtualQueueConfiguration()
    {
        var configuration = ConfigurationBuilder.BuildQueueWithVirtualQueue();
        var branch = BranchBuilder.Build(configuration, true);

        var now = DateTime.UtcNow;

        AddScheduleWithBoundarySchedules(now, branch);

        var employee = EmployeeBuilder.Build();
        branch.AddEmployee(employee);

        var order = OrderBuilder.Build(now.AddHours(1.5), branch, employee, new List<Service>() { ServiceBuilder.Build(TimeSpan.FromMinutes(40)) });

        var scheduler = new Mock<IScheduler>();

        var orderPolicy = new OrderPolicy(scheduler.Object);

        var isAllowed = orderPolicy.IsAllowed(order, new List<Order>(), out var reason);

        Assert.True(!isAllowed);
        Assert.True(reason.Equals(OrderExceptionResourceMessages.ORDER_TIME_IS_GREATER_THEN_COMMERCIAL_TIME));
        var orders = new List<Order>() {
            OrderBuilder.Build(now.AddHours(1), branch, employee,
                new List<Service>() {
                ServiceBuilder.Build(TimeSpan.FromMinutes(40))
                }
            )
        };

        isAllowed = orderPolicy.IsAllowed(order, orders, out reason);

        Assert.True(!isAllowed);
        Assert.True(reason.Equals(OrderExceptionResourceMessages.ORDER_TIME_IS_GREATER_THEN_COMMERCIAL_TIME));
    }

    [Fact]
    public void ShouldThrowWhenOrderTimeIsBeforeTimeNowOnQueueConfiguration()
    {
        var configuration = ConfigurationBuilder.BuildQueueWithVirtualQueue();
        var branch = BranchBuilder.Build(configuration, true);

        var now = DateTime.UtcNow;

        AddScheduleWithBoundarySchedules(now, branch);

        var employee = EmployeeBuilder.Build();

        branch.AddEmployee(employee);

        var order = OrderBuilder.Build(now.AddHours(-1), branch, employee);

        var scheduler = new Mock<IScheduler>();

        var orderPolicy = new OrderPolicy(scheduler.Object);

        var isAllowed = orderPolicy.IsAllowed(order, new List<Order>(), out var reason);

        Assert.False(isAllowed);
        Assert.True(reason.Equals(OrderExceptionResourceMessages.ORDER_TIME_IS_BEFORE_DATE_NOW));
    }

    [Fact]
    public void ShouldRejectWhenOrderQueueLimitIsFull()
    {
        var configuration = ConfigurationBuilder.BuildWithQueueLimit(1);
        var branch = BranchBuilder.Build(configuration, true);

        var now = DateTime.UtcNow;

        AddScheduleWithBoundarySchedules(now, branch);

        var employee = EmployeeBuilder.Build();

        branch.AddEmployee(employee);

        var order = OrderBuilder.Build(now, branch, employee);

        var scheduler = new Mock<IScheduler>();

        var orderPolicy = new OrderPolicy(scheduler.Object);

        var isAllowed = orderPolicy.IsAllowed(order, new List<Order>() { OrderBuilder.Build(now, branch) }, out var reason);

        Assert.True(!isAllowed);
        Assert.True(reason.Equals(OrderExceptionResourceMessages.ORDER_QUEUE_IS_FULL));
    }

    [Fact]
    public void ShouldAllowWhenOrderQueueLimitIsNotFull()
    {
        var configuration = ConfigurationBuilder.BuildWithQueueLimit(2);

        var branch = BranchBuilder.Build(configuration, true);

        var now = DateTime.Now.ToUniversalTime();

        AddScheduleWithBoundarySchedules(now, branch);

        var employee = EmployeeBuilder.Build();

        branch.AddEmployee(employee);

        var order = OrderBuilder.Build(now, branch, employee);

        var scheduler = new Mock<IScheduler>();

        var orderPolicy = new OrderPolicy(scheduler.Object);

        var isAllowed = orderPolicy.IsAllowed(order, new List<Order>() { OrderBuilder.Build() }, out _);

        Assert.True(isAllowed);
    }

    [Fact]
    public void ShouldThrowWhenOrderIsNotInAutoGenerateScheduleTimes()
    {
        var configuration = ConfigurationBuilder.BuildWithScheduleWithNoDelay();
        var branch = BranchBuilder.Build(configuration, true);

        var now = DateTime.UtcNow;

        AddScheduleWithBoundarySchedules(now, branch);

        var employee = EmployeeBuilder.Build();

        branch.AddEmployee(employee);

        var order = OrderBuilder.Build(now.AddMinutes(40), branch, employee);

        var scheduler = new Mock<IScheduler>();

        var orderPolicy = new OrderPolicy(scheduler.Object);

        var isAllowed = orderPolicy.IsAllowed(order, new List<Order>(), out var reason);

        Assert.False(isAllowed);
        Assert.True(reason.Equals(OrderExceptionResourceMessages.ORDER_TIME_NOT_ALLOWED));
    }

    [Fact]
    public void ShouldThrowWhenOrderIsInLunchInterval()
    {
        var configuration = ConfigurationBuilder.BuildWithScheduleWithDelay();
        var branch = BranchBuilder.Build(configuration, true);

        var now = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day,
                               23, 23, 32, DateTimeKind.Utc); ;

        AddScheduleWithBoundarySchedules(now, branch);

        var employee = EmployeeBuilder.Build();
        branch.AddEmployee(employee);

        branch.AddEmployeeLunchInterval(new Schedule(new TimeOnly(now.Hour, now.Minute), new TimeOnly(now.Hour, now.Minute).AddHours(1), now.DayOfWeek), employee.Id);

        var order = OrderBuilder.Build(now.AddMinutes(-30), branch, employee, new List<Service>() { ServiceBuilder.Build(TimeSpan.FromMinutes(32)) });

        var scheduler = new Mock<IScheduler>();

        var orderPolicy = new OrderPolicy(scheduler.Object);

        var isAllowed = orderPolicy.IsAllowed(order, new List<Order>(), out var reason);

        Assert.False(isAllowed);
        Assert.True(reason.Equals(OrderExceptionResourceMessages.ORDER_TIME_IS_IN_LUNCH_INTERVAL));
    }

    [Fact]
    public void ShouldThrowWhenOrderServicesIsOverflowingTheLunchInterval()
    {
        var configuration = ConfigurationBuilder.BuildWithScheduleWithDelay();
        var branch = BranchBuilder.Build(configuration);

        var now = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day,
                               23, 23, 32, DateTimeKind.Utc);

        AddScheduleWithBoundarySchedules(now, branch);

        var employee = EmployeeBuilder.Build();
        branch.AddEmployee(employee);

        branch.AddEmployeeLunchInterval(new Schedule(new TimeOnly(now.Hour, now.Minute), new TimeOnly(now.Hour, now.Minute).AddHours(1), now.DayOfWeek), employee.Id);

        var order = OrderBuilder.Build(now.AddMinutes(-30), branch, employee, new List<Service>() { ServiceBuilder.Build(TimeSpan.FromMinutes(40)) });

        var scheduler = new Mock<IScheduler>();

        var orderPolicy = new OrderPolicy(scheduler.Object);

        var isAllowed = orderPolicy.IsAllowed(order, new List<Order>(), out var reason);

        Assert.False(isAllowed);
        Assert.True(reason.Equals(OrderExceptionResourceMessages.ORDER_TIME_IS_IN_LUNCH_INTERVAL));
    }

    [Fact]
    public void ShouldRejectWhenOrderIsOverflowingTheCommercialTime()
    {
        var configuration = ConfigurationBuilder.BuildWithScheduleWithNoDelay();
        var branch = BranchBuilder.Build(configuration);

        var now = DateTime.UtcNow;

        AddScheduleWithBoundarySchedules(now, branch);

        var employee = EmployeeBuilder.Build();

        branch.AddEmployee(employee);

        var order = OrderBuilder.Build(now.AddHours(1.5), branch, employee, new List<Service>() { ServiceBuilder.Build(TimeSpan.FromMinutes(40)) });

        var scheduler = new Mock<IScheduler>();

        var orderPolicy = new OrderPolicy(scheduler.Object);

        var isAllowed = orderPolicy.IsAllowed(order, new List<Order>(), out var reason);

        Assert.False(isAllowed);
        Assert.True(reason.Equals(OrderExceptionResourceMessages.ORDER_TIME_IS_GREATER_THEN_COMMERCIAL_TIME));
    }

    [Fact]
    public void ShouldRejectWhenOrderIsOverflowingNextOrderTime()
    {
        var configuration = ConfigurationBuilder.BuildWithScheduleWithNoDelay();
        var branch = BranchBuilder.Build(configuration);

        var now = DateTime.UtcNow;

        AddScheduleWithBoundarySchedules(now, branch);

        var employee = EmployeeBuilder.Build();

        branch.AddEmployee(employee);

        var order = OrderBuilder.Build(now, branch, employee, new List<Service>() { ServiceBuilder.Build(TimeSpan.FromMinutes(40)) });

        var scheduler = new Mock<IScheduler>();
        scheduler.Setup(scheduler =>
                scheduler.GetAvailable(It.IsAny<DateTime>(), It.IsAny<OrderBase>(), It.IsAny<List<Order>>()))
            .Returns(new HashSet<DateTime>());

        var orderPolicy = new OrderPolicy(scheduler.Object);

        var isAllowed = orderPolicy.IsAllowed(order, new List<Order>() { OrderBuilder.Build(now.AddHours(0.5), branch, employee) }, out var reason);

        Assert.True(!isAllowed);
        Assert.True(reason.Equals(OrderExceptionResourceMessages.ORDER_TIME_ALREADY_ALOCATED));
    }

    [Fact]
    public void ShouldRejectWhenPreviousOrderIsOverFlowingTheCurrentOrder()
    {
        var configuration = ConfigurationBuilder.BuildWithScheduleWithNoDelay();
        var branch = BranchBuilder.Build(configuration);

        var now = DateTime.UtcNow;

        AddScheduleWithBoundarySchedules(now, branch);

        var employee = EmployeeBuilder.Build();

        branch.AddEmployee(employee);

        var order = OrderBuilder.Build(now, branch, employee);

        var scheduler = new Mock<IScheduler>();
        scheduler.Setup(scheduler =>
                scheduler.GetAvailable(It.IsAny<DateTime>(), It.IsAny<OrderBase>(), It.IsAny<List<Order>>()))
            .Returns(new HashSet<DateTime>());

        var orderPolicy = new OrderPolicy(scheduler.Object);

        var orders = new List<Order>() {
                                OrderBuilder.Build(now.AddHours(-0.5),
                                branch, employee,
                                new List<Service>() { ServiceBuilder.Build(TimeSpan.FromMinutes(40)) })
                                };
        var isAllowed = orderPolicy.IsAllowed(order, orders, out var reason);

        Assert.True(!isAllowed);
        Assert.True(reason.Equals(OrderExceptionResourceMessages.ORDER_TIME_ALREADY_ALOCATED));
    }
    [Fact]
    private void ShouldPassAOrderWithCrossingDays()
    {
        var configuration = ConfigurationBuilder.BuildWithScheduleWithNoDelay();

        var branch = BranchBuilder.Build(configuration);

        var now = DateTime.UtcNow;

        var schedule = new Schedule(new TimeOnly(22, 0), new TimeOnly(3, 0), now.DayOfWeek);
        branch.AddSchedule(schedule);

        var employee = EmployeeBuilder.Build();
        branch.AddEmployee(employee);

        var services = new List<Service>() { ServiceBuilder.Build(TimeSpan.FromMinutes(120)) };

        var order = OrderBuilder.Build(new DateTime(now.Year, now.AddDays(1).Month, now.AddDays(1).Day, 1, 0, 0), branch, employee, services);

        var scheduler = new Mock<IScheduler>();
        scheduler.Setup(scheduler =>
                scheduler.GetAvailable(It.IsAny<DateTime>(), It.IsAny<OrderBase>(), It.IsAny<List<Order>>()))
            .Returns(new HashSet<DateTime>() { order.RelocatedSchedule });

        var orderPolicy = new OrderPolicy(scheduler.Object);

        var isAllowed = orderPolicy.IsAllowed(order, new List<Order>(), out _);

        Assert.True(isAllowed);
    }

    [Fact]
    private void ShouldNotAcceptWhenTheScheduleIsFull()
    {
        var configuration = ConfigurationBuilder.BuildWithScheduleWithNoDelay();

        var branch = BranchBuilder.Build(configuration);

        var now = DateTime.UtcNow;

        var schedule = new Schedule(new TimeOnly(now.AddHours(-1).Hour, 0), new TimeOnly(now.AddHours(1).Hour, 0), now.AddHours(-1).DayOfWeek);
        branch.AddSchedule(schedule);

        var employee = EmployeeBuilder.Build();
        branch.AddEmployee(employee);

        var order = OrderBuilder.Build(new DateTime(now.Year, now.Month, now.Day, now.Hour, 0, 0), branch, employee,
                           new List<Service> { ServiceBuilder.Build(TimeSpan.FromMinutes(30)) });

        var allOrders = new List<Order>()
        {
            OrderBuilder.Build(new DateTime(now.Year, now.Month, now.AddHours(-1).Day, now.AddHours(-1).Hour, 0, 0), branch, employee,
                           new List<Service>() { ServiceBuilder.Build(TimeSpan.FromMinutes(2*60)) }),
        };

        var scheduler = new Mock<IScheduler>();
        scheduler.Setup(scheduler =>
                scheduler.GetAvailable(It.IsAny<DateTime>(), It.IsAny<OrderBase>(), It.IsAny<List<Order>>()))
            .Returns(new HashSet<DateTime>());

        var orderPolicy = new OrderPolicy(scheduler.Object);

        var isAllowed = orderPolicy.IsAllowed(order, allOrders, out _);

        Assert.False(isAllowed);
    }

    [Fact]
    public void ShouldRejectWhenTheOrderIsScheduleToTheStartTimeOfAnotherOrder()
    {
        var configuration = ConfigurationBuilder.BuildWithScheduleWithNoDelay();

        var branch = BranchBuilder.Build(configuration);

        var now = DateTime.UtcNow;

        var schedule = new Schedule(new TimeOnly(now.AddHours(-1).Hour, 0), new TimeOnly(now.AddHours(1).Hour, 0), now.AddHours(-1).DayOfWeek);
        branch.AddSchedule(schedule);

        var employee = EmployeeBuilder.Build();
        branch.AddEmployee(employee);

        var order = OrderBuilder.Build(new DateTime(now.Year, now.Month, now.Day, now.Hour, 0, 0), branch, employee,
                                      new List<Service> { ServiceBuilder.Build(TimeSpan.FromMinutes(0)) });

        var allOrders = new List<Order>() { OrderBuilder.Build(new DateTime(now.Year, now.Month, now.Day, now.Hour, 0, 0), branch, employee, new List<Service> { ServiceBuilder.Build(TimeSpan.FromMinutes(30)) }), };

        var scheduler = new Mock<IScheduler>();
        scheduler.Setup(scheduler =>
                scheduler.GetAvailable(It.IsAny<DateTime>(), It.IsAny<OrderBase>(), It.IsAny<List<Order>>()))
            .Returns(new HashSet<DateTime>());

        var orderPolicy = new OrderPolicy(scheduler.Object);

        var isAllowed = orderPolicy.IsAllowed(order, allOrders, out var reason);

        Assert.False(isAllowed);
        Assert.Equal(reason, OrderExceptionResourceMessages.ORDER_TIME_ALREADY_ALOCATED);
    }

    [Theory]
    [MemberData(nameof(BuildObjects))]
    private void ShouldAcceptThisOrders(OrderPolicyTestData testOrders)
    {
        var scheduler = new Mock<IScheduler>();
        scheduler.Setup(scheduler =>
                scheduler.GetAvailable(It.IsAny<DateTime>(), It.IsAny<OrderBase>(), It.IsAny<List<Order>>()))
            .Returns(new HashSet<DateTime>() { testOrders.Order.RelocatedSchedule });

        var orderPolicy = new OrderPolicy(scheduler.Object);

        var isAllowed = orderPolicy.IsAllowed(testOrders.Order, testOrders.Orders, out var reason);

        Assert.True(isAllowed, $"{testOrders.TestId}-{reason}");
    }


    private static readonly DateTime Now = DateTime.UtcNow;

    public static IEnumerable<object[]> BuildObjects()
    {
        yield return new object[]
        {
                QueueOrderWithPreviousOrderAlmostClosing()
        };

        yield return new object[]
        {
                QueueOrderWithNoPreviousOrderAlmostClosing(),
        };

        yield return new object[]
        {
                QueueLimitOrderWithFreeSpace(),
        };

        yield return new object[]
        {
                QueueScheduleCloseToLunchInterval(),
        };

        yield return new object[]
        {
                QueueScheduleCloseToEndTime(),
        };

        yield return new object[]
        {
                QueueScheduleCloseToNextOrder(),
        };

        yield return new object[]
        {
                QueueScheduleCloseToPreviousOrder(),
        };

        yield return new object[]
        {
                QueueScheduleWithUpperAndBellowBoundary(),
        };

        yield return new object[]
        {
            QueueScheduleWithFullScheduleTodayButFreeAWeekAfter(),
        };

        yield return new object[]
        {
            QueueScheduleWithOrderForTomorrow(),
        };
    }

    private static OrderPolicyTestData QueueOrderWithPreviousOrderAlmostClosing()
    {

        var configuration = ConfigurationBuilder.BuildQueueWithVirtualQueue();

        var branch = BranchBuilder.Build(configuration, true);

        AddScheduleWithBoundarySchedules(Now, branch);

        var employee = EmployeeBuilder.Build();
        branch.AddEmployee(employee);

        var order = OrderBuilder.Build(Now.AddHours(1), branch, employee, new List<Service> { ServiceBuilder.Build(TimeSpan.FromMinutes(60)) });

        var allOrders = new List<Order>()
            {
                OrderBuilder.Build(Now, branch, employee, new List<Service> { ServiceBuilder.Build(TimeSpan.FromMinutes(60)) })
            };

        return new OrderPolicyTestData(order, allOrders, "QueueOrderWithPreviousOrderAlmostClosing");
    }

    private static OrderPolicyTestData QueueOrderWithNoPreviousOrderAlmostClosing()
    {
        var configuration = ConfigurationBuilder.BuildQueueWithVirtualQueue();

        var branch = BranchBuilder.Build(configuration, true);
        AddScheduleWithBoundarySchedules(Now, branch);

        var employee = EmployeeBuilder.Build();
        branch.AddEmployee(employee);

        var order = OrderBuilder.Build(Now.AddHours(1), branch, employee, new List<Service> { ServiceBuilder.Build(TimeSpan.FromMinutes(60)) });

        var allOrders = new List<Order>();

        return new OrderPolicyTestData(order, allOrders, "QueueOrderWithNoPreviousOrderAlmostClosing");
    }

    private static OrderPolicyTestData QueueLimitOrderWithFreeSpace()
    {
        var configuration = ConfigurationBuilder.BuildWithQueueLimit(2);

        var branch = BranchBuilder.Build(configuration, true);
        AddScheduleWithBoundarySchedules(Now, branch);

        var employee = EmployeeBuilder.Build();
        branch.AddEmployee(employee);

        var order = OrderBuilder.Build(Now.AddHours(1), branch, employee, new List<Service> { ServiceBuilder.Build(TimeSpan.FromMinutes(60)) });

        var allOrders = new List<Order>();

        return new OrderPolicyTestData(order, allOrders, "QueueLimitOrderWithFreeSpace");
    }

    private static OrderPolicyTestData QueueScheduleCloseToLunchInterval()
    {
        var configuration = ConfigurationBuilder.BuildWithScheduleWithNoDelay();

        var branch = BranchBuilder.Build(configuration, true);
        AddScheduleWithBoundarySchedules(Now, branch);

        var employee = EmployeeBuilder.Build();
        branch.AddEmployee(employee);

        branch.AddEmployeeLunchInterval(new Schedule(new TimeOnly(Now.Hour, Now.Minute).AddHours(1),
            new TimeOnly(Now.Hour, Now.Minute).AddHours(2), Now.DayOfWeek), employee.Id);

        var order = OrderBuilder.Build(Now, branch, employee, new List<Service> { ServiceBuilder.Build(TimeSpan.FromMinutes(60)) });

        var allOrders = new List<Order>();

        return new OrderPolicyTestData(order, allOrders, "QueueScheduleCloseToLunchInterval");
    }

    private static OrderPolicyTestData QueueScheduleCloseToEndTime()
    {
        var configuration = ConfigurationBuilder.BuildWithScheduleWithNoDelay();

        var branch = BranchBuilder.Build(configuration, true);
        AddScheduleWithBoundarySchedules(Now, branch);

        var employee = EmployeeBuilder.Build();
        branch.AddEmployee(employee);

        var order = OrderBuilder.Build(Now.AddHours(1), branch, employee, new List<Service> { ServiceBuilder.Build(TimeSpan.FromMinutes(60)) });

        var allOrders = new List<Order>();


        return new OrderPolicyTestData(order, allOrders, "QueueScheduleCloseToEndTime");
    }

    private static OrderPolicyTestData QueueScheduleCloseToNextOrder()
    {
        var configuration = ConfigurationBuilder.BuildWithScheduleWithNoDelay();

        var branch = BranchBuilder.Build(configuration, true);
        AddScheduleWithBoundarySchedules(Now, branch);

        var employee = EmployeeBuilder.Build();
        branch.AddEmployee(employee);

        var order = OrderBuilder.Build(Now, branch, employee, new List<Service> { ServiceBuilder.Build(TimeSpan.FromMinutes(30)) });

        var allOrders = new List<Order>() { OrderBuilder.Build(Now.AddMinutes(30), branch, employee) };

        return new OrderPolicyTestData(order, allOrders, "QueueScheduleCloseToNextOrder");
    }

    private static OrderPolicyTestData QueueScheduleCloseToPreviousOrder()
    {
        var configuration = ConfigurationBuilder.BuildWithScheduleWithNoDelay();

        var branch = BranchBuilder.Build(configuration, true);
        AddScheduleWithBoundarySchedules(Now, branch);

        var employee = EmployeeBuilder.Build();
        branch.AddEmployee(employee);

        var order = OrderBuilder.Build(Now, branch, employee,
                new List<Service> { ServiceBuilder.Build(TimeSpan.FromMinutes(30)) });

        var allOrders = new List<Order>() {
                OrderBuilder.Build(Now.AddMinutes(-30), branch, employee, new List<Service>() { ServiceBuilder.Build(TimeSpan.FromMinutes(30))})
            };

        return new OrderPolicyTestData(order, allOrders, "QueueScheduleCloseToPreviousOrder");
    }

    private static OrderPolicyTestData QueueScheduleWithUpperAndBellowBoundary()
    {
        var configuration = ConfigurationBuilder.BuildWithScheduleWithNoDelay();

        var branch = BranchBuilder.Build(configuration, true);
        AddScheduleWithBoundarySchedules(Now, branch);

        var employee = EmployeeBuilder.Build();
        branch.AddEmployee(employee);

        var order = OrderBuilder.Build(Now, branch, employee,
                new List<Service> { ServiceBuilder.Build(TimeSpan.FromMinutes(30)) });

        var allOrders = new List<Order>() {
                OrderBuilder.Build(
                    Now.AddMinutes(-30), branch, employee,
                    new List<Service>() { ServiceBuilder.Build(TimeSpan.FromMinutes(30))}
                ),
                OrderBuilder.Build(Now.AddMinutes(30), branch)
            };

        return new OrderPolicyTestData(order, allOrders, "QueueScheduleWithUpperAndBellowBoundary");
    }

    private static OrderPolicyTestData QueueScheduleWithFullScheduleTodayButFreeAWeekAfter()
    {
        var configuration = ConfigurationBuilder.BuildWithScheduleWithNoDelay();
        var branch = BranchBuilder.Build(configuration, true);
        AddScheduleWithBoundarySchedules(Now, branch);

        var employee = EmployeeBuilder.Build();
        branch.AddEmployee(employee);

        var order = OrderBuilder.Build(Now.AddDays(7), branch, employee,
                           new List<Service> { ServiceBuilder.Build(TimeSpan.FromMinutes(30)) });

        var allOrders = new List<Order>()
        {
            OrderBuilder.Build(Now.AddHours(-1), branch, employee,
                           new List<Service>() { ServiceBuilder.Build(TimeSpan.FromMinutes(3*60)) }),
        };

        return new OrderPolicyTestData(order, allOrders, "QueueScheduleWithFullScheduleTodayButFreeAWeekAfter");
    }

    private static OrderPolicyTestData QueueScheduleWithOrderForTomorrow()
    {
        var configuration = ConfigurationBuilder.BuildWithScheduleWithNoDelay();
        var branch = BranchBuilder.Build(configuration, true);
        AddScheduleWithBoundarySchedules(Now, branch);

        var employee = EmployeeBuilder.Build();
        branch.AddEmployee(employee);

        var order = OrderBuilder.Build(Now.AddDays(1), branch, employee,
                           new List<Service> { ServiceBuilder.Build(TimeSpan.FromMinutes(30)) });

        var allOrders = new List<Order>()
        {
        };


        return new OrderPolicyTestData(order, allOrders, "QueueScheduleWithOrderForTomorrow");
    }
    private class OrderPolicyTestData
    {
        public Order Order { get; set; }
        public List<Order> Orders { get; set; }
        public string TestId { get; set; }

        public OrderPolicyTestData(Order order, List<Order> orders, string testId)
        {
            Order = order;
            Orders = orders;
            TestId = testId;
        }
    }
}
