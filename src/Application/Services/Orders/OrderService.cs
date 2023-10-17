using Domain.Entities.Orders;
using Domain.Exceptions;
using Domain.Exceptions.Messages;
using Domain.ValueObjects.DTO.Orders;
using Domain.ValueObjects.Enums;
using Infra.Repositories.Company;
using Infra.Repositories.CompanyRepository;

namespace Application.Services.Orders;

internal class OrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IServiceRepository _serviceRepository;
    private readonly IBranchRepository _branchRepository;
    private readonly IOrderPolicy _orderPolicy;
    private readonly IOrderTool _orderTool;

    public OrderService(IOrderRepository orderRepository, IServiceRepository serviceRepository, IBranchRepository branchRepository, IOrderPolicy orderPolicy, IOrderTool orderTool)
    {
        _orderRepository = orderRepository;
        _serviceRepository = serviceRepository;
        _branchRepository = branchRepository;
        _orderPolicy = orderPolicy;
        _orderTool = orderTool;
    }

    public async Task Create(Order order)
     {
        var servicesTask = order.Services.Select(service => Task.Run(async () =>
        {
            var serviceExists = await _serviceRepository.Exists(order.Branch.Id, service.Id);

            if (!serviceExists)
                throw new ServiceException(string.Format(ServiceExceptionMessagesResource.SERVICE_DOES_NOT_EXISTS, service.Id));

            return serviceExists;
        }));

        await Task.WhenAll(servicesTask);

        var branch = await _branchRepository.GetBy(order.Branch.Id);
       
        var allOrders = await _orderRepository.GetBy(order.Branch.Id, order.Worker.Id);

        var createdOrder = new Order(order.Branch, order.Worker, order.Services, order.User, OrderStatus.Accepted, order.ScheduleTime);
        
        if (branch.Configuration.OrderQueueType == OrderQueueType.Queue)
            createdOrder.RelocatedSchedule = _orderTool.RelocateTime(createdOrder, allOrders, branch);

        if (!_orderPolicy.IsAllowed(createdOrder, allOrders, branch))
            throw new OrderException(OrderExceptionResourceMessages.INVALID_ORDER);

        await _orderRepository.Create(createdOrder);
    }
}
