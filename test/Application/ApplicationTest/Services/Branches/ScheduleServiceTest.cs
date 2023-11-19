using Application.Services.Branches;
using Domain.Entities.Barbers;
using Domain.Exceptions;
using Domain.Exceptions.Messages;
using Domain.Services.Barbers;
using DomainTest.Entities;
using DomainTest.Entities.Barbers;
using DomainTest.ValueObjects.DTO;
using Infra.Repositories.Company;
using Infra.Repositories.CompanyRepository;
using Moq;

namespace ApplicationTest.Services.Branches;

public class ScheduleServiceTest
{
    [Fact]
    public async void ShouldBeAbleToAddASchedule()
    {
        var schedule = ScheduleRequestDTOBuilder.Build();

        var scheduleRepository = new Mock<IScheduleRepository>();

        scheduleRepository.Setup(repository => repository.Add(It.IsAny<Schedule>(), It.IsAny<Guid>()));

        var branchRepository = new Mock<IBranchRepository>();

        var scheduler = new Mock<IScheduler>();

        var scheduleService = new ScheduleService(scheduleRepository.Object, scheduler.Object, branchRepository.Object);

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
        scheduleRepository.Setup(repository => repository.Add(It.IsAny<Schedule>(), It.IsAny<Guid>()));

        scheduleRepository.Setup(repository => repository.Exists(DayOfWeek.Saturday, schedule.Schedule.ElementAt(5).BranchId).Result).Returns(true);

        var branchRepository = new Mock<IBranchRepository>();

        var scheduler = new Mock<IScheduler>();

        var scheduleService = new ScheduleService(scheduleRepository.Object, scheduler.Object, branchRepository.Object);

        var createdResponse = await scheduleService.Add(schedule);

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
        scheduleRepository.Setup(repository => repository.Add(It.IsAny<Schedule>(), It.IsAny<Guid>()))
            .Callback<Schedule, Guid>((day, _)=>
            {
                scheduleAdded.Add(day);
            });
        scheduleRepository.Setup(repository => repository.Update(It.IsAny<Schedule>(), It.IsAny<Guid>()))
            .Callback<Schedule, Guid>((day, _)=>
            {
                updatedSchedule.Add(day);
            });
        scheduleRepository.Setup(repository => repository.Exists(It.IsAny<DayOfWeek>(), It.IsAny<Guid>()).Result).Returns(true);

        var branchRepository = new Mock<IBranchRepository>();

        var scheduler = new Mock<IScheduler>();

        var scheduleService = new ScheduleService(scheduleRepository.Object, scheduler.Object, branchRepository.Object);

        await scheduleService.Update(schedule);

        Assert.True(scheduleAdded.Count.Equals(0));
        Assert.True(updatedSchedule.Count.Equals(schedule.Schedule.Count));

        foreach (var day in updatedSchedule)
        {
            Assert.Contains(day, schedule.Schedule.Select(day => new Schedule(day.StartTime, day.EndTime, day.WeekDay)));
        }
    }

    [Fact]
    public async void ShouldBeCreateAScheduleIfNotExistsAndUpdateTheOthers()
    {
        var schedule = ScheduleUpdateRequestDTOBuilder.Build();
        var scheduleAdded = new HashSet<Schedule>();
        var scheduleUpdated = new HashSet<Schedule>();

        var scheduleRepository = new Mock<IScheduleRepository>();
        scheduleRepository.Setup(repository => repository.Add(It.IsAny<Schedule>(), It.IsAny<Guid>()))
            .Callback<Schedule, Guid>((day, _) =>
            {
                scheduleAdded.Add(day);
            });
        scheduleRepository.Setup(repository => repository.Update(It.IsAny<Schedule>(), It.IsAny<Guid>()))
            .Callback<Schedule, Guid>((day, _) =>
            {
                scheduleUpdated.Add(day);
            });
        scheduleRepository.Setup(repository => repository.Exists(schedule.Schedule.First().WeekDay, schedule.Schedule.First().BranchId).Result).Returns(true);

        var branchRepository = new Mock<IBranchRepository>();

        var scheduler = new Mock<IScheduler>();

        var scheduleService = new ScheduleService(scheduleRepository.Object, scheduler.Object, branchRepository.Object);

        await scheduleService.Update(schedule);

        Assert.True(scheduleAdded.Count.Equals(schedule.Schedule.Count - 1));
        Assert.True(scheduleUpdated.Count.Equals(1));

        foreach (var day in scheduleUpdated)
        {
            Assert.Contains(day, schedule.Schedule.Select(day => new Schedule(day.StartTime, day.EndTime, day.WeekDay)));
        }

        foreach (var day in scheduleAdded)
        {
            Assert.Contains(day, schedule.Schedule.Select(day => new Schedule(day.StartTime, day.EndTime, day.WeekDay)));
        }
    }

    [Fact]
    public async Task ShouldBeAbleToGetAScheduleByDayOfWeek()
    {
        var branchId = Guid.NewGuid();
        var now = DateTime.UtcNow;
        var schedule = new Schedule(new TimeOnly(now.Hour, now.Minute), new TimeOnly(now.Hour, now.Minute).AddHours(1), DayOfWeek.Wednesday);

        var scheduleRepository = new Mock<IScheduleRepository>();
        scheduleRepository.Setup(repository => repository.GetByDay(branchId, schedule.WeekDay).Result).Returns(schedule);

        var branchRepository = new Mock<IBranchRepository>();

        var scheduler = new Mock<IScheduler>();

        var scheduleService = new ScheduleService(scheduleRepository.Object, scheduler.Object, branchRepository.Object);

        var scheduleResponse = await scheduleService.GetByDay(branchId, schedule.WeekDay);

        Assert.True(scheduleResponse.Items.Count.Equals(1));
        Assert.Contains(schedule, scheduleResponse.Items);
    }

