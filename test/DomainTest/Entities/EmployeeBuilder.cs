using Bogus;
using Bogus.Extensions.Brazil;
using Domain.Entities;

namespace DomainTest.Entities;

internal class EmployeeBuilder
{
    public static Employee Build()
    {
        var faker = new Faker();

        return new Employee(Guid.NewGuid(), faker.Random.Bool(), faker.Image.PicsumUrl(), faker.Person.Cpf(), faker.Person.FullName, faker.Internet.Email(), faker.Phone.PhoneNumber());
    }

    public static Employee Build(Guid branchId)
    {
        var faker = new Faker();

        return new Employee(branchId, faker.Random.Bool(), faker.Image.PicsumUrl(), faker.Person.Cpf(), faker.Person.FullName, faker.Internet.Email(), faker.Phone.PhoneNumber());
    }
}
