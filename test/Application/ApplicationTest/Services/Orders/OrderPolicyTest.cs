using Application.Services.Orders;
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

        var order = OrderBuilder.Build(now.AddDays(2));

        var orderPolicy = new OrderPolicy();

        var exception = Assert.Throws<OrderException>(() => orderPolicy.IsAllowed(order, new List<Order>(), branch));

        Assert.True(exception.Message.Equals(OrderExceptionResourceMessages.ORDER_TIME_IS_NOT_IN_COMMERCIAL_TIME));
    }

    private static void AddScheduleWithBoundarySchedules(DateTime time, Branch branch)
    {
        var schedule = new Schedule(new TimeOnly(time.Hour, time.Minute).AddHours(-1), new TimeOnly(time.Hour, time.Minute).AddHours(2), time.AddHours(-1).DayOfWeek);
        branch.AddSchedule(schedule);

        var scheduleDayBefore = new Schedule(new TimeOnly(time.AddDays(-1).Hour, time.AddDays(-1).Minute), new TimeOnly(time.AddDays(-1).Hour, time.AddDays(-1).Minute).AddHours(2), time.AddDays(-1).AddHours(-1).DayOfWeek);
        branch.AddSchedule(scheduleDayBefore);

        var scheduleDayAfter = new Schedule(new TimeOnly(time.AddDays(1).Hour, time.AddDays(1).Minute), new TimeOnly(time.AddDays(1).Hour, time.AddDays(1).Minute).AddHours(2), time.AddDays(1).AddHours(-1).DayOfWeek);
        branch.AddSchedule(scheduleDayAfter);
    }

    [Fact]
    public void ShouldThrowWhenAOrderIsNotInCommercialTime()
    {
        var configuration = ConfigurationBuilder.BuildRandom();
        var branch = BranchBuilder.Build(configuration, true);

        var now = DateTime.UtcNow;

        AddScheduleWithBoundarySchedules(now, branch);

        var order = OrderBuilder.Build(now.AddHours(3));

        var orderPolicy = new OrderPolicy();

        var exceptionGratherThan = Assert.Throws<OrderException>(() => orderPolicy.IsAllowed(order, new List<Order>(), branch));

        Assert.True(exceptionGratherThan.Message.Equals(OrderExceptionResourceMessages.ORDER_TIME_IS_NOT_IN_COMMERCIAL_TIME), "Mensagem divergente. Message: " + exceptionGratherThan.Message);

        order = OrderBuilder.Build(now.AddHours(-2));

        var exceptionLessThan = Assert.Throws<OrderException>(() => orderPolicy.IsAllowed(order, new List<Order>(), branch));

        Assert.True(exceptionLessThan.Message.Equals(OrderExceptionResourceMessages.ORDER_TIME_IS_NOT_IN_COMMERCIAL_TIME));
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

        var order = OrderBuilder.Build(now, employee.Id);

        var orderPolicy = new OrderPolicy();

        var exception = Assert.Throws<CompanyException>(() => orderPolicy.IsAllowed(order, new List<Order>(), branch));

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

        var order = OrderBuilder.Build(now.AddHours(1.5), employee.Id, new List<Service>() { ServiceBuilder.Build(branch.Id, TimeSpan.FromMinutes(40)) });

        var orderPolicy = new OrderPolicy();

        var isAllowed = orderPolicy.IsAllowed(order, new List<Order>(), branch);

        Assert.True(!isAllowed);

        var orders = new List<Order>() {
            OrderBuilder.Build(now.AddHours(1), employee.Id,
                new List<Service>() {
                ServiceBuilder.Build(branch.Id, TimeSpan.FromMinutes(40))
                }
            )
        };

        isAllowed = orderPolicy.IsAllowed(order, orders, branch);

        Assert.True(!isAllowed);
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

        var order = OrderBuilder.Build(now.AddHours(-1), employee.Id);

        var orderPolicy = new OrderPolicy();

        var exception = Assert.Throws<OrderException>(() => orderPolicy.IsAllowed(order, new List<Order>(), branch));


        Assert.True(exception.Message.Equals(OrderExceptionResourceMessages.ORDER_TIME_IS_BEFORE_DATE_NOW));
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

        var order = OrderBuilder.Build(now, employee.Id);

        var orderPolicy = new OrderPolicy();

        var isAllowed = orderPolicy.IsAllowed(order, new List<Order>() { OrderBuilder.Build(now) }, branch);

        Assert.True(!isAllowed);
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

        var order = OrderBuilder.Build(now, employee.Id);

        var orderPolicy = new OrderPolicy();

        var isAllowed = orderPolicy.IsAllowed(order, new List<Order>() { OrderBuilder.Build() }, branch);

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

        var order = OrderBuilder.Build(now.AddMinutes(40), employee.Id);

        var orderPolicy = new OrderPolicy();

        var exception = Assert.Throws<OrderException>(() => orderPolicy.IsAllowed(order, new List<Order>(), branch));

        Assert.True(exception.Message.Equals(OrderExceptionResourceMessages.ORDER_TIME_NOT_ALLOWED));
    }

    [Fact]
    public void ShouldThrowWhenOrderIsInLunchInterval()
    {
        var configuration = ConfigurationBuilder.BuildWithScheduleWithDelay();
        var branch = BranchBuilder.Build(configuration, true);

        var now = DateTime.UtcNow;

        AddScheduleWithBoundarySchedules(now, branch);

        var employee = EmployeeBuilder.Build();
        employee.LunchInterval = new Schedule(new TimeOnly(now.Hour, now.Minute), new TimeOnly(now.Hour, now.Minute).AddHours(1), now.DayOfWeek);
        branch.AddEmployee(employee);

        var order = OrderBuilder.Build(now.AddMinutes(-30), employee.Id, new List<Service>() { ServiceBuilder.Build(branch.Id, TimeSpan.FromMinutes(32)) });

        var orderPolicy = new OrderPolicy();

        var exception = Assert.Throws<OrderException>(() => orderPolicy.IsAllowed(order, new List<Order>(), branch));

        Assert.True(exception.Message.Equals(OrderExceptionResourceMessages.ORDER_TIME_IS_IN_LUNCH_INTERVAL));
    }

    [Fact]
    public void ShouldThrowWhenOrderServicesIsOverflowingTheLunchInterval()
    {
        var configuration = ConfigurationBuilder.BuildWithScheduleWithDelay();
        var branch = BranchBuilder.Build(configuration);

        var now = DateTime.UtcNow;

        AddScheduleWithBoundarySchedules(now, branch);

        var employee = EmployeeBuilder.Build();
        employee.LunchInterval = new Schedule(new TimeOnly(now.Hour, now.Minute), new TimeOnly(now.Hour, now.Minute).AddHours(1), now.DayOfWeek);

        branch.AddEmployee(employee);

        var order = OrderBuilder.Build(now.AddMinutes(-30), employee.Id, new List<Service>() { ServiceBuilder.Build(branch.Id, TimeSpan.FromMinutes(40)) });

        var orderPolicy = new OrderPolicy();

        var exception = Assert.Throws<OrderException>(() => orderPolicy.IsAllowed(order, new List<Order>(), branch));

        Assert.True(exception.Message.Equals(OrderExceptionResourceMessages.ORDER_TIME_IS_IN_LUNCH_INTERVAL));
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

        var order = OrderBuilder.Build(now.AddHours(1.5), employee.Id, new List<Service>() { ServiceBuilder.Build(branch.Id, TimeSpan.FromMinutes(40)) });

        var orderPolicy = new OrderPolicy();

        var isAllowed = orderPolicy.IsAllowed(order, new List<Order>(), branch);

        Assert.True(!isAllowed);
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

        var order = OrderBuilder.Build(now, employee.Id, new List<Service>() { ServiceBuilder.Build(branch.Id, TimeSpan.FromMinutes(40)) });

        var orderPolicy = new OrderPolicy();

        var isAllowed = orderPolicy.IsAllowed(order, new List<Order>() { OrderBuilder.Build(now.AddHours(0.5)) }, branch);

        Assert.True(!isAllowed);
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

        var order = OrderBuilder.Build(now, employee.Id);

        var orderPolicy = new OrderPolicy();

        var isAllowed = orderPolicy.IsAllowed(order, new List<Order>() { OrderBuilder.Build(now.AddHours(-0.5), employee.Id, new List<Service>() { ServiceBuilder.Build(branch.Id, TimeSpan.FromMinutes(40)) }) }, branch);

        Assert.True(!isAllowed);
    }
    [Fact]
    private void ShouldPassAOrderWithCrossingDays()
    {
        var configuration = ConfigurationBuilder.BuildWithScheduleWithNoDelay();

        var branch = BranchBuilder.Build(configuration);

        var now = DateTime.UtcNow;

        var schedule = new Schedule(new TimeOnly(22, 0), new TimeOnly(2, 0), now.DayOfWeek);
        branch.AddSchedule(schedule);

        var employee = EmployeeBuilder.Build();
        branch.AddEmployee(employee);

        var services = new List<Service>() { ServiceBuilder.Build(branch.Id, TimeSpan.FromMinutes(120)) };

        var order = OrderBuilder.Build(new DateTime(now.Year, now.Month, now.Day, 23, 0, 0), employee.Id, services);

        var orderPolicy = new OrderPolicy();

        var isAllowed = orderPolicy.IsAllowed(order, new List<Order>(), branch);

        Assert.True(isAllowed);
    }

    [Fact]
    private void ShouldNotAcceptWhenTheScheduleIsFull()
    {
        var configuration = ConfigurationBuilder.BuildWithScheduleWithNoDelay();

        var branch = BranchBuilder.Build(configuration);

        var now = DateTime.UtcNow;
        
        var schedule = new Schedule(new TimeOnly(now.AddHours(-1).Hour, 0), new TimeOnly(now.AddHours(1).Hour, 0), now.DayOfWeek);
        branch.AddSchedule(schedule);

        var employee = EmployeeBuilder.Build();
        branch.AddEmployee(employee);

        var order = OrderBuilder.Build(new DateTime(now.Year, now.Month, now.Day, now.Hour, 0, 0), employee.Id,
                           new List<Service> { ServiceBuilder.Build(Guid.NewGuid(), TimeSpan.FromMinutes(30)) });

        var allOrders = new List<Order>()
        {
            OrderBuilder.Build(new DateTime(now.Year, now.Month, now.Day, now.AddHours(-1).Hour, 0, 0), employee.Id,
                           new List<Service>() { ServiceBuilder.Build(Guid.NewGuid(), TimeSpan.FromMinutes(2*60)) }),
        };

        var orderPolicy = new OrderPolicy();

        var isAllowed = orderPolicy.IsAllowed(order, allOrders, branch);

        Assert.False(isAllowed);
    }

    [Theory]
    [MemberData(nameof(BuildObjects))]
    private void ShouldAcceptThisOrders(OrderPolicyTestData testOrders)
    {
        var branch = BranchBuilder.Build(testOrders.Configuration, true);

        AddScheduleWithBoundarySchedules(Now, branch);

        Employee.LunchInterval = testOrders.LunchInterval;

        branch.AddEmployee(Employee);

        var orderPolicy = new OrderPolicy();

        var isAllowed = orderPolicy.IsAllowed(testOrders.Order, testOrders.Orders, branch);

        Assert.True(isAllowed, testOrders.TestId);
    }


    private static readonly DateTime Now = DateTime.UtcNow;
    private static readonly Employee Employee = EmployeeBuilder.Build();

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
        var order = OrderBuilder.Build(Now.AddHours(1), Employee.Id, new List<Service> { ServiceBuilder.Build(Guid.NewGuid(), TimeSpan.FromMinutes(60)) });

        var allOrders = new List<Order>()
            {
                OrderBuilder.Build(Now, Employee.Id, new List<Service> { ServiceBuilder.Build(Guid.NewGuid(), TimeSpan.FromMinutes(60)) })
            };

        var configuration = ConfigurationBuilder.BuildQueueWithVirtualQueue();

        return new OrderPolicyTestData(order, allOrders, configuration, "QueueOrderWithPreviousOrderAlmostClosing");
    }

    private static OrderPolicyTestData QueueOrderWithNoPreviousOrderAlmostClosing()
    {
        var order = OrderBuilder.Build(Now.AddHours(1), Employee.Id, new List<Service> { ServiceBuilder.Build(Guid.NewGuid(), TimeSpan.FromMinutes(60)) });

        var allOrders = new List<Order>();

        var configuration = ConfigurationBuilder.BuildQueueWithVirtualQueue();

        return new OrderPolicyTestData(order, allOrders, configuration, "QueueOrderWithNoPreviousOrderAlmostClosing");
    }

    private static OrderPolicyTestData QueueLimitOrderWithFreeSpace()
    {
        var order = OrderBuilder.Build(Now.AddHours(1), Employee.Id, new List<Service> { ServiceBuilder.Build(Guid.NewGuid(), TimeSpan.FromMinutes(60)) });

        var allOrders = new List<Order>();

        var configuration = ConfigurationBuilder.BuildWithQueueLimit(2);

        return new OrderPolicyTestData(order, allOrders, configuration, "QueueLimitOrderWithFreeSpace");
    }

    private static OrderPolicyTestData QueueScheduleCloseToLunchInterval()
    {
        var order = OrderBuilder.Build(Now, Employee.Id, new List<Service> { ServiceBuilder.Build(Guid.NewGuid(), TimeSpan.FromMinutes(60)) });

        var allOrders = new List<Order>();

        var configuration = ConfigurationBuilder.BuildWithScheduleWithNoDelay();

        return new OrderPolicyTestData(order, allOrders, configuration,
            new Schedule(new TimeOnly(Now.Hour, Now.Minute).AddHours(1),
            new TimeOnly(Now.Hour, Now.Minute).AddHours(2), Now.DayOfWeek),
            "QueueScheduleCloseToLunchInterval");
    }

    private static OrderPolicyTestData QueueScheduleCloseToEndTime()
    {
        var order = OrderBuilder.Build(Now.AddHours(1), Employee.Id, new List<Service> { ServiceBuilder.Build(Guid.NewGuid(), TimeSpan.FromMinutes(60)) });

        var allOrders = new List<Order>();

        var configuration = ConfigurationBuilder.BuildWithScheduleWithNoDelay();

        return new OrderPolicyTestData(order, allOrders, configuration, "QueueScheduleCloseToEndTime");
    }

    private static OrderPolicyTestData QueueScheduleCloseToNextOrder()
    {
        var order = OrderBuilder.Build(Now, Employee.Id, new List<Service> { ServiceBuilder.Build(Guid.NewGuid(), TimeSpan.FromMinutes(30)) });

        var allOrders = new List<Order>() { OrderBuilder.Build(Now.AddMinutes(30), Employee.Id) };

        var configuration = ConfigurationBuilder.BuildWithScheduleWithNoDelay();

        return new OrderPolicyTestData(order, allOrders, configuration, "QueueScheduleCloseToNextOrder");
    }

    private static OrderPolicyTestData QueueScheduleCloseToPreviousOrder()
    {
        var order = OrderBuilder.Build(Now, Employee.Id,
                new List<Service> { ServiceBuilder.Build(Guid.NewGuid(), TimeSpan.FromMinutes(30)) });

        var allOrders = new List<Order>() {
                OrderBuilder.Build(Now.AddMinutes(-30), Employee.Id, new List<Service>() { ServiceBuilder.Build(Guid.NewGuid(), TimeSpan.FromMinutes(30))})
            };

        var configuration = ConfigurationBuilder.BuildWithScheduleWithNoDelay();

        return new OrderPolicyTestData(order, allOrders, configuration, "QueueScheduleCloseToPreviousOrder");
    }

    private static OrderPolicyTestData QueueScheduleWithUpperAndBellowBoundary()
    {
        var order = OrderBuilder.Build(Now, Employee.Id,
                new List<Service> { ServiceBuilder.Build(Guid.NewGuid(), TimeSpan.FromMinutes(30)) });

        var allOrders = new List<Order>() {
                OrderBuilder.Build(
                    Now.AddMinutes(-30), Employee.Id,
                    new List<Service>() { ServiceBuilder.Build(Guid.NewGuid(), TimeSpan.FromMinutes(30))}
                ),
                OrderBuilder.Build(Now.AddMinutes(30))
            };

        var configuration = ConfigurationBuilder.BuildWithScheduleWithNoDelay();

        return new OrderPolicyTestData(order, allOrders, configuration, "QueueScheduleWithUpperAndBellowBoundary");
    }

    private static OrderPolicyTestData QueueScheduleWithFullScheduleTodayButFreeAWeekAfter()
    {
        var order = OrderBuilder.Build(Now.AddDays(7), Employee.Id,
                           new List<Service> { ServiceBuilder.Build(Guid.NewGuid(), TimeSpan.FromMinutes(30)) });

        var allOrders = new List<Order>()
        {
            OrderBuilder.Build(Now.AddHours(-1), Employee.Id,
                           new List<Service>() { ServiceBuilder.Build(Guid.NewGuid(), TimeSpan.FromMinutes(3*60)) }),
        };

        var configuration = ConfigurationBuilder.BuildWithScheduleWithNoDelay();

        return new OrderPolicyTestData(order, allOrders, configuration, "QueueScheduleWithFullScheduleTodayButFreeAWeekAfter");
    }

    private static OrderPolicyTestData QueueScheduleWithOrderForTomorrow()
    {
        var order = OrderBuilder.Build(Now.AddDays(1), Employee.Id,
                           new List<Service> { ServiceBuilder.Build(Guid.NewGuid(), TimeSpan.FromMinutes(30)) });

        var allOrders = new List<Order>()
        {
        };

        var configuration = ConfigurationBuilder.BuildWithScheduleWithNoDelay();

        return new OrderPolicyTestData(order, allOrders, configuration, "QueueScheduleWithFullScheduleTodayButFreeAWeekAfter");
    }
    private class OrderPolicyTestData
    {
        public Order Order { get; set; }
        public List<Order> Orders { get; set; }
        public Configuration Configuration { get; set; }
        public Schedule? LunchInterval { get; set; }
        public string TestId { get; set; }

        public OrderPolicyTestData(Order order, List<Order> orders, Configuration configuration, string testId)
        {
            Order = order;
            Orders = orders;
            Configuration = configuration;
            TestId = testId;
        }

        public OrderPolicyTestData(Order order, List<Order> orders, Configuration configuration, Schedule schedule, string testId)
        {
            Order = order;
            Orders = orders;
            Configuration = configuration;
            LunchInterval = schedule;
            TestId = testId;
        }
    }
}
