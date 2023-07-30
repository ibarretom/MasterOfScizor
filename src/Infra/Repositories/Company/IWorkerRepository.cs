using Domain.Entities;

namespace Infra.Repositories.Company;

internal interface IWorkerRepository
{
    Task<bool> Exists(Guid branchId, string document, string phone, string email);
    Task Add(Employee worker);
}
