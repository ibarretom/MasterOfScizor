using Domain.Entities.Barbers;
using Domain.ValueObjects.DTO.Barber;
using Infra.Repositories.Company;
using Infra.Repositories.CompanyRepository;
using System.Reflection.Metadata.Ecma335;

namespace Application.Services.Branches;

internal class ScheduleService
{
    private readonly IScheduleRepository _scheduleRepository;
   
    public ScheduleService(IScheduleRepository scheduleRepository)
    {
        _scheduleRepository = scheduleRepository;
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
}
