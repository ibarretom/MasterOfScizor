using Domain.Entities.Barbers;
using Domain.Entities.Barbers.Service;
using Domain.Entities.Orders;
using Domain.Services.Barbers;
using DomainTest.Entities;
using DomainTest.Entities.Barbers;
using DomainTest.Entities.Barbers.Services;
using DomainTest.Entities.Orders;

namespace ApplicationTest.Services.Orders;

public class SchedulerTest
{
    [Fact]
    public void ShouldReturnAllAvailableTimesForNoLunchIntervalOrOrders()
    {
        var defaultInterval = 30;
        var branch = BranchBuilder.Build(ConfigurationBuilder.BuildWithScheduleWithNoDelay(defaultInterval));

        var utcNow = DateTime.UtcNow.AddDays(2);
        var now = new DateTime(utcNow.Year, utcNow.Month, utcNow.Day,
                               0, 23, 32, DateTimeKind.Utc);

        var nowMinusOneHour = now.AddHours(-2);
        var nowPlusOneHour = now.AddHours(1);

        var schedule = new Schedule(new TimeOnly(nowMinusOneHour.Hour, nowMinusOneHour.Minute),
                                    new TimeOnly(nowPlusOneHour.Hour, nowPlusOneHour.Minute), nowMinusOneHour.DayOfWeek);
        branch.AddSchedule(schedule);

        var employee = EmployeeBuilder.Build();
        branch.AddEmployee(employee);

        var service = ServiceBuilder.Build(branch.Id, TimeSpan.FromMinutes(defaultInterval));
        branch.AddService(service);
        branch.AddService(service.Id, employee.Id);

        var scheduler = new Scheduler();

        var order = OrderBaseBuilder.Build(branch, employee);

        var availableTimes = scheduler.GetAvailable(now.AddHours(-4), order, new List<Order>());

        Assert.Equal(4, availableTimes.Count);
        Assert.Contains(GetDate(nowMinusOneHour), availableTimes);
        Assert.Contains(GetDate(nowMinusOneHour, defaultInterval), availableTimes);
        Assert.Contains(GetDate(nowMinusOneHour, 2 * defaultInterval), availableTimes);
        Assert.Contains(GetDate(nowMinusOneHour, 3 * defaultInterval), availableTimes);
    }

    private static DateTime GetDate(DateTime date, int interval = 0)
    {
        var dateReference = date.AddMinutes(interval);

        return new DateTime(dateReference.Year, dateReference.Month, dateReference.Day,
                                       dateReference.Hour, dateReference.Minute, 0, date.Kind);
    }

    [Fact]
    public void ShouldReturnAllOrderTimesMinusLunchIntervalTimes()
    {
        var defaultInterval = 30;

        var branch = BranchBuilder.Build(ConfigurationBuilder.BuildWithScheduleWithNoDelay(defaultInterval));

        var utcNow = DateTime.UtcNow.AddDays(2);
        var now = new DateTime(utcNow.Year, utcNow.Month, utcNow.Day,
                               0, 23, 32, DateTimeKind.Utc);

        var nowMinusTwoHour = now.AddHours(-2);
        var nowPlusOneHour = now.AddHours(1);

        var schedule = new Schedule(new TimeOnly(nowMinusTwoHour.Hour, nowMinusTwoHour.Minute),
                                    new TimeOnly(nowPlusOneHour.Hour, nowPlusOneHour.Minute), nowMinusTwoHour.DayOfWeek);
        branch.AddSchedule(schedule);

        var employee = EmployeeBuilder.Build();

        branch.AddEmployee(employee);

        var nowMinusOneHour = now.AddHours(-1);
        branch.AddEmployeeLunchInterval(new Schedule(new TimeOnly(nowMinusOneHour.Hour, nowMinusOneHour.Minute),
                                                     new TimeOnly(now.Hour, now.Minute), nowMinusOneHour.DayOfWeek), employee.Id);

        var service = ServiceBuilder.Build(branch.Id, TimeSpan.FromMinutes(defaultInterval));
        branch.AddService(service);
        branch.AddService(service.Id, employee.Id);

        var scheduler = new Scheduler();

        var order = OrderBaseBuilder.Build(branch, employee);

        var availableTimes = scheduler.GetAvailable(nowMinusTwoHour, order, new List<Order>());

        Assert.Equal(2, availableTimes.Count);
        Assert.DoesNotContain(GetDate(nowMinusOneHour), availableTimes);
        Assert.DoesNotContain(GetDate(nowMinusOneHour, defaultInterval), availableTimes);
        Assert.Contains(GetDate(nowMinusTwoHour), availableTimes);
        Assert.Contains(GetDate(nowMinusTwoHour, defaultInterval), availableTimes);
    }

