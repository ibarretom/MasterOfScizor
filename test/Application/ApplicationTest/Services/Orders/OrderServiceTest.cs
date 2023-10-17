using Application.Services.Orders;
using Domain.Entities.Barbers;
using Domain.Entities.Barbers.Service;
using Domain.Entities.Orders;
using Domain.Exceptions;
using Domain.Exceptions.Messages;
using Domain.ValueObjects.Enums;
using DomainTest.Entities;
using DomainTest.Entities.Barbers;
using DomainTest.Entities.Barbers.Services;
using DomainTest.Entities.Orders;
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
        var configuration = ConfigurationBuilder.BuildRandom();
        var branch = BranchBuilder.Build(configuration);

        var employee = EmployeeBuilder.Build();
        branch.AddEmployee(employee);
        
        var user = UserBuilder.Build();
        
        var order = new Order(branch, employee, new List<Service>(), user, OrderStatus.Pending, DateTime.UtcNow);

        var emptyOrder = new Order(BranchBuilder.Build(configuration), EmployeeBuilder.Build(), new List<Service>(), UserBuilder.Build(), OrderStatus.Pending, DateTime.UtcNow);

        var orderRepository = new Mock<IOrderRepository>();
        orderRepository
            .Setup(repository => repository.Create(It.IsAny<Order>()))
            .Callback<Order>(order =>
             {
                 emptyOrder = order;
            })
            .Returns(Task.CompletedTask);
        orderRepository.Setup(repository => repository.GetBy(It.IsAny<Guid>(), It.IsAny<Guid>()).Result).Returns(new List<Order>());

        var currentService = ServiceBuilder.Build();

        var serviceRepository = new Mock<IServiceRepository>();
        serviceRepository.Setup(repository => repository.Exists(It.IsAny<Guid>(), It.IsAny<Guid>()).Result)
            .Returns(true);

        var branchRepository = new Mock<IBranchRepository>();
        branchRepository.Setup(repository => repository.GetBy(It.IsAny<Guid>()).Result).Returns(branch);

        var orderPolicy = new Mock<IOrderPolicy>();
        orderPolicy.Setup(repository => repository.IsAllowed(It.IsAny<Order>(), It.IsAny<List<Order>>(), It.IsAny<Branch>())).Returns(true);

        var orderTools = new Mock<IOrderTool>();
        orderTools.Setup(repository => repository.RelocateTime(It.IsAny<Order>(), It.IsAny<List<Order>>(), It.IsAny<Branch>())).Returns(order.ScheduleTime);

        var orderService = new OrderService(orderRepository.Object, serviceRepository.Object, branchRepository.Object, orderPolicy.Object, orderTools.Object);

        await orderService.Create(order);

        emptyOrder.Services.ForEach(service =>
        {
            Assert.Contains(service, emptyOrder.Services);
        });

        Assert.True(emptyOrder.User.Id.Equals(emptyOrder.User.Id), "User Id does not matches");
        Assert.True(emptyOrder.Worker.Id.Equals(emptyOrder.Worker.Id), "Worker Id does not matches");
        Assert.True(emptyOrder.Branch.Id.Equals(emptyOrder.Branch.Id), "Branch Id does not matches");
        Assert.True(emptyOrder.ScheduleTime.Equals(emptyOrder.ScheduleTime), "Schedule Time does not Matches");
        Assert.True(emptyOrder.Status.Equals(OrderStatus.Accepted), "Order status is not Accepted");
    }

    [Fact]
    public async Task ShouldThrowAnErrorWhenServiceDoesNotExists()
    {
        var orderDTO = OrderRequestDTOBuilder.Build(1);

        var order = OrderBuilder.Build(orderDTO.Services);

        var orderRepository = new Mock<IOrderRepository>();

        var serviceRepository = new Mock<IServiceRepository>();
        serviceRepository.Setup(serviceRepository => serviceRepository.Exists(It.IsAny<Guid>(), It.IsAny<Guid>()).Result)
            .Returns(false);

        var branchRepository = new Mock<IBranchRepository>();

        var orderPolicy = new Mock<IOrderPolicy>();

        var orderTool = new Mock<IOrderTool>();

        var orderService = new OrderService(orderRepository.Object, serviceRepository.Object, branchRepository.Object, orderPolicy.Object, orderTool.Object);

        var exception = await Assert.ThrowsAsync<ServiceException>(async () => await orderService.Create(order));

        Assert.True(exception.Message.Equals(string.Format(ServiceExceptionMessagesResource.SERVICE_DOES_NOT_EXISTS, orderDTO.Services.First().Id)), "Mensagens de erro não conferem.");
    }
}
