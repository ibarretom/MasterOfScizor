using Domain.Entities.Barbers;
using Domain.Entities.Barbers.Service;

namespace Domain.Entities.Orders;

internal class OrderBase
{
    public Guid Id { get; }
    public Branch Branch { get; }
    public Employee Worker { get; }
    public List<Service> Services { get; }

    public OrderBase(Branch branch, Employee worker, List<Service> services)
    {
        Id = Guid.NewGuid();
        Branch = branch;
        Worker = worker;
        Services = services;
    }
}