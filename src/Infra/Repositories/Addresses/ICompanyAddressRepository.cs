
using Domain.ValueObjects.Addresses;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Application")]
namespace Infra.Repositories.Addresses;

internal interface ICompanyAddressRepository
{
    Task Create(Address companyAddress);
}
