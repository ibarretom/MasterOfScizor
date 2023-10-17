using Bogus;
using Domain.Entities;
using Domain.ValueObjects.Enums;

namespace DomainTest.Entities;

public class UserBuilder
{
    public static User Build()
    {
        var faker = new Faker();

        var user = new User(faker.Person.FullName, faker.Internet.Email(), faker.Phone.PhoneNumber());
        user.AddRoles(UserRole.Customer);

        return user;
    }
}