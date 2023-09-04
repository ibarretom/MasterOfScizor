using Domain.Entities.Barbers;

namespace Infra.Repositories.Company;

internal interface IScheduleRepository
{
    Task Add(Schedule schedule, Guid BranchId);
    Task Update(Schedule schedule, Guid BranchId);
    Task<Schedule> GetByDay(Guid branchId, DayOfWeek dayOfWeek);
    Task<HashSet<Schedule>> GetAll(Guid branchId);
    Task<bool> Exists(DayOfWeek dayOfWeek, Guid branchId);
}