    [Fact]
    public void ShouldPassWhenTheLunchIntervalIsOverflowing()
    {
        var defaultInterval = 30;

        var branch = BranchBuilder.Build(ConfigurationBuilder.BuildWithScheduleWithNoDelay(defaultInterval));

        var now = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day,
                               0, 23, 32, DateTimeKind.Utc);

        var nowElevenOClock = new DateTime(now.Year, now.Month, now.Day, 23, 0, 0, DateTimeKind.Utc);
        var nowNextDayTwoAm = new DateTime(now.AddDays(1).Year, now.AddDays(1).Month, now.AddDays(1).Day, 2, 0, 0, DateTimeKind.Utc);

        var schedule = new Schedule(new TimeOnly(nowElevenOClock.Hour, nowElevenOClock.Minute),
                                    new TimeOnly(nowNextDayTwoAm.Hour, nowNextDayTwoAm.Minute), nowElevenOClock.DayOfWeek);
        branch.AddSchedule(schedule);

        var employee = EmployeeBuilder.Build();
        branch.AddEmployee(employee);
        branch.AddEmployeeLunchInterval(new Schedule(new TimeOnly(23, 30),
                                                     new TimeOnly(0, 30), nowElevenOClock.DayOfWeek), employee.Id);

        var service = ServiceBuilder.Build(branch.Id, TimeSpan.FromMinutes(defaultInterval));
        branch.AddService(service);
        branch.AddService(service.Id, employee.Id);

        var scheduler = new Scheduler();

        var order = OrderBaseBuilder.Build(branch, employee);

        var availableTimes = scheduler.GetAvailable(nowNextDayTwoAm, order, new List<Order>());

