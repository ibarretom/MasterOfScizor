using Domain.Entities.Barbers.Service;

namespace Infra.Repositories.Company;

internal interface IServiceRepository
{
    Task Add(Service service);
    Task Update(Service service);
    Task<bool> Exists(Guid branchId, string name, Guid categoryId);
    Task<Service> GetById(Guid branchId, Guid serviceId);
    Task<IEnumerable<Service>> GetAll(Guid branchId);
    Task Disable(Guid branchId, Guid serviceId);
}
