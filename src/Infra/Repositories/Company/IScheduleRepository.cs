using Domain.Entities.Barbers;

namespace Infra.Repositories.Company;

internal interface IScheduleRepository
{
    Task Add(Schedule schedule);
    Task Update(Schedule schedule);
    Task<Schedule> GetByDay(Guid branchId, DayOfWeek dayOfWeek);
    Task<HashSet<Schedule>> GetAll(Guid branchId);
    Task<bool> Exists(DayOfWeek dayOfWeek, Guid branchId);
}
