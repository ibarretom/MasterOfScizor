using Domain.Entities;
using Domain.Entities.Barbers;
using Domain.Exceptions;
using Domain.Exceptions.Messages;
using Domain.Services.Barbers;
using Domain.ValueObjects.DTO.Barber;
using Infra.Repositories.Company;
using Infra.Repositories.CompanyRepository;

namespace Application.Services.Branches;

internal class ScheduleService
{
    private readonly IScheduleRepository _scheduleRepository;
    private readonly IScheduler _scheduler;
    private readonly IBranchRepository _branchRepository;
   
    public ScheduleService(IScheduleRepository scheduleRepository, IScheduler scheduler, IBranchRepository branchRepository)
    {
        _scheduleRepository = scheduleRepository;
        _scheduler = scheduler;
        _branchRepository = branchRepository;
    }

    public async Task<ScheduleCreateResponseDTO> Add(ScheduleCreateRequestDTO schedule)
    {
        var scheduleResponse = new ScheduleCreateResponseDTO();

        foreach (var day in schedule.Schedule)
        {
            if (await _scheduleRepository.Exists(day.WeekDay, day.BranchId))
            {
                scheduleResponse.ExistentSchedule.Add(day);
                continue;
            }

            await _scheduleRepository.Add(new Schedule(day.StartTime, day.EndTime, day.WeekDay), day.BranchId);
            scheduleResponse.CreatedSchedule.Add(day);
        }

        return scheduleResponse;
    }

    public async Task Update(ScheduleUpdateRequestDTO schedules)
    {
        foreach (var day in schedules.Schedule)
        {
            if (!(await _scheduleRepository.Exists(day.WeekDay, day.BranchId)))
            {
                await _scheduleRepository.Add(new Schedule(day.StartTime, day.EndTime, day.WeekDay), day.BranchId);
                continue;
            }

            await _scheduleRepository.Update(new Schedule(day.StartTime, day.EndTime, day.WeekDay), day.BranchId);
        }
    }

    public async Task<ScheduleGetResponseDTO> GetByDay(Guid branchId, DayOfWeek day)
    {
        var schedule = await _scheduleRepository.GetByDay(branchId, day);

        if (schedule is null)
            return new ScheduleGetResponseDTO(new HashSet<Schedule>());

        return new ScheduleGetResponseDTO(new HashSet<Schedule>() { schedule });
    }

    public async Task<ScheduleGetResponseDTO> GetAll(Guid branchId)
    {
        var schedule = await _scheduleRepository.GetAll(branchId);

        return new ScheduleGetResponseDTO(schedule);
    }

    public async Task<HashSet<TimeOnly>> GetAllAvailableTimes(DateTime day, Employee employee)
    {
        var schedule = await _scheduleRepository.GetByDay(employee.BranchId, day.DayOfWeek);

        if (schedule is null)
            return new HashSet<TimeOnly>();

        var branch = await _branchRepository.GetBy(employee.BranchId) ?? throw new CompanyException(CompanyExceptionMessagesResource.BRANCH_NOT_FOUND);

        return _scheduler.GetAllPossible(schedule, branch.Configuration);
    }
}
