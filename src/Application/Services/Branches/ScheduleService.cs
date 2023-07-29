using Domain.ValueObjects.DTO.Barber;
using Infra.Repositories.Company;

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
                scheduleResponse.ExistentSchedule.Add(day);

            await _scheduleRepository.Add(day);
            scheduleResponse.CreatedSchedule.Add(day);
        }

        return scheduleResponse;
    }
}
