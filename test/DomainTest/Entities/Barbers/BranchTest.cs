using Domain.Entities.Barbers;
using Domain.Exceptions;
using Domain.Exceptions.Messages;

namespace DomainTest.Entities.Barbers;

public class BranchTest
{
    [Fact]
    public void ShouldBeAbleToAddALunchTimeForAWorker()
    {
        var configuration = ConfigurationBuilder.BuildRandom();
        var branch = BranchBuilder.Build(configuration);
        branch.AddSchedule(new Schedule(new TimeOnly(22, 0), new TimeOnly(2, 0), DayOfWeek.Saturday));
        var worker = EmployeeBuilder.Build();
        branch.AddEmployee(worker);

        var lunchTime = new Schedule(new TimeOnly(23, 0), new TimeOnly(0, 0), DayOfWeek.Saturday);

        branch.AddEmployeeLunchInterval(lunchTime, worker.Id);

        Assert.Contains(lunchTime, worker.LunchInterval);
    }

    [Fact]
    public void ShouldNotBeAbleToAddALunchTimeForAWorkerWhenLunchTimeIsNotIncludedInSchedule()
    {
        var configuration = ConfigurationBuilder.BuildRandom();
        var branch = BranchBuilder.Build(configuration);
        branch.AddSchedule(new Schedule(new TimeOnly(22, 0), new TimeOnly(2, 0), DayOfWeek.Saturday));
        var worker = EmployeeBuilder.Build();
        branch.AddEmployee(worker);

        var lunchTime = new Schedule(new TimeOnly(21, 0), new TimeOnly(0, 0), DayOfWeek.Saturday);

        var exception = Assert.Throws<CompanyException>(() => branch.AddEmployeeLunchInterval(lunchTime, worker.Id));

        Assert.True(exception.Message.Equals(CompanyExceptionMessagesResource.INVALID_LUNCH_TIME_FOR_BRANCH));
    }

    [Fact]
    public void ShouldNotBeAbleToAddALunchTimeFOrAWorkerThatIsNotRegistered()
    {
        var configuration = ConfigurationBuilder.BuildRandom();
        var branch = BranchBuilder.Build(configuration);
        branch.AddSchedule(new Schedule(new TimeOnly(22, 0), new TimeOnly(2, 0), DayOfWeek.Saturday));
        var worker = EmployeeBuilder.Build();
        branch.AddEmployee(worker);

        var lunchTime = new Schedule(new TimeOnly(23, 0), new TimeOnly(0, 0), DayOfWeek.Saturday);

        var exception = Assert.Throws<CompanyException>(() => branch.AddEmployeeLunchInterval(lunchTime, Guid.NewGuid()));

        Assert.True(exception.Message.Equals(CompanyExceptionMessagesResource.EMPLOYEE_NOT_FOUND));
    }
}
