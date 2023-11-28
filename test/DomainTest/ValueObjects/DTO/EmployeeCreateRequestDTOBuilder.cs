using Bogus;
using Bogus.Extensions.Brazil;
using Domain.ValueObjects.DTO;
using Domain.ValueObjects.Enums;
using DomainTest.Entities.Barbers;

namespace DomainTest.ValueObjects.DTO;

internal class EmployeeCreateRequestDTOBuilder
{
    public static EmployeeCreateRequestDTO Build()
    {
        var faker = new Faker();
        var roles = Enumerable.Range(1, 3).Select(r => faker.Random.Enum<UserRole>()).Where(role => role != UserRole.Customer).ToHashSet();

        var branch = BranchBuilder.Build(ConfigurationBuilder.BuildRandom());
        return new EmployeeCreateRequestDTO(branch, faker.Name.FullName(), faker.Internet.Email(), faker.Phone.PhoneNumber(), faker.Internet.Password(), faker.Image.PicsumUrl(), faker.Random.Bool(), faker.Person.Cpf(), roles);
    }
}
