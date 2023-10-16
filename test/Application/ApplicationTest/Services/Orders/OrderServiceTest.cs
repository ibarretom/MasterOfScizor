using Application.Services.Orders;
using Domain.Entities.Barbers;
using Domain.Entities.Barbers.Service;
using Domain.Entities.Orders;
using Domain.Exceptions;
using Domain.Exceptions.Messages;
using Domain.ValueObjects.Enums;
using DomainTest.Entities.Barbers;
using DomainTest.Entities.Barbers.Services;
using DomainTest.ValueObjects.DTO;
using Infra.Repositories.Company;
using Infra.Repositories.CompanyRepository;
using Moq;

namespace ApplicationTest.Services.Orders;

public class OrderServiceTest
{
    [Fact]
    public async Task ShouldBeAbleToCreateAOrder()
    {
        var orderDTO = OrderRequestDTOBuilder.Build();

        var orderCreated = new Order(Guid.Empty, Guid.Empty, new List<Service>(), Guid.Empty, OrderStatus.Pending, DateTime.UtcNow);

        var orderRepository = new Mock<IOrderRepository>();
        orderRepository
            .Setup(repository => repository.Create(It.IsAny<Order>()))
            .Callback<Order>(order =>
             {
                orderCreated = order;
            })
            .Returns(Task.CompletedTask);
        orderRepository.Setup(repository => repository.GetBy(It.IsAny<Guid>(), It.IsAny<Guid>()).Result).Returns(new List<Order>());

        var currentService = ServiceBuilder.Build();

        var serviceRepository = new Mock<IServiceRepository>();
        serviceRepository.Setup(repository => repository.Exists(It.IsAny<Guid>(), It.IsAny<Guid>()).Result)
            .Returns(true);

        var configuration = ConfigurationBuilder.BuildRandom();
        var branch = BranchBuilder.Build(configuration);

        var branchRepository = new Mock<IBranchRepository>();
        branchRepository.Setup(repository => repository.GetBy(It.IsAny<Guid>()).Result).Returns(branch);

        var orderPolicy = new Mock<IOrderPolicy>();
        orderPolicy.Setup(repository => repository.IsAllowed(It.IsAny<Order>(), It.IsAny<List<Order>>(), It.IsAny<Branch>())).Returns(true);

        var orderTools = new Mock<IOrderTool>();
        orderTools.Setup(repository => repository.RelocateTime(It.IsAny<Order>(), It.IsAny<List<Order>>(), It.IsAny<Branch>())).Returns(orderDTO.ScheduleTime);

        var orderService = new OrderService(orderRepository.Object, serviceRepository.Object, branchRepository.Object, orderPolicy.Object, orderTools.Object);

        await orderService.Create(orderDTO);

        orderDTO.Services.ForEach(service =>
        {
            Assert.Contains(service, orderCreated.Services);
        });

        Assert.True(orderDTO.UserId.Equals(orderCreated.UserId), "User Id does not matches");
        Assert.True(orderDTO.WorkerId.Equals(orderCreated.WorkerId), "Worker Id does not matches");
        Assert.True(orderDTO.BranchId.Equals(orderCreated.BranchId), "Branch Id does not matches");
        Assert.True(orderDTO.ScheduleTime.Equals(orderCreated.ScheduleTime), "Schedule Time does not Matches");
        Assert.True(orderCreated.Status.Equals(OrderStatus.Accepted), "Order status is not Accepted");
    }

    [Fact]
    public async Task ShouldThrowAnErrorWhenServiceDoesNotExists()
    {
        var orderDTO = OrderRequestDTOBuilder.Build(1);

        var orderRepository = new Mock<IOrderRepository>();

        var serviceRepository = new Mock<IServiceRepository>();
        serviceRepository.Setup(serviceRepository => serviceRepository.Exists(It.IsAny<Guid>(), It.IsAny<Guid>()).Result)
            .Returns(false);
        var branchRepository = new Mock<IBranchRepository>();

        var orderPolicy = new Mock<IOrderPolicy>();

        var orderTool = new Mock<IOrderTool>();

        var orderService = new OrderService(orderRepository.Object, serviceRepository.Object, branchRepository.Object, orderPolicy.Object, orderTool.Object);

        var exception = await Assert.ThrowsAsync<ServiceException>(async () => await orderService.Create(orderDTO));

        Assert.True(exception.Message.Equals(string.Format(ServiceExceptionMessagesResource.SERVICE_DOES_NOT_EXISTS, orderDTO.Services.First().Id)), "Mensagens de erro não conferem.");
    }
}
