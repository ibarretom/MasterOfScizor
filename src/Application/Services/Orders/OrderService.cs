using Domain.Entities;
using Domain.Entities.Barbers;
using Domain.Entities.Orders;
using Domain.Exceptions;
using Domain.Exceptions.Messages;
using Domain.ValueObjects.Enums;
using Infra.Repositories.Company;

namespace Application.Services.Orders;

internal class OrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IServiceRepository _serviceRepository;
    private readonly IOrderPolicy _orderPolicy;
    private readonly IOrderTime _orderTime;

    public OrderService(IOrderRepository orderRepository, IServiceRepository serviceRepository, IOrderPolicy orderPolicy, IOrderTime orderTime)
    {
        _orderRepository = orderRepository;
        _serviceRepository = serviceRepository;
        _orderPolicy = orderPolicy;
        _orderTime = orderTime;
    }

    public async Task Create(Order order, Branch branch)
     {
        var servicesTask = order.Services.Select(service => Task.Run(async () =>
        {
            var serviceExists = await _serviceRepository.Exists(order.Branch.Id, service.Id);

            if (!serviceExists)
                throw new ServiceException(string.Format(ServiceExceptionMessagesResource.SERVICE_DOES_NOT_EXISTS, service.Id));

            return serviceExists;
        }));

        await Task.WhenAll(servicesTask);
       
        var allOrders = await _orderRepository.GetBy(order.Branch.Id, order.Worker.Id);

        var createdOrder = new Order(order.Branch, order.Worker, order.Services, order.User, OrderStatus.Accepted, order.ScheduleTime);

        if (!_orderPolicy.IsAllowed(createdOrder, allOrders, branch, out var reason))
            throw new OrderException(reason);

        await _orderRepository.Create(createdOrder);
    }

    public async Task Update(Order order, OrderStatus orderStatus)
    {
        order.UpdateStatus(orderStatus);

        await _orderRepository.Update(order);
    }

    public async Task Update(Order order, Branch branch, DateTime scheduleTime)
    {
        order.RelocatedSchedule = scheduleTime;

        var allOrders = await _orderRepository.GetBy(order.Branch.Id, order.Worker.Id);

        if(!_orderPolicy.IsAllowed(order, allOrders, branch, out var reason))
            throw new OrderException(reason);

        await _orderRepository.Update(order);
    }

    public async Task<IEnumerable<DateTime>> GetAvailableTimes(DateTime day, Employee employee, Branch branch)
    {
        var ordersForThisWorker = await _orderRepository.GetBy(employee.BranchId, employee.Id);

        return _orderTime.GetAvailable(day, employee, ordersForThisWorker, branch);
    }
}
