﻿using Domain.Exceptions;
using Domain.Exceptions.Messages;

namespace DomainTest.Entities.Barber.Services;

public class ServiceTest
{
    [Theory]
    [InlineData("0", "0")]
    [InlineData("0", "-1")]
    [InlineData("0", "1")]
    [InlineData("1", "0")]
    [InlineData("-1", "0")]
    [InlineData("1", "-1")]
    [InlineData("-1", "1")]
    [InlineData("-1", "-1")]
    public void ShouldNotBeAbleToSetNonPositivesPrices(string price, string promotionalPrice)
    {
        var service = ServiceBuilder.Build();
        
        var exception = Assert.Throws<ServiceException>(() => service.SetPrices(Decimal.Parse(price), Decimal.Parse(promotionalPrice)));

        Assert.True(exception.Message.Equals(ServiceExceptionMessagesResource.PRICE_MUST_BE_GRATHER_THEN_ZERO));
    }

    [Fact]
    public void ShouldNotBeAbleToSetPromotionalPriceGreaterThanRegularPrice()
    {
        var service = ServiceBuilder.Build();
        
        var exception = Assert.Throws<ServiceException>(() => service.SetPrices((decimal) 10, (decimal) 11));
        Assert.True(exception.Message.Equals(ServiceExceptionMessagesResource.PROMOTIONAL_PRICE_CANT_BE_GREATER_THEN_REGULAR_PRICE));
    }
}
