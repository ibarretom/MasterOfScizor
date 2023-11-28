using Bogus;
using Bogus.Extensions.Brazil;
using Domain.ValueObjects.Addresses;
using Domain.ValueObjects.DTO.Barber;
using DomainTest.Entities;
using DomainTest.ValueObjects.Addresses;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("ApplicationTest")]
namespace DomainTest.ValueObjects.DTO;

internal static class CreateCompanyDTOBuilder
{
    public static CreateCompanyRequestDTO Build()
    {
        var faker = new Faker();
        var cnpj = faker.Company.Cnpj();
        var address = AddressLocalizationBuilder.Build();
        var branchDTO = new BranchDTO(
                cnpj.Split("/").ElementAt(1).Split("-").ElementAt(0),
                new Address(address,
                new AddressIdentifier(faker.Address.BuildingNumber(), faker.Address.SecondaryAddress())),
                faker.Phone.PhoneNumber(),
                faker.Internet.Email()
                );

        var user = UserBuilder.Build();

        return new CreateCompanyRequestDTO(user, cnpj.Split("/").First(), faker.Company.CompanyName(), branchDTO);
    }
}
