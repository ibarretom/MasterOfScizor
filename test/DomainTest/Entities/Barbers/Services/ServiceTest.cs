using Domain.Entities.Barbers.Service;
using Domain.Exceptions;
using Domain.Exceptions.Messages;

namespace DomainTest.Entities.Barbers.Services;

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
    public void ShouldNotBeAbleToCreateAServiceWithNonPositivePrices(string price, string promotionalPrice)
    {
        var exception = Assert.Throws<ServiceException>(() => new Service(Guid.NewGuid(), new Category("Any", Guid.NewGuid()), "Any", "Any", Decimal.Parse(price), Decimal.Parse(promotionalPrice), false, false, TimeSpan.FromMinutes(1)));
        Assert.True(exception.Message.Equals(ServiceExceptionMessagesResource.PRICE_MUST_BE_GRATHER_THEN_ZERO));
    }

    [Fact]
    public void ShouldNotBeAbleToCreateAServiceWithPromotionalPriceGreaterThenRegularPrice()
    {
        var exception = Assert.Throws<ServiceException>(() => new Service(Guid.NewGuid(), new Category("Any", Guid.NewGuid()), "Any", "Any", (decimal) 10, (decimal) 11, false, false, TimeSpan.FromMinutes(1)));
        Assert.True(exception.Message.Equals(ServiceExceptionMessagesResource.PROMOTIONAL_PRICE_CANT_BE_GREATER_THEN_REGULAR_PRICE));
    }

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

    [Fact]
    public void ShouldNotBeAbleToCreateAServiceWithInvalidBranchId()
    {
        var exception = Assert.Throws<ServiceException>(() => new Service(Guid.Empty, new Category("Any", Guid.NewGuid()), "Any", "Any", (decimal) 10, (decimal) 11, false, false, TimeSpan.FromMinutes(1)));
        Assert.True(exception.Message.Equals(ServiceExceptionMessagesResource.BRANCH_ID_MUST_BE_VALID));
    }
}
