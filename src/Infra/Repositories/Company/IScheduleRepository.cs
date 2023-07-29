using Domain.Entities.Barbers;

namespace Infra.Repositories.Company;

internal interface IScheduleRepository
{
    Task Add(Schedule schedule);
    Task Update(Schedule schedule);
    Task<bool> Exists(DayOfWeek dayOfWeek, Guid branchId);
}
