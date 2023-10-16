using Application.Services.Orders;
using Domain.Entities.Barbers;
using Domain.Entities.Barbers.Service;
using Domain.Entities.Orders;
using DomainTest.Entities;
using DomainTest.Entities.Barbers;
using DomainTest.Entities.Barbers.Services;
using DomainTest.Entities.Orders;

namespace ApplicationTest.Services.Orders;

public class OrderToolTest
{
    [Fact]
    public void ShouldNotRelocateWhenConfigurationIsNotQueue()
    {
        var configuration = ConfigurationBuilder.BuildWithScheduleWithNoDelay();

        var branch = BranchBuilder.Build(configuration);

        var order = OrderBuilder.Build();

        var orders = new List<Order>();

        var orderTool = new OrderTool();

        var relocatedTime = orderTool.RelocateTime(order, orders, branch);

        Assert.Equal(order.ScheduleTime, relocatedTime);
    }

    [Fact]
    public void ShouldRelocateWhenOrderIsInLunchTime()
    {

        var configuration = ConfigurationBuilder.BuildQueueWithVirtualQueue();

        var branch = BranchBuilder.Build(configuration);

        var now = DateTime.UtcNow;

        var schedule = new Schedule(new TimeOnly(now.AddHours(-1).Hour, now.Minute), new TimeOnly(now.AddHours(2).Hour, now.Minute), now.DayOfWeek);
        branch.AddSchedule(schedule);

        var employee = EmployeeBuilder.Build();
        branch.AddEmployee(employee);


        var lunchTime = new Schedule(new TimeOnly(now.Hour, now.Minute), new TimeOnly(now.AddHours(1).Hour, now.Minute), now.DayOfWeek);

        branch.AddEmployeeLunchInterval(lunchTime, employee.Id);

        var order = OrderBuilder.Build(now, employee.Id);

        var orderTool = new OrderTool();

        var relocatedTime = orderTool.RelocateTime(order, new List<Order>(), branch);

        Assert.Equal(relocatedTime, new DateTime(now.Year, now.Month, now.Day, now.AddHours(1).Hour, now.Minute, 0, now.Kind));
    }

    [Fact]
    public void ShouldNotRelocateOrRelocateToAfterLunchIntervalWhenThereIsNoOrder()
    {
        var configuration = ConfigurationBuilder.BuildQueueWithVirtualQueue();

        var branch = BranchBuilder.Build(configuration);

        var now = DateTime.UtcNow;

        var employee = EmployeeBuilder.Build();
        branch.AddEmployee(employee);

        var order = OrderBuilder.Build(now, employee.Id);

        var orders = new List<Order>();

        var orderTool = new OrderTool();

        var relocatedTime = orderTool.RelocateTime(order, orders, branch);

        Assert.Equal(relocatedTime, new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0, now.Kind));
    }

    [Fact]
    public void ShouldNotRelocateWhenTheLastOrderIsBeforeTheOrderSchedule()
    {
        var configuration = ConfigurationBuilder.BuildQueueWithVirtualQueue();

        var branch = BranchBuilder.Build(configuration);

        var now = DateTime.UtcNow;

        var employee = EmployeeBuilder.Build();
        branch.AddEmployee(employee);

        var order = OrderBuilder.Build(now.AddHours(1), employee.Id);

        var orders = new List<Order>
        {
            OrderBuilder.Build(now, employee.Id)
        };

        var orderTool = new OrderTool();

        var relocatedTime = orderTool.RelocateTime(order, orders, branch);

        Assert.Equal(relocatedTime, new DateTime(now.Year, now.Month, now.Day, now.AddHours(1).Hour, now.Minute, 0, now.Kind));
    }

    [Fact]
    public void ShouldRelocateForTheLastOrderTimeWhenIsAfterTheLastOrderSchedule()
    {
        var configuration = ConfigurationBuilder.BuildQueueWithVirtualQueue();

        var branch = BranchBuilder.Build(configuration);

        var now = DateTime.UtcNow;

        var employee = EmployeeBuilder.Build();

        branch.AddEmployee(employee);

        var order = OrderBuilder.Build(now.AddHours(1), employee.Id);

        var orders = new List<Order>
        {
            OrderBuilder.Build(now, employee.Id, new List<Service>() { ServiceBuilder.Build(branch.Id, TimeSpan.FromMinutes(60)), ServiceBuilder.Build(branch.Id, TimeSpan.FromMinutes(60))})
        };

        var orderTool = new OrderTool();

        var relocatedTime = orderTool.RelocateTime(order, orders, branch);

        Assert.Equal(relocatedTime, new DateTime(now.Year, now.Month, now.Day, now.AddHours(2).Hour, now.Minute, 0, now.Kind));
    }
    

}
