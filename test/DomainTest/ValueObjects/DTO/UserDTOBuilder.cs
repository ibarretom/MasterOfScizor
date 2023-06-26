using Bogus;
using Domain.ValueObjects.DTO;

namespace DomainTest.ValueObjects.DTO;

public static class UserDTOBuilder
{
    public static UserRequestDTO Build()
    {
        var faker = new Faker();

        return new UserRequestDTO(faker.Name.FullName(), faker.Internet.Email(), faker.Phone.PhoneNumberFormat(faker.Random.Int(0, 19)), faker.Internet.Password());
    }
}
