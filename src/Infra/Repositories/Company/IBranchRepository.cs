using Domain.Entities.Barbers;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Application")]
namespace Infra.Repositories.CompanyRepository;
internal interface IBranchRepository
{
    Task Create(Branch branch);
    Task<bool> Exists(Guid id);
    Task<Branch> GetBy(Guid id);
    Task Update(Branch branch);
}
