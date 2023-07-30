using Domain.Entities.Barbers;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Application")]
[assembly: InternalsVisibleTo("ApplicationTest")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]
namespace Infra.Repositories.CompanyRepository;

internal interface ICompanyRepository
{
    Task Create(Barber barber);
    Task Update(Barber barber);
    Task<Barber> GetById(Guid companyId);
    Task<Barber> GetByCompanyIdentifier(string companyIdentifier);
}
