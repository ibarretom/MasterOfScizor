using Bogus;
using Bogus.Extensions.Brazil;
using Domain.ValueObjects.Addresses;
using Domain.ValueObjects.DTO.Barber;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("ApplicationTest")]
namespace DomainTest.ValueObjects.DTO;

internal static class CreateCompanyDTOBuilder
{
    public static CreateCompanyRequestDTO Build()
    {
        var faker = new Faker();
        var cnpj = faker.Company.Cnpj();
        var branchDTO = new BranchDTO(
                cnpj.Split("/").ElementAt(1).Split("-").ElementAt(0),
                new Address(Guid.NewGuid(),
                new AddressIdentifier(faker.Address.BuildingNumber(), faker.Address.SecondaryAddress())),
                faker.Phone.PhoneNumber(),
                faker.Internet.Email()
                );

        return new CreateCompanyRequestDTO(Guid.NewGuid(), cnpj.Split("/").First(), faker.Company.CompanyName(), branchDTO);
    }
}