    [Fact]
    public void ShouldReturnAEmptyListWhenAScheduleDoesNotExists()
    {
        var branchId = Guid.NewGuid();
        var now = DateTime.UtcNow;
        var schedule = new Schedule(new TimeOnly(now.Hour, now.Minute), new TimeOnly(now.Hour, now.Minute).AddHours(2), DayOfWeek.Monday);

        var scheduleRepository = new Mock<IScheduleRepository>();

        var branchRepository = new Mock<IBranchRepository>();

        var scheduler = new Mock<IScheduler>();

        var scheduleService = new ScheduleService(scheduleRepository.Object, scheduler.Object, branchRepository.Object);

        var scheduleResponse = scheduleService.GetByDay(branchId, schedule.WeekDay).Result;

        Assert.True(scheduleResponse.Items.Count.Equals(0));
    }

    [Fact]
    public async Task ShouldBeAbleToRetrieveTheEntireScheduleRegistered()
    {
        var branchId = Guid.NewGuid();
        var now = DateTime.UtcNow;
        var schedule = new HashSet<Schedule>()
        {
            new Schedule(new TimeOnly(now.Hour, now.Minute), new TimeOnly(now.Hour, now.Minute).AddHours(2), DayOfWeek.Monday),
            new Schedule(new TimeOnly(now.Hour, now.Minute), new TimeOnly(now.Hour, now.Minute).AddHours(3), DayOfWeek.Tuesday),
            new Schedule(new TimeOnly(now.Hour, now.Minute), new TimeOnly(now.Hour, now.Minute).AddHours(1), DayOfWeek.Wednesday),
        };

        var scheduleRepository = new Mock<IScheduleRepository>();
        scheduleRepository.Setup(repository => repository.GetAll(branchId).Result).Returns(schedule);

        var branchRepository = new Mock<IBranchRepository>();

        var scheduler = new Mock<IScheduler>();

        var scheduleService = new ScheduleService(scheduleRepository.Object, scheduler.Object, branchRepository.Object);

        var scheduleResponse = await scheduleService.GetAll(branchId);

        Assert.True(scheduleResponse.Items.Count.Equals(3));
        
        foreach( var scheduleItem in scheduleResponse.Items )
        {
            Assert.Contains(scheduleItem, schedule);
        }
    }

    [Fact]
    public async Task ShouldBeAbleToRetrieveAllTimesAvailableForAWorker()
    {
        var branch = BranchBuilder.Build(ConfigurationBuilder.BuildWithScheduleWithNoDelay());

        var now = DateTime.UtcNow;

        var schedule = new Schedule(new TimeOnly(now.Hour, now.Minute), new TimeOnly(now.Hour, now.Minute).AddHours(2), now.DayOfWeek);

        var scheduleRepository = new Mock<IScheduleRepository>();
        scheduleRepository.Setup(repository => repository.GetByDay(branch.Id, now.DayOfWeek))
            .ReturnsAsync(schedule);

        var scheduler = new Mock<IScheduler>();

        var branchRepository = new Mock<IBranchRepository>();
        branchRepository.Setup(repository => repository.GetBy(branch.Id)).ReturnsAsync(branch);

        var scheduleService = new ScheduleService(scheduleRepository.Object, scheduler.Object, branchRepository.Object);

        var employee = EmployeeBuilder.Build(branch.Id);

        await scheduleService.GetAllAvailableTimes(now, employee);

        scheduler.Verify(scheduler => scheduler.GetAllPossible(schedule, It.IsAny<Configuration>()));
        branchRepository.Verify(branchRepository => branchRepository.GetBy(branch.Id));
    }

    [Fact]
    public async Task ShouldReturnEmptyHashSetWhenNoScheduleIsFound()
    {
        var now = DateTime.UtcNow;

        var scheduleRepository = new Mock<IScheduleRepository>();

        var scheduler = new Mock<IScheduler>();

        var branchRepository = new Mock<IBranchRepository>();

        var scheduleService = new ScheduleService(scheduleRepository.Object, scheduler.Object, branchRepository.Object);

        var employee = EmployeeBuilder.Build();

        var allSchedule = await scheduleService.GetAllAvailableTimes(now, employee);

        Assert.Empty(allSchedule);
    }

    [Fact]
    public async Task ShouldThrowsWhenNoBranchIsFound()
    {
        var branch = BranchBuilder.Build(ConfigurationBuilder.BuildWithScheduleWithNoDelay());

        var now = DateTime.UtcNow;

        var schedule = new Schedule(new TimeOnly(now.Hour, now.Minute), new TimeOnly(now.Hour, now.Minute).AddHours(2), now.DayOfWeek);

        var scheduleRepository = new Mock<IScheduleRepository>();
        scheduleRepository.Setup(repository => repository.GetByDay(branch.Id, now.DayOfWeek))
            .ReturnsAsync(schedule);

        var scheduler = new Mock<IScheduler>();

        var branchRepository = new Mock<IBranchRepository>();

        var scheduleService = new ScheduleService(scheduleRepository.Object, scheduler.Object, branchRepository.Object);

        var employee = EmployeeBuilder.Build(branch.Id);

        var exception = await Assert.ThrowsAsync<CompanyException>(async () => await scheduleService.GetAllAvailableTimes(now, employee));

        Assert.Equal(CompanyExceptionMessagesResource.BRANCH_NOT_FOUND, exception.Message);
    }

}
