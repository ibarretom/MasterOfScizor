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

        var createdSchedule = await scheduleService.AddSchedule(schedule);

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

        var createdResponse = await branchService.AddSchedule(schedule);

        Assert.True(createdResponse.CreatedSchedule.Count.Equals(6));
        Assert.True(createdResponse.ExistentSchedule.ElementAt(0).Equals(schedule.Schedule.Where(schedule => schedule.WeekDay.Equals(DayOfWeek.Saturday)).FirstOrDefault()));
    }
}
