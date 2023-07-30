﻿using Application.Services.Branches;
using Domain.Entities.Barbers;
using DomainTest.ValueObjects.DTO;
using Infra.Repositories.Company;
using Moq;

namespace ApplicationTest.Services.Branches;

public class ScheduleServiceTest
{
    [Fact]
    public async void ShouldBeAbleToAddASchedule()
    {
        var schedule = ScheduleRequestDTOBuilder.Build();

        var scheduleRepository = new Mock<IScheduleRepository>();

        scheduleRepository.Setup(repository => repository.Add(It.IsAny<Schedule>()));

        var scheduleService = new ScheduleService(scheduleRepository.Object);

        var createdSchedule = await scheduleService.Add(schedule);

        Assert.True(createdSchedule.CreatedSchedule.Count.Equals(schedule.Schedule.Count));
        Assert.True(!createdSchedule.ExistentSchedule.Any());

        foreach (var day in createdSchedule.CreatedSchedule)
        {
            Assert.Contains(day, schedule.Schedule);
        }
    }

    [Fact]
    public async void ShouldNotCreateAScheduleForAnExistentDayOfWeek()
    {
        var schedule = ScheduleRequestDTOBuilder.Build();

        var scheduleRepository = new Mock<IScheduleRepository>();
        scheduleRepository.Setup(repository => repository.Add(It.IsAny<Schedule>()));

        scheduleRepository.Setup(repository => repository.Exists(DayOfWeek.Saturday, schedule.Schedule.ElementAt(5).BranchId).Result).Returns(true);

        var branchService = new ScheduleService(scheduleRepository.Object);

        var createdResponse = await branchService.Add(schedule);

        Assert.True(createdResponse.CreatedSchedule.Count.Equals(5));
        Assert.True(createdResponse.ExistentSchedule.ElementAt(0).Equals(schedule.Schedule.Where(schedule => schedule.WeekDay.Equals(DayOfWeek.Saturday)).FirstOrDefault()));
    }

    [Fact]
    public async void ShouldBeAbleToUpdateASchedule()
    {
        var schedule = ScheduleUpdateRequestDTOBuilder.Build();

        var scheduleAdded = new HashSet<Schedule>();
        var updatedSchedule = new HashSet<Schedule>();


        var scheduleRepository = new Mock<IScheduleRepository>();
        scheduleRepository.Setup(repository => repository.Add(It.IsAny<Schedule>()))
            .Callback<Schedule>(day =>
            {
                scheduleAdded.Add(day);
            });
        scheduleRepository.Setup(repository => repository.Update(It.IsAny<Schedule>()))
            .Callback<Schedule>(day =>
            {
                updatedSchedule.Add(day);
            });
        scheduleRepository.Setup(repository => repository.Exists(It.IsAny<DayOfWeek>(), It.IsAny<Guid>()).Result).Returns(true);

        var scheduleService = new ScheduleService(scheduleRepository.Object);

        await scheduleService.Update(schedule);

        Assert.True(scheduleAdded.Count.Equals(0));
        Assert.True(updatedSchedule.Count.Equals(schedule.Schedule.Count));

        foreach (var day in updatedSchedule)
        {
            Assert.Contains(day, schedule.Schedule);
        }
    }

    [Fact]
    public async void ShouldBeCreateAScheduleIfNotExistsAndUpdateTheOthers()
    {
        var schedule = ScheduleUpdateRequestDTOBuilder.Build();
        var scheduleAdded = new HashSet<Schedule>();
        var scheduleUpdated = new HashSet<Schedule>();

        var scheduleRepository = new Mock<IScheduleRepository>();
        scheduleRepository.Setup(repository => repository.Add(It.IsAny<Schedule>()))
            .Callback<Schedule>(day =>
            {
                scheduleAdded.Add(day);
            });
        scheduleRepository.Setup(repository => repository.Update(It.IsAny<Schedule>()))
            .Callback<Schedule>(day =>
            {
                scheduleUpdated.Add(day);
            });
        scheduleRepository.Setup(repository => repository.Exists(schedule.Schedule.First().WeekDay, schedule.Schedule.First().BranchId).Result).Returns(true);

        var scheduleService = new ScheduleService(scheduleRepository.Object);

        await scheduleService.Update(schedule);

        Assert.True(scheduleAdded.Count.Equals(schedule.Schedule.Count - 1));
        Assert.True(scheduleUpdated.Count.Equals(1));

        foreach (var day in scheduleUpdated)
        {
            Assert.Contains(day, schedule.Schedule);
        }

        foreach (var day in scheduleAdded)
        {
            Assert.Contains(day, schedule.Schedule);
        }
    }

    [Fact]
    public void ShouldBeAbleToGetAScheduleByDayOfWeek()
    {
        var schedule = new Schedule(Guid.NewGuid(), DateTime.UtcNow, DateTime.UtcNow.AddHours(1), DayOfWeek.Wednesday);

        var scheduleRepository = new Mock<IScheduleRepository>();
        scheduleRepository.Setup(repository => repository.GetByDay(schedule.BranchId, schedule.WeekDay).Result).Returns(schedule);

        var scheduleService = new ScheduleService(scheduleRepository.Object);

        var scheduleResponse = scheduleService.GetByDay(schedule.BranchId, schedule.WeekDay).Result;

        Assert.True(scheduleResponse.Items.Count.Equals(1));
        Assert.Contains(schedule, scheduleResponse.Items);
    }

    [Fact]
    public void ShouldReturnAEmptyListWhenAScheduleDoesntExists()
    {
        var schedule = new Schedule(Guid.NewGuid(), DateTime.UtcNow, DateTime.UtcNow.AddHours(2), DayOfWeek.Monday);

        var scheduleRepository = new Mock<IScheduleRepository>();

        var scheduleService = new ScheduleService(scheduleRepository.Object);

        var scheduleResponse = scheduleService.GetByDay(schedule.BranchId, schedule.WeekDay).Result;

        Assert.True(scheduleResponse.Items.Count.Equals(0));
    }

    [Fact]
    public async void ShouldBeAbleToRetrieveTheIntireScheduleRegistered()
    {
        var branchId = Guid.NewGuid();
        var schedule = new HashSet<Schedule>()
        {
            new Schedule(branchId, DateTime.UtcNow, DateTime.UtcNow.AddHours(2), DayOfWeek.Monday),
            new Schedule(branchId, DateTime.UtcNow, DateTime.UtcNow.AddHours(3), DayOfWeek.Tuesday),
            new Schedule(branchId, DateTime.UtcNow, DateTime.UtcNow.AddHours(1), DayOfWeek.Wednesday),
        };

        var scheduleRepository = new Mock<IScheduleRepository>();
        scheduleRepository.Setup(repository => repository.GetAll(branchId).Result).Returns(schedule);

        var scheduleService = new ScheduleService(scheduleRepository.Object);

        var scheduleResponse = await scheduleService.GetAll(branchId);

        Assert.True(scheduleResponse.Items.Count.Equals(3));
        
        foreach( var scheduleItem in scheduleResponse.Items )
        {
            Assert.Contains(scheduleItem, schedule);
        }
    }
}
