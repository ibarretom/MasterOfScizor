using Domain.Entities.Orders;
using Domain.Exceptions;
using Domain.Exceptions.Messages;
using Domain.ValueObjects.DTO.Orders;
using Domain.ValueObjects.Enums;
using Infra.Repositories.Company;

namespace Application.Services.Orders;

internal class OrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IServiceRepository _serviceRepository;

    public OrderService(IOrderRepository orderRepository, IServiceRepository serviceRepository)
    {
        _orderRepository = orderRepository;
        _serviceRepository = serviceRepository;
    }

    public async Task Create(OrderRequestDTO order)
    {

        var servicesTask = order.ServiceId.Select(serviceId => Task.Run(async () =>
        {
            var service = await _serviceRepository.GetById(order.BranchId, serviceId);

            return service is null
                ? throw new ServiceException(string.Format(ServiceExceptionMessagesResource.SERVICE_DOES_NOT_EXISTS, serviceId))
                : service;
        }));


        var createdOrder = new Order(order.BranchId, order.WorkerId, order.ServiceId, order.UserId, OrderStatus.Accepted, order.ScheduleTime);

        await _orderRepository.Create(createdOrder);
    }
}
