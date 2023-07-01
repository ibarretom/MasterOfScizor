using Bogus;
using Domain.ValueObjects.DTO;

namespace DomainTest.ValueObjects.DTO;

public class SignInRequestDTOBuilder
{
    public static SignInRequestDTO Build(string credentialType)
    {
        var faker = new Faker();

        var credential = credentialType == "email" ? faker.Internet.Email() : faker.Phone.PhoneNumber();
        return new SignInRequestDTO(credential, faker.Internet.Password());
    }
}
