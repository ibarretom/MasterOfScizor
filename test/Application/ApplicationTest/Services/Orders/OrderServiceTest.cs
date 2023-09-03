using Application.Services.Orders;
using Domain.Entities.Barbers.Service;
using Domain.Entities.Orders;
using Domain.Exceptions;
using Domain.Exceptions.Messages;
using Domain.ValueObjects.Enums;
using DomainTest.Entities.Barbers.Services;
using DomainTest.ValueObjects.DTO;
using Infra.Repositories.Company;
using Moq;

namespace ApplicationTest.Services.Orders;

public class OrderServiceTest
{
    [Fact]
    public async Task ShouldBeAbleToCreateAOrder()
    {
        var orderDTO = OrderRequestDTOBuilder.Build();

        var services = orderDTO.ServiceId.Select(serviceId => ServiceBuilder.Build(orderDTO.BranchId)).ToList();

        var servicesId = services.Select(service => service.Id).ToList();

        orderDTO = OrderRequestDTOBuilder.Build(servicesId);

        var orderCreated = new Order(Guid.Empty, Guid.Empty, new List<Guid>(), Guid.Empty, OrderStatus.Pending, DateTime.UtcNow);

        var orderRepository = new Mock<IOrderRepository>();
        orderRepository.Setup(repository => repository.Create(It.IsAny<Order>()))
            .Callback<Order>(order =>
             {
                orderCreated = order;
            }).Returns(Task.CompletedTask);

        var currentService = ServiceBuilder.Build();

        var serviceRepository = new Mock<IServiceRepository>();
        serviceRepository.Setup(repository => repository.GetById(It.IsAny<Guid>(), It.IsAny<Guid>()).Result)
            .Callback<Guid, Guid>((branchId, serviceId) =>
            {
                currentService = ServiceBuilder.Build();
                services.RemoveAt(0);
            })
            .Returns(currentService);

        var orderService = new OrderService(orderRepository.Object, serviceRepository.Object);

        await orderService.Create(orderDTO);

        Assert.Contains(orderDTO.ServiceId, serviceId => orderCreated.ServiceId.Contains(serviceId));
        Assert.True(orderDTO.UserId.Equals(orderCreated.UserId), "User Id does not matches");
        Assert.True(orderDTO.WorkerId.Equals(orderCreated.WorkerId), "Worker Id does not matches");
        Assert.True(orderDTO.BranchId.Equals(orderCreated.BranchId), "Branch Id does not matches");
        Assert.True(orderDTO.ScheduleTime.Equals(orderCreated.ScheduleTime), "Schedule Time does not Matches");
        Assert.True(orderCreated.Status.Equals(OrderStatus.Accepted), "Order status is not Accepted");
    }

    [Fact]
    public async Task ShouldThrowAnErrorWhenServiceIdDoesNotExists()
    {
        var services = new List<Guid>() { Guid.NewGuid()};

        var orderDTO = OrderRequestDTOBuilder.Build(services);

        var orderRepository = new Mock<IOrderRepository>();

        var serviceRepository = new Mock<IServiceRepository>();

        var orderService = new OrderService(orderRepository.Object, serviceRepository.Object);

        var exception = await Assert.ThrowsAsync<ServiceException>(() => orderService.Create(orderDTO));

        Assert.True(exception.Message.Equals(string.Format(ServiceExceptionMessagesResource.SERVICE_DOES_NOT_EXISTS, services.First())));
    }
}
