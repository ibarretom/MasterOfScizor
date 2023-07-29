using Application.Services.Branches;
using Domain.Entities.Barbers;
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
}
