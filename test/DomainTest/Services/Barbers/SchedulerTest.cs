﻿using Domain.Entities.Barbers;
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
        Assert.Contains(new DateTime(nowMinusOneHour.Year, nowMinusOneHour.Month, nowMinusOneHour.Day,
                                     nowMinusOneHour.Hour, nowMinusOneHour.Minute, 0), availableTimes);
        Assert.Contains(new DateTime(nowMinusOneHour.AddMinutes(30).Year, nowMinusOneHour.AddMinutes(30).Month, nowMinusOneHour.AddMinutes(30).Day,
                                     nowMinusOneHour.AddMinutes(30).Hour, nowMinusOneHour.AddMinutes(30).Minute, 0), availableTimes);
        Assert.Contains(new DateTime(nowMinusOneHour.AddMinutes(60).Year, nowMinusOneHour.AddMinutes(60).Month, nowMinusOneHour.AddMinutes(60).Day,
                                     nowMinusOneHour.AddMinutes(60).Hour, nowMinusOneHour.AddMinutes(60).Minute, 0), availableTimes);
        Assert.Contains(new DateTime(nowMinusOneHour.AddMinutes(90).Year, nowMinusOneHour.AddMinutes(90).Month, nowMinusOneHour.AddMinutes(90).Day,
                                     nowMinusOneHour.AddMinutes(90).Hour, nowMinusOneHour.AddMinutes(90).Minute, 0), availableTimes);
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
        Assert.DoesNotContain(new DateTime(nowMinusOneHour.Year, nowMinusOneHour.Month, nowMinusOneHour.Day,
                                           nowMinusOneHour.Hour, nowMinusOneHour.Minute, 0), availableTimes);
        Assert.DoesNotContain(new DateTime(nowMinusOneHour.AddMinutes(30).Year, nowMinusOneHour.AddMinutes(30).Month, nowMinusOneHour.AddMinutes(30).Day,
                                           nowMinusOneHour.AddMinutes(30).Hour, nowMinusOneHour.AddMinutes(30).Minute, 0), availableTimes);
        Assert.Contains(new DateTime(nowMinusTwoHour.Year, nowMinusTwoHour.Month, nowMinusTwoHour.Day, nowMinusTwoHour.Hour, nowMinusTwoHour.Minute, 0), availableTimes);
        Assert.Contains(new DateTime(nowMinusTwoHour.Year, nowMinusTwoHour.Month, nowMinusTwoHour.Day, nowMinusTwoHour.Hour, nowMinusTwoHour.Minute, 0), availableTimes);
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

        var defaultInterval = 30;
        Assert.Equal(3, availableTimes.Count);
        Assert.Contains(new DateTime(nowElevenOClock.AddMinutes(3 * defaultInterval).Year, nowElevenOClock.AddMinutes(3 * defaultInterval).Month, nowElevenOClock.AddMinutes(3 * defaultInterval).Day,
                                     nowElevenOClock.AddMinutes(3 * defaultInterval).Hour, nowElevenOClock.AddMinutes(3 * defaultInterval).Minute, 0), availableTimes);
        Assert.Contains(new DateTime(nowElevenOClock.AddMinutes(4 * defaultInterval).Year, nowElevenOClock.AddMinutes(4 * defaultInterval).Month, nowElevenOClock.AddMinutes(4 * defaultInterval).Day,
                                     nowElevenOClock.AddMinutes(4 * defaultInterval).Hour, nowElevenOClock.AddMinutes(4 * defaultInterval).Minute, 0), availableTimes);
        Assert.Contains(new DateTime(nowElevenOClock.AddMinutes(5 * defaultInterval).Year, nowElevenOClock.AddMinutes(5 * defaultInterval).Month, nowElevenOClock.AddMinutes(5 * defaultInterval).Day,
                                     nowElevenOClock.AddMinutes(5 * defaultInterval).Hour, nowElevenOClock.AddMinutes(5 * defaultInterval).Minute, 0), availableTimes);
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
        Assert.DoesNotContain(new DateTime(nowMinusOneHour.Year, nowMinusOneHour.Month, nowMinusOneHour.Day,
                                     nowMinusOneHour.Hour, nowMinusOneHour.Minute, 0), availableTimes);
        Assert.DoesNotContain(new DateTime(nowMinusOneHour.AddMinutes(30).Year, nowMinusOneHour.AddMinutes(30).Month, nowMinusOneHour.AddMinutes(30).Day,
                                     nowMinusOneHour.AddMinutes(30).Hour, nowMinusOneHour.AddMinutes(30).Minute, 0), availableTimes);
        Assert.Contains(new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0), availableTimes);
        Assert.Contains(new DateTime(now.AddMinutes(30).Year, now.AddMinutes(30).Month, now.AddMinutes(30).Day,
                                     now.AddMinutes(30).Hour, now.AddMinutes(30).Minute, 0), availableTimes);
    }

    [Fact]
    public void ShouldBringTimesWhenTheTimeProvidedIsNotInWorkingTime()
    {
        var branch = BranchBuilder.Build(ConfigurationBuilder.BuildWithScheduleWithNoDelay(30));

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
        Assert.Contains(new DateTime(nowMinusOneHour.Year, nowMinusOneHour.Month, nowMinusOneHour.Day,
                                     nowMinusOneHour.Hour, nowMinusOneHour.Minute, 0), availableTimes);
        Assert.Contains(new DateTime(nowMinusOneHour.AddMinutes(30).Year, nowMinusOneHour.AddMinutes(30).Month, nowMinusOneHour.AddMinutes(30).Day,
                                     nowMinusOneHour.AddMinutes(30).Hour, nowMinusOneHour.AddMinutes(30).Minute, 0), availableTimes);
        Assert.Contains(new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0), availableTimes);
        Assert.Contains(new DateTime(now.AddMinutes(30).Year, now.AddMinutes(30).Month, now.AddMinutes(30).Day,
                                     now.AddMinutes(30).Hour, now.AddMinutes(30).Minute, 0), availableTimes);
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
        Assert.Contains(new DateTime(testNow.Year, testNow.Month, testNow.Day,
                                    testNow.Hour, testNow.Minute, 0), availableTimes);
        Assert.Contains(new DateTime(testNow.AddMinutes(defaultInterval).Year, testNow.AddMinutes(defaultInterval).Month, testNow.AddMinutes(defaultInterval).Day,
                                      testNow.AddMinutes(defaultInterval).Hour, testNow.AddMinutes(defaultInterval).Minute, 0, DateTimeKind.Utc), availableTimes);
        Assert.Contains(new DateTime(testNow.AddMinutes(-defaultInterval).AddDays(1).Year, testNow.AddMinutes(-defaultInterval).AddDays(1).Month, testNow.AddMinutes(-defaultInterval).AddDays(1).Day,
                                     testNow.AddMinutes(-defaultInterval).AddDays(1).Hour, testNow.AddMinutes(-defaultInterval).AddDays(1).Minute, 0, DateTimeKind.Utc), availableTimes);
        Assert.Contains(new DateTime(testNow.AddMinutes(-2 * defaultInterval).AddDays(1).Year, testNow.AddMinutes(-2 * defaultInterval).AddDays(1).Month, testNow.AddMinutes(-2 * defaultInterval).AddDays(1).Day,
                                     testNow.AddMinutes(-2 * defaultInterval).AddDays(1).Hour, testNow.AddMinutes(-2 * defaultInterval).AddDays(1).Minute, 0, DateTimeKind.Utc), availableTimes);
        Assert.DoesNotContain(new DateTime(testNow.AddDays(1).Year, testNow.AddDays(1).Month, testNow.AddDays(1).Day,
                                    testNow.AddDays(1).Hour, testNow.AddDays(1).Minute, 0), availableTimes);
        Assert.DoesNotContain(new DateTime(testNow.AddMinutes(-defaultInterval).Year, testNow.AddMinutes(-defaultInterval).Month, testNow.AddMinutes(-defaultInterval).Day,
                                    testNow.AddMinutes(-defaultInterval).Hour, testNow.AddMinutes(-defaultInterval).Minute, 0), availableTimes);
    }
    [Fact]
    public void ShouldReturnAvailableTimesMinusTheOrderTimeWhenOrderIsNotOverflowing()
    {
        var branch = BranchBuilder.Build(ConfigurationBuilder.BuildWithScheduleWithNoDelay(30));

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

        var services = new List<Service>() { ServiceBuilder.Build(branch.Id, TimeSpan.FromMinutes(30)) };
        branch.AddService(services.First());
        branch.AddService(services.First().Id, employee.Id);

        var order = OrderBuilder.Build(orderSchedule, branch, employee, services);

        var requestedOrder = OrderBaseBuilder.Build(branch, employee);
        var availableTimes = scheduler.GetAvailable(orderSchedule, requestedOrder, new List<Order>() { order });

        Assert.Equal(3, availableTimes.Count);
        Assert.DoesNotContain(new DateTime(orderSchedule.Year, orderSchedule.Month, orderSchedule.Day,
                                           orderSchedule.Hour, orderSchedule.Minute, 0), availableTimes);
        Assert.Contains(new DateTime(nowMinusTwoHour.Year, nowMinusTwoHour.Month, nowMinusTwoHour.Day,
                                           nowMinusTwoHour.Hour, nowMinusTwoHour.Minute, 0), availableTimes);
        Assert.Contains(new DateTime(nowMinusTwoHour.AddMinutes(30).Year, nowMinusTwoHour.AddMinutes(30).Month, nowMinusTwoHour.AddMinutes(30).Day,
                                           nowMinusTwoHour.AddMinutes(30).Hour, nowMinusTwoHour.AddMinutes(30).Minute, 0), availableTimes);
        Assert.Contains(new DateTime(orderSchedule.AddMinutes(30).Year, orderSchedule.AddMinutes(30).Month, orderSchedule.AddMinutes(30).Day,
                                           orderSchedule.AddMinutes(30).Hour, orderSchedule.AddMinutes(30).Minute, 0), availableTimes);
    }

    [Fact]
    public void ShouldReturnAvailableTimesMinusTheOrderTimeAndTheTimeThatIsOverflowingTheSchedule()
    {
        var branch = BranchBuilder.Build(ConfigurationBuilder.BuildWithScheduleWithNoDelay(30));

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

        var services = new List<Service>() { ServiceBuilder.Build(branch.Id, TimeSpan.FromMinutes(40)) };
        branch.AddService(services.First());
        branch.AddService(services.First().Id, employee.Id);

        var order = OrderBuilder.Build(nowMinusOneHour, branch, employee, services);

        var requestedOrder = OrderBaseBuilder.Build(branch, employee);

        var availableTimes = scheduler.GetAvailable(nowMinusTwoHour, requestedOrder, new List<Order>() { order });

        Assert.Equal(2, availableTimes.Count);
        Assert.DoesNotContain(new DateTime(nowMinusOneHour.Year, nowMinusOneHour.Month, nowMinusOneHour.Day,
                                           nowMinusOneHour.Hour, nowMinusOneHour.Minute, 0), availableTimes);
        Assert.DoesNotContain(new DateTime(nowMinusOneHour.AddMinutes(30).Year, nowMinusOneHour.AddMinutes(30).Month, now.AddMinutes(30).Day,
                                           nowMinusOneHour.AddMinutes(30).Hour, nowMinusOneHour.AddMinutes(30).Minute, 0), availableTimes);
        Assert.Contains(new DateTime(nowMinusTwoHour.Year, nowMinusTwoHour.Month, nowMinusTwoHour.Day, nowMinusTwoHour.Hour, nowMinusTwoHour.Minute, 0), availableTimes);
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
        Assert.Contains(new DateTime(nowMinusOneHour.Year, nowMinusOneHour.Month, nowMinusOneHour.Day,
                                     nowMinusOneHour.Hour, nowMinusOneHour.Minute, 0), availableTimes);
        Assert.Contains(new DateTime(nowMinusOneHour.AddMinutes(defaultInterval).Year, nowMinusOneHour.AddMinutes(defaultInterval).Month, nowMinusOneHour.AddMinutes(defaultInterval).Day,
                                     nowMinusOneHour.AddMinutes(defaultInterval).Hour, nowMinusOneHour.AddMinutes(defaultInterval).Minute, 0), availableTimes);
        Assert.Contains(new DateTime(nowMinusOneHour.AddMinutes(2 * defaultInterval).Year, nowMinusOneHour.AddMinutes(2 * defaultInterval).Month, nowMinusOneHour.AddMinutes(2 * defaultInterval).Day,
                                     nowMinusOneHour.AddMinutes(2 * defaultInterval).Hour, nowMinusOneHour.AddMinutes(2 * defaultInterval).Minute, 0), availableTimes);
    }

    [Fact]
    public void ShouldNotBringTimesThatAreBeforeUTCNowForNoDelaySchedule()
    {
        var defaultInterval = 45;

        var branch = BranchBuilder.Build(ConfigurationBuilder.BuildWithScheduleWithNoDelay( defaultInterval));

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

    private DateTime GetDate(DateTime date, int interval = 0)
    {
        var dateReference = date.AddMinutes(interval);

        return new DateTime(dateReference.Year, dateReference.Month, dateReference.Day,
                                       dateReference.Hour, dateReference.Minute, 0, date.Kind);
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
        var remainingTimeSerice = ServiceBuilder.Build(branch.Id, TimeSpan.FromMinutes(15));
        branch.AddService(remainingTimeSerice);
        branch.AddService(remainingTimeSerice.Id, employee.Id);

        var order = OrderBuilder.Build(nowMinusTwoHour.AddMinutes(3 * defaultInterval), branch, employee, new List<Service>() { service });

        var orders = new List<Order>() { order };

        var requestedOrder = OrderBaseBuilder.Build(branch, employee);

        var scheduler = new Scheduler();

        var availableTimes = scheduler.GetAvailable(nowPlusOneHour, requestedOrder, orders);

        Assert.Equal(2, availableTimes.Count);

        Assert.Contains(GetDate(nowMinusTwoHour, (4 * defaultInterval) + 15), availableTimes);
        Assert.Contains(GetDate(nowMinusTwoHour, 5 * defaultInterval), availableTimes);
    }
}
