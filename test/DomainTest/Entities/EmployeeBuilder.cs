using Bogus;
using Bogus.Extensions.Brazil;
using Domain.Entities;
using Domain.Entities.Barbers;
using DomainTest.Entities.Barbers;

namespace DomainTest.Entities;

internal class EmployeeBuilder
{
    public static Employee Build()
    {
        var faker = new Faker();
        var branch = BranchBuilder.Build(ConfigurationBuilder.BuildRandom());

        return new Employee(branch, faker.Random.Bool(), faker.Image.PicsumUrl(), faker.Person.Cpf(), faker.Person.FullName, faker.Internet.Email(), faker.Phone.PhoneNumber());
    }

    public static Employee Build(Branch branch)
    {
        var faker = new Faker();

        return new Employee(branch, faker.Random.Bool(), faker.Image.PicsumUrl(), faker.Person.Cpf(), faker.Person.FullName, faker.Internet.Email(), faker.Phone.PhoneNumber());
    }
}