        Assert.Equal(3, availableTimes.Count);
        Assert.Contains(GetDate(nowElevenOClock, 3 * defaultInterval), availableTimes);
        Assert.Contains(GetDate(nowElevenOClock, 4 * defaultInterval), availableTimes);
        Assert.Contains(GetDate(nowElevenOClock, 5 * defaultInterval), availableTimes);
    }

    [Fact]
    public void ShouldNotHaveTimesBeforeTheDayTimePassed()
    {
        var defaultInterval = 30;

        var branch = BranchBuilder.Build(ConfigurationBuilder.BuildWithScheduleWithNoDelay(defaultInterval));

        var utcNow = DateTime.UtcNow.AddDays(2);
        var now = new DateTime(utcNow.Year, utcNow.Month, utcNow.Day,
                               0, 23, 32, DateTimeKind.Utc);

        var nowMinusOneHour = now.AddHours(-1);
        var nowPlusOneHour = now.AddHours(1);

        var schedule = new Schedule(new TimeOnly(nowMinusOneHour.Hour, nowMinusOneHour.Minute),
                                    new TimeOnly(nowPlusOneHour.Hour, nowPlusOneHour.Minute), nowMinusOneHour.DayOfWeek);
        branch.AddSchedule(schedule);

        var employee = EmployeeBuilder.Build();
        branch.AddEmployee(employee);

        var service = ServiceBuilder.Build(branch.Id, TimeSpan.FromMinutes(defaultInterval));
        branch.AddService(service);
        branch.AddService(service.Id, employee.Id);

        var scheduler = new Scheduler();

        var order = OrderBaseBuilder.Build(branch, employee);

        var availableTimes = scheduler.GetAvailable(now, order, new List<Order>());

        Assert.Equal(2, availableTimes.Count);

        Assert.DoesNotContain(GetDate(nowMinusOneHour), availableTimes);
        Assert.DoesNotContain(GetDate(nowMinusOneHour, defaultInterval), availableTimes);

        Assert.Contains(GetDate(now), availableTimes);
        Assert.Contains(GetDate(now, defaultInterval), availableTimes);
    }

    [Fact]
    public void ShouldBringTimesWhenTheTimeProvidedIsNotInWorkingTime()
    {
        var defaultInterval = 30;
        var branch = BranchBuilder.Build(ConfigurationBuilder.BuildWithScheduleWithNoDelay(defaultInterval));

        var utcNow = DateTime.UtcNow.AddDays(2);
        var now = new DateTime(utcNow.Year, utcNow.Month, utcNow.Day,
                               12, 23, 13);

        var nowMinusOneHour = now.AddHours(-1);
        var nowPlusOneHour = now.AddHours(1);

        var schedule = new Schedule(new TimeOnly(nowMinusOneHour.Hour, nowMinusOneHour.Minute),
                                    new TimeOnly(nowPlusOneHour.Hour, nowPlusOneHour.Minute), nowMinusOneHour.DayOfWeek);
        branch.AddSchedule(schedule);

        var employee = EmployeeBuilder.Build();
        branch.AddEmployee(employee);

        var service = ServiceBuilder.Build(branch.Id, TimeSpan.FromMinutes(defaultInterval));
        branch.AddService(service);
        branch.AddService(service.Id, employee.Id);

        var scheduler = new Scheduler();

        var order = OrderBaseBuilder.Build(branch, employee);

        var availableTimes = scheduler.GetAvailable(now.AddHours(-2), order, new List<Order>());

        Assert.Equal(4, availableTimes.Count);
        Assert.Contains(GetDate(nowMinusOneHour), availableTimes);
        Assert.Contains(GetDate(nowMinusOneHour, defaultInterval), availableTimes);
        Assert.Contains(GetDate(now), availableTimes);
        Assert.Contains(GetDate(now, defaultInterval), availableTimes);
    }

    [Fact]
    public void ShouldBringTimesForTwoSchedulesWhenHasAScheduleCrossingDays()
    {
        var defaultInterval = 30;
        var branch = BranchBuilder.Build(ConfigurationBuilder.BuildWithScheduleWithNoDelay(defaultInterval));

        var utcNow = DateTime.UtcNow.AddDays(2);
        var testNow = new DateTime(utcNow.Year, utcNow.Month, utcNow.Day, 0, 0, 0);

        var nowMinusOneHour = testNow.AddHours(-1);
        var nowPlusOneHour = testNow.AddHours(1);

        var schedule = new Schedule(new TimeOnly(nowMinusOneHour.Hour, nowMinusOneHour.Minute),
                                               new TimeOnly(nowPlusOneHour.Hour, nowPlusOneHour.Minute), nowMinusOneHour.DayOfWeek);

        branch.AddSchedule(schedule);

        schedule = new Schedule(new TimeOnly(nowMinusOneHour.Hour, nowMinusOneHour.Minute),
                                new TimeOnly(nowPlusOneHour.Hour, nowPlusOneHour.Minute), nowMinusOneHour.AddDays(1).DayOfWeek);

        branch.AddSchedule(schedule);

        var employee = EmployeeBuilder.Build();
        branch.AddEmployee(employee);

        var service = ServiceBuilder.Build(branch.Id, TimeSpan.FromMinutes(defaultInterval));
        branch.AddService(service);
        branch.AddService(service.Id, employee.Id);

        var scheduler = new Scheduler();

        var order = OrderBaseBuilder.Build(branch, employee);

        var availableTimes = scheduler.GetAvailable(testNow.AddHours(4), order, new List<Order>());

        Assert.Equal(4, availableTimes.Count);
        Assert.Contains(GetDate(testNow), availableTimes);
        Assert.Contains(GetDate(testNow, defaultInterval), availableTimes);
        Assert.Contains(GetDate(testNow.AddDays(1), -defaultInterval), availableTimes);
        Assert.Contains(GetDate(testNow.AddDays(1), -2 * defaultInterval), availableTimes);

        Assert.DoesNotContain(GetDate(testNow.AddDays(1)), availableTimes);
        Assert.DoesNotContain(GetDate(testNow, -defaultInterval), availableTimes);
    }
    [Fact]
    public void ShouldReturnAvailableTimesMinusTheOrderTimeWhenOrderIsNotOverflowing()
    {
        var defaultInterval = 30;
        var branch = BranchBuilder.Build(ConfigurationBuilder.BuildWithScheduleWithNoDelay(defaultInterval));

        var utcNow = DateTime.UtcNow.AddDays(2);
        var now = new DateTime(utcNow.Year, utcNow.Month, utcNow.Day,
                               0, 23, 32, DateTimeKind.Utc);

        var nowMinusTwoHour = now.AddHours(-2);
        var nowPlusOneHour = now.AddHours(1);

        var schedule = new Schedule(new TimeOnly(nowMinusTwoHour.Hour, nowMinusTwoHour.Minute),
                                    new TimeOnly(nowPlusOneHour.Hour, nowPlusOneHour.Minute), nowMinusTwoHour.DayOfWeek);
        branch.AddSchedule(schedule);

        var employee = EmployeeBuilder.Build();

        branch.AddEmployee(employee);

        var scheduler = new Scheduler();

        var orderSchedule = now.AddHours(-1);

        var services = new List<Service>() { ServiceBuilder.Build(branch.Id, TimeSpan.FromMinutes(defaultInterval)) };
        branch.AddService(services.First());
        branch.AddService(services.First().Id, employee.Id);

        var order = OrderBuilder.Build(orderSchedule, branch, employee, services);

        var requestedOrder = OrderBaseBuilder.Build(branch, employee);
        var availableTimes = scheduler.GetAvailable(orderSchedule, requestedOrder, new List<Order>() { order });

        Assert.Equal(3, availableTimes.Count);

        Assert.DoesNotContain(GetDate(orderSchedule), availableTimes);

        Assert.Contains(GetDate(orderSchedule, defaultInterval), availableTimes);
        Assert.Contains(GetDate(nowMinusTwoHour), availableTimes);
        Assert.Contains(GetDate(nowMinusTwoHour, defaultInterval), availableTimes);
    }

    [Fact]
    public void ShouldReturnAvailableTimesMinusTheOrderTimeAndTheTimeThatIsOverflowingTheSchedule()
    {
        var defaultInterval = 30;
        var branch = BranchBuilder.Build(ConfigurationBuilder.BuildWithScheduleWithNoDelay(defaultInterval));

        var utcNow = DateTime.UtcNow.AddDays(2);
        var now = new DateTime(utcNow.Year, utcNow.Month, utcNow.Day,
                               0, 13, 32, DateTimeKind.Utc);

        var nowMinusTwoHour = now.AddHours(-2);
        var nowPlusOneHour = now.AddHours(1);

        var schedule = new Schedule(new TimeOnly(nowMinusTwoHour.Hour, nowMinusTwoHour.Minute),
                                    new TimeOnly(nowPlusOneHour.Hour, nowPlusOneHour.Minute), nowMinusTwoHour.DayOfWeek);
        branch.AddSchedule(schedule);

        var employee = EmployeeBuilder.Build();
        branch.AddEmployee(employee);

        var scheduler = new Scheduler();

        var nowMinusOneHour = now.AddHours(-1);

        var services = new List<Service>() { ServiceBuilder.Build(branch.Id, TimeSpan.FromMinutes(defaultInterval + 10)) };
        branch.AddService(services.First());
        branch.AddService(services.First().Id, employee.Id);

        var order = OrderBuilder.Build(nowMinusOneHour, branch, employee, services);

        var requestedOrder = OrderBaseBuilder.Build(branch, employee);

        var availableTimes = scheduler.GetAvailable(nowMinusTwoHour, requestedOrder, new List<Order>() { order });

        Assert.Equal(2, availableTimes.Count);
        Assert.Contains(GetDate(nowMinusTwoHour), availableTimes);

        Assert.DoesNotContain(GetDate(nowMinusOneHour), availableTimes);
        Assert.DoesNotContain(GetDate(nowMinusOneHour, defaultInterval), availableTimes);
    }

    [Fact]
    public void ShouldBeAbleToBringAllTimesForANonMultipleInterval()
    {
        var defaultInterval = 45;
        var branch = BranchBuilder.Build(ConfigurationBuilder.BuildWithScheduleWithNoDelay(defaultInterval));

        var utcNow = DateTime.UtcNow.AddDays(2);
        var now = new DateTime(utcNow.Year, utcNow.Month, utcNow.Day,
                               0, 23, 32, DateTimeKind.Utc);

        var nowMinusOneHour = now.AddHours(-2);
        var nowPlusOneHour = now.AddHours(1);

        var schedule = new Schedule(new TimeOnly(nowMinusOneHour.Hour, nowMinusOneHour.Minute),
                                    new TimeOnly(nowPlusOneHour.Hour, nowPlusOneHour.Minute), nowMinusOneHour.DayOfWeek);
        branch.AddSchedule(schedule);

        var employee = EmployeeBuilder.Build();
        branch.AddEmployee(employee);

        var service = ServiceBuilder.Build(branch.Id, TimeSpan.FromMinutes(defaultInterval));
        branch.AddService(service);
        branch.AddService(service.Id, employee.Id);

        var scheduler = new Scheduler();

        var order = OrderBaseBuilder.Build(branch, employee);
        var availableTimes = scheduler.GetAvailable(now.AddHours(-2), order, new List<Order>());

        Assert.Equal(3, availableTimes.Count);
        Assert.Contains(GetDate(nowMinusOneHour), availableTimes);
        Assert.Contains(GetDate(nowMinusOneHour, defaultInterval), availableTimes);
        Assert.Contains(GetDate(nowMinusOneHour, 2 * defaultInterval), availableTimes);
    }

    [Fact]
    public void ShouldNotBringTimesThatAreBeforeUTCNowForNoDelaySchedule()
    {
        var defaultInterval = 45;

        var branch = BranchBuilder.Build(ConfigurationBuilder.BuildWithScheduleWithNoDelay(defaultInterval));

        var now = DateTime.UtcNow;

        var nowMinusTwoHour = now.AddHours(-2);
        var nowPlusOneHour = now.AddHours(1);

        var schedule = new Schedule(new TimeOnly(nowMinusTwoHour.Hour, nowMinusTwoHour.Minute),
                                    new TimeOnly(nowPlusOneHour.Hour, nowPlusOneHour.Minute), nowMinusTwoHour.DayOfWeek);
        branch.AddSchedule(schedule);

        var employee = EmployeeBuilder.Build();
        branch.AddEmployee(employee);

        var service = ServiceBuilder.Build(branch.Id, TimeSpan.FromMinutes(defaultInterval));
        branch.AddService(service);
        branch.AddService(service.Id, employee.Id);

        var scheduler = new Scheduler();

        var requestedOrder = OrderBaseBuilder.Build(branch, employee);

        var availableTimes = scheduler.GetAvailable(now, requestedOrder, new List<Order>());

        Assert.True(availableTimes.Any());

        Assert.DoesNotContain(availableTimes, time => time < new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0, now.Kind));
    }

    [Fact]
    public void ShouldBringTimesConsideringTheDelayTime()
    {
        var defaultInterval = 30;
        var delay = 30;

        var branch = BranchBuilder.Build(ConfigurationBuilder.BuildWithScheduleWithDelay(delay, defaultInterval));

        var now = DateTime.UtcNow.AddMinutes(-delay);

        var nowMinusTwoHour = now.AddHours(-2);
        var nowPlusTwoHour = now.AddHours(2);

        var schedule = new Schedule(new TimeOnly(nowMinusTwoHour.Hour, nowMinusTwoHour.Minute),
                                    new TimeOnly(nowPlusTwoHour.Hour, nowPlusTwoHour.Minute), nowMinusTwoHour.DayOfWeek);
        branch.AddSchedule(schedule);

        var employee = EmployeeBuilder.Build();
        branch.AddEmployee(employee);

        var service = ServiceBuilder.Build(branch.Id, TimeSpan.FromMinutes(defaultInterval));
        branch.AddService(service);
        branch.AddService(service.Id, employee.Id);

        var scheduler = new Scheduler();

        var requestedOrder = OrderBaseBuilder.Build(branch, employee);

        var availableTimes = scheduler.GetAvailable(now, requestedOrder, new List<Order>());

        Assert.True(availableTimes.Any());
        Assert.DoesNotContain(availableTimes, time => time.AddMinutes(delay) < new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0, now.Kind));
    }

    [Fact]
    public void ShouldRemoveADateWhenHasAOrderThatStartsADayBeforeOfTheOverflowingDayOfTheScheduleAndTheServiceOverflows()
    {
        var defaultInterval = 45;

        var branch = BranchBuilder.Build(ConfigurationBuilder.BuildWithScheduleWithNoDelay(defaultInterval));

        var utcNow = DateTime.UtcNow.AddDays(2);
        var now = new DateTime(utcNow.Year, utcNow.Month, utcNow.Day,
                               0, 23, 32, DateTimeKind.Utc);

        var nowMinusTwoHour = now.AddHours(-2);
        var nowPlusTwHours = now.AddHours(2);

        var schedule = new Schedule(new TimeOnly(nowMinusTwoHour.Hour, nowMinusTwoHour.Minute),
                                    new TimeOnly(nowPlusTwHours.Hour, nowPlusTwHours.Minute), nowMinusTwoHour.DayOfWeek);
        branch.AddSchedule(schedule);

        var employee = EmployeeBuilder.Build();
        branch.AddEmployee(employee);

        var service = ServiceBuilder.Build(branch.Id, TimeSpan.FromMinutes(60));
        branch.AddService(service);
        branch.AddService(service.Id, employee.Id);

        var order = OrderBuilder.Build(nowMinusTwoHour.AddMinutes(2 * defaultInterval), branch, employee, new List<Service>() { service });

        var orders = new List<Order>() { order };

        var requestedOrder = OrderBaseBuilder.Build(branch, employee);

        var scheduler = new Scheduler();

        var availableTimes = scheduler.GetAvailable(nowPlusTwHours, requestedOrder, orders);

        Assert.Equal(2, availableTimes.Count);
        Assert.Contains(GetDate(nowMinusTwoHour, 4 * defaultInterval), availableTimes);
        Assert.Contains(GetDate(nowMinusTwoHour, 5 * defaultInterval), availableTimes);
    }

    [Fact]
    public void ShouldNotRemoveTheTimeAfterTheLunchIntervalWhenHasAOrderWithServicesBeforeTheLunchInterval()
    {
        var defaultInterval = 45;

        var branch = BranchBuilder.Build(ConfigurationBuilder.BuildWithScheduleWithNoDelay(defaultInterval));

        var utcNow = DateTime.UtcNow.AddDays(2);
        var now = new DateTime(utcNow.Year, utcNow.Month, utcNow.Day,
                               0, 23, 32, DateTimeKind.Utc);

        var nowMinusTwoHour = now.AddHours(-3);
        var nowPlusTwHours = now.AddHours(2);

        var schedule = new Schedule(new TimeOnly(nowMinusTwoHour.Hour, nowMinusTwoHour.Minute),
                                    new TimeOnly(nowPlusTwHours.Hour, nowPlusTwHours.Minute), nowMinusTwoHour.DayOfWeek);
        branch.AddSchedule(schedule);

        var employee = EmployeeBuilder.Build();
        branch.AddEmployee(employee);

        var lunchIntervalStartsAt = nowMinusTwoHour.AddMinutes(2 * defaultInterval);
        var lunchIntervalEndsAt = nowMinusTwoHour.AddMinutes(3 * defaultInterval);
        var lunchInterval = new Schedule(new TimeOnly(lunchIntervalStartsAt.Hour, lunchIntervalStartsAt.Minute),
                                         new TimeOnly(lunchIntervalEndsAt.Hour, lunchIntervalEndsAt.Minute), lunchIntervalStartsAt.DayOfWeek);
        branch.AddEmployeeLunchInterval(lunchInterval, employee.Id);

        var service = ServiceBuilder.Build(branch.Id, TimeSpan.FromMinutes(defaultInterval));
        branch.AddService(service);
        branch.AddService(service.Id, employee.Id);
        service = ServiceBuilder.Build(branch.Id, TimeSpan.FromMinutes(10));

        var order = OrderBuilder.Build(nowMinusTwoHour.AddMinutes(defaultInterval), branch, employee, new List<Service>() { service });

        var orders = new List<Order>() { order };

        var requestedOrder = OrderBaseBuilder.Build(branch, employee);

        var scheduler = new Scheduler();

        var availableTimes = scheduler.GetAvailable(nowMinusTwoHour, requestedOrder, orders);

        Assert.Equal(2, availableTimes.Count);
        Assert.Contains(GetDate(nowMinusTwoHour), availableTimes);
        Assert.Contains(GetDate(nowMinusTwoHour, 3 * defaultInterval), availableTimes);
    }

    [Fact]
    public void ShouldBringTheTimeThatOverflowTheScheduleWhenTheSumOfOrderServiceOverflowTheSchedule()
    {
        var defaultInterval = 30;

        var branch = BranchBuilder.Build(ConfigurationBuilder.BuildWithScheduleWithNoDelay(defaultInterval));

        var utcNow = DateTime.UtcNow.AddDays(2);
        var now = new DateTime(utcNow.Year, utcNow.Month, utcNow.Day,
                               0, 23, 32, DateTimeKind.Utc);

        var nowMinusTwoHour = now.AddHours(-2);
        var nowPlusOneHour = now.AddHours(1);

        var schedule = new Schedule(new TimeOnly(nowMinusTwoHour.Hour, nowMinusTwoHour.Minute),
                                    new TimeOnly(nowPlusOneHour.Hour, nowPlusOneHour.Minute), nowMinusTwoHour.DayOfWeek);
        branch.AddSchedule(schedule);

        var employee = EmployeeBuilder.Build();
        branch.AddEmployee(employee);

        var service = ServiceBuilder.Build(branch.Id, TimeSpan.FromMinutes(defaultInterval + 15));
        branch.AddService(service);
        branch.AddService(service.Id, employee.Id);
        var remainingTimeService = ServiceBuilder.Build(branch.Id, TimeSpan.FromMinutes(15));
        branch.AddService(remainingTimeService);
        branch.AddService(remainingTimeService.Id, employee.Id);

        var order = OrderBuilder.Build(nowMinusTwoHour.AddMinutes(3 * defaultInterval), branch, employee, new List<Service>() { service });

        var orders = new List<Order>() { order };

        var requestedOrder = OrderBaseBuilder.Build(branch, employee);

        var scheduler = new Scheduler();

        var availableTimes = scheduler.GetAvailable(nowPlusOneHour, requestedOrder, orders);

        Assert.Equal(2, availableTimes.Count);

        Assert.Contains(GetDate(nowMinusTwoHour, (4 * defaultInterval) + 15), availableTimes);
        Assert.Contains(GetDate(nowMinusTwoHour, 5 * defaultInterval), availableTimes);
    }

    [Fact]
    public void ShouldConsiderTheServicesInTheOrderToBringTheAvailableTimes()
    {
        var defaultInterval = 45;

        var branch = BranchBuilder.Build(ConfigurationBuilder.BuildWithScheduleWithNoDelay(defaultInterval));
        
        var utcNow = DateTime.UtcNow.AddDays(2);

        var now = new DateTime(utcNow.Year, utcNow.Month, utcNow.Day,
                                          0, 23, 32, DateTimeKind.Utc);
        var nowMinusSomeHour = now.AddHours(-3);
        var nowPlusTwoHour = now.AddHours(2);

        var schedule = new Schedule(new TimeOnly(nowMinusSomeHour.Hour, nowMinusSomeHour.Minute),
                                    new TimeOnly(nowPlusTwoHour.Hour, nowPlusTwoHour.Minute), nowMinusSomeHour.DayOfWeek);
        branch.AddSchedule(schedule);

        var employee = EmployeeBuilder.Build();
        branch.AddEmployee(employee);

        var serviceForRequesteOrder = ServiceBuilder.Build(branch.Id, TimeSpan.FromMinutes(defaultInterval + 15));
        branch.AddService(serviceForRequesteOrder);
        branch.AddService(serviceForRequesteOrder.Id, employee.Id);
        var service = ServiceBuilder.Build(branch.Id, TimeSpan.FromMinutes(defaultInterval));
        branch.AddService(serviceForRequesteOrder);

        var orderStatingAtFirstTime = OrderBuilder.Build(nowMinusSomeHour, branch, employee, new List<Service>() { service });
        var orderForRemoveTheLasTime = OrderBuilder.Build(nowMinusSomeHour.AddMinutes(3 * defaultInterval), branch, employee, new List<Service>() { service });

        var requestedOrder = OrderBaseBuilder.Build(branch, employee, new List<Service>() { serviceForRequesteOrder });

        var scheduler = new Scheduler();

        var orders = new List<Order>() { orderStatingAtFirstTime, orderForRemoveTheLasTime };

        var availableTimes = scheduler.GetAvailable(nowMinusSomeHour, requestedOrder, orders);

        Assert.Single(availableTimes);
        Assert.Contains(GetDate(nowMinusSomeHour, defaultInterval), availableTimes);

    }

    [Fact]
    public void ShouldNotBringTheLastTimeThatIsBeforeTheClosingTimeWhenTheOrderOverflowsIt()
    {
        var defaultInterval = 45;

        var branch = BranchBuilder.Build(ConfigurationBuilder.BuildWithScheduleWithNoDelay(defaultInterval));

        var utcNow = DateTime.UtcNow.AddDays(2);

        var now = new DateTime(utcNow.Year, utcNow.Month, utcNow.Day,
                                          0, 23, 32, DateTimeKind.Utc);
        var nowMinusSomeHour = now.AddHours(-1);
        var nowPlusTwoHour = now.AddHours(2);

        var schedule = new Schedule(new TimeOnly(nowMinusSomeHour.Hour, nowMinusSomeHour.Minute),
                                    new TimeOnly(nowPlusTwoHour.Hour, nowPlusTwoHour.Minute), nowMinusSomeHour.DayOfWeek);
        branch.AddSchedule(schedule);

        var employee = EmployeeBuilder.Build();
        branch.AddEmployee(employee);

        var service = ServiceBuilder.Build(branch.Id, TimeSpan.FromMinutes(defaultInterval));
        branch.AddService(service);
        branch.AddService(service.Id, employee.Id);
        var serviceToRemoveAllTimes = ServiceBuilder.Build(branch.Id, TimeSpan.FromMinutes(2 * defaultInterval));

        var order = OrderBuilder.Build(nowMinusSomeHour.AddMinutes(defaultInterval), branch, employee, new List<Service>() { serviceToRemoveAllTimes });

        var scheduler = new Scheduler();

        var requestedOrder = OrderBaseBuilder.Build(branch, employee, new List<Service> { service });

        var availableTimes = scheduler.GetAvailable(nowPlusTwoHour, requestedOrder, new List<Order>() { order });

        Assert.Empty(availableTimes);
    }

    [Fact]
    public void ShouldNotAcceptWhenTheOrderServicesOverflowsALocatedSchedule()
    {
        var defaultInterval = 45;

        var branch = BranchBuilder.Build(ConfigurationBuilder.BuildWithScheduleWithNoDelay(defaultInterval));

        var utcNow = DateTime.UtcNow.AddDays(2);

        var now = new DateTime(utcNow.Year, utcNow.Month, utcNow.Day,
                                          0, 23, 32, DateTimeKind.Utc);
        var nowMinusSomeHour = now.AddHours(-3);
        var nowPlusTwoHour = now.AddHours(2);

        var schedule = new Schedule(new TimeOnly(nowMinusSomeHour.Hour, nowMinusSomeHour.Minute),
                                    new TimeOnly(nowPlusTwoHour.Hour, nowPlusTwoHour.Minute), nowMinusSomeHour.DayOfWeek);
        branch.AddSchedule(schedule);

        var employee = EmployeeBuilder.Build();
        branch.AddEmployee(employee);

        var service = ServiceBuilder.Build(branch.Id, TimeSpan.FromMinutes(defaultInterval));
        branch.AddService(service);
        branch.AddService(service.Id, employee.Id);

        var order = OrderBuilder.Build(nowMinusSomeHour.AddMinutes(defaultInterval), branch, employee, new List<Service>() { service });

        var requestedOrder = OrderBaseBuilder.Build(branch, employee, new List<Service> { service, service, service });

        var scheduler = new Scheduler();

        var availableTimes = scheduler.GetAvailable(nowMinusSomeHour, requestedOrder, new List<Order>() { order });

        Assert.Equal(2, availableTimes.Count);

        Assert.Contains(GetDate(nowMinusSomeHour, 2 * defaultInterval), availableTimes);
        Assert.Contains(GetDate(nowMinusSomeHour, 3 * defaultInterval), availableTimes);
    }
}
