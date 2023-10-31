﻿using Domain.Entities.Barbers;
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

    public OrderService(IOrderRepository orderRepository, IServiceRepository serviceRepository, IOrderPolicy orderPolicy)
    {
        _orderRepository = orderRepository;
        _serviceRepository = serviceRepository;
        _orderPolicy = orderPolicy;
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

        if (!_orderPolicy.IsAllowed(createdOrder, allOrders, branch))
            throw new OrderException(OrderExceptionResourceMessages.INVALID_ORDER);

        await _orderRepository.Create(createdOrder);
    }
}
