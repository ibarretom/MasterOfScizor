using Domain.Entities.Barbers;

namespace Domain.ValueObjects.DTO.Barber;

internal class ScheduleCreateResponseDTO
{
    public HashSet<ScheduleRequestDTO> CreatedSchedule { get; set; } = new HashSet<ScheduleRequestDTO> { };
    public HashSet<ScheduleRequestDTO> ExistentSchedule { get; set; } = new HashSet<ScheduleRequestDTO> { };

    public ScheduleCreateResponseDTO()
    {
    }
}
