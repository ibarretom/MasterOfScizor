using Application.Services.Orders;
using Domain.Entities.Barbers.Service;
using Domain.Entities.Orders;
using Domain.Exceptions;
using Domain.Exceptions.Messages;
using Domain.Services.Barbers;
using Domain.ValueObjects.Enums;
using DomainTest.Entities;
using DomainTest.Entities.Barbers;
using DomainTest.Entities.Barbers.Services;
using DomainTest.Entities.Orders;
using DomainTest.ValueObjects.DTO;
using Infra.Repositories.Company;
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
        
        var order = new Order(branch, employee, new List<Service>(), OrderStatus.Pending, DateTime.UtcNow);

        var emptyOrder = new Order(BranchBuilder.Build(configuration), EmployeeBuilder.Build(), new List<Service>(), OrderStatus.Pending, DateTime.UtcNow);

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

        var orderPolicy = new Mock<IOrderPolicy>();
        
        string? reason = null;

        orderPolicy.Setup(repository => repository.IsAllowed(It.IsAny<Order>(), It.IsAny<List<Order>>(), out reason)).Returns(true);

        var orderTime = new Mock<IScheduler>();

        var orderService = new OrderService(orderRepository.Object, serviceRepository.Object, orderPolicy.Object, orderTime.Object);

        await orderService.Create(order);

        emptyOrder.Services.ForEach(service =>
        {
            Assert.Contains(service, emptyOrder.Services);
        });

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

        var orderPolicy = new Mock<IOrderPolicy>();

        var orderTime = new Mock<IScheduler>();
        var orderService = new OrderService(orderRepository.Object, serviceRepository.Object, orderPolicy.Object, orderTime.Object);

        var branch = BranchBuilder.Build(ConfigurationBuilder.BuildWithQueueLimit());

        var exception = await Assert.ThrowsAsync<ServiceException>(async () => await orderService.Create(order));

        Assert.True(exception.Message.Equals(string.Format(ServiceExceptionMessagesResource.SERVICE_DOES_NOT_EXISTS, orderDTO.Services.First().Id)), "Mensagens de erro não conferem.");
    }

    [Fact]
    public async Task ShouldBeAbleToChangeTheOrderStatus()
    {
        var order = OrderBuilder.Build();
        Order? orderUpdated = null;

        var orderRepository = new Mock<IOrderRepository>();
        orderRepository.Setup(repository => repository.Update(It.IsAny<Order>()))
            .Callback<Order>(order =>
            {
                orderUpdated = order;
            })
            .Returns(Task.CompletedTask);

        var serviceRepository = new Mock<IServiceRepository>();

        var orderPolicy = new Mock<IOrderPolicy>();

        var orderTime = new Mock<IScheduler>();
        var orderService = new OrderService(orderRepository.Object, serviceRepository.Object, orderPolicy.Object, orderTime.Object);

        await orderService.Update(order, OrderStatus.Accepted);

        Assert.True(orderUpdated?.Status.Equals(OrderStatus.Accepted), "Order status is not Accepted");
    }

    [Fact]
    public async Task ShouldBeAbleToUpdateTheRescheduleOrderTime()
    {
        var order = OrderBuilder.Build();
        Order? orderUpdated = null;

        var orderRepository = new Mock<IOrderRepository>();
        orderRepository.Setup(repository => repository.Update(It.IsAny<Order>()))
            .Callback<Order>(order =>
            {
                orderUpdated = order;
            })
            .Returns(Task.CompletedTask);

        var serviceRepository = new Mock<IServiceRepository>();

        var orderPolicy = new Mock<IOrderPolicy>();

        string? reason = null;
        orderPolicy.Setup(repository => repository.IsAllowed(It.IsAny<Order>(), It.IsAny<List<Order>>(), out reason)).Returns(true);
        
        var orderTime = new Mock<IScheduler>();

        var orderService = new OrderService(orderRepository.Object, serviceRepository.Object, orderPolicy.Object, orderTime.Object);

        var now = DateTime.UtcNow.AddMinutes(30);

        await orderService.Update(order, now);

        Assert.True(orderUpdated?.RelocatedSchedule.Equals(now), "Order status is not Accepted");
        Assert.True(orderUpdated?.ScheduleTime.Equals(order.ScheduleTime), "Order status is not Accepted");
    }

    [Fact]
    public async Task ShouldRejectAOrderUpdateWhenThePolicyIsNotMatched()
    {
        var order = OrderBuilder.Build();
        Order? orderUpdated = null;

        var orderRepository = new Mock<IOrderRepository>();
        orderRepository.Setup(repository => repository.Update(It.IsAny<Order>()))
            .Callback<Order>(order =>
            {
                orderUpdated = order;
            })
            .Returns(Task.CompletedTask);

        var serviceRepository = new Mock<IServiceRepository>();

        var orderPolicy = new Mock<IOrderPolicy>();
        
        string? reason = null;
        orderPolicy.Setup(repository => repository.IsAllowed(It.IsAny<Order>(), It.IsAny<List<Order>>(), out reason))
            .Returns(false);

        var orderTime = new Mock<IScheduler>();

        var orderService = new OrderService(orderRepository.Object, serviceRepository.Object, orderPolicy.Object, orderTime.Object);

        var now = DateTime.UtcNow.AddMinutes(30);

        await Assert.ThrowsAsync<OrderException>(async () => await orderService.Update(order, now));
    }
}
