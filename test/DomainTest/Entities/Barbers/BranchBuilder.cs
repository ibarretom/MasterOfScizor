﻿using Bogus;
using Bogus.Extensions.Brazil;
using Domain.Entities.Barbers;
using Domain.ValueObjects.Addresses;
using DomainTest.ValueObjects.Addresses;

namespace DomainTest.Entities.Barbers;

internal class BranchBuilder
{

    public static Branch Build(Configuration configuration, bool isOpened = false)
    {
        var faker = new Faker();

        var addressIdentifier = new AddressIdentifier(faker.Address.BuildingNumber(), faker.Address.BuildingNumber());
        var localization = AddressLocalizationBuilder.Build();
        var address = new Address(localization, addressIdentifier);
  
        return new Branch(faker.Company.Cnpj(), address, faker.Phone.PhoneNumber(), faker.Internet.Email(), isOpened, configuration);
    }
}
