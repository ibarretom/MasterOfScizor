using Domain.Entities;
using Domain.Entities.Barbers;
using Domain.Entities.Barbers.Service;
using Domain.Entities.Orders;
using Domain.ValueObjects.Enums;
using DomainTest.Entities.Barbers;

namespace DomainTest.Entities.Orders;

internal class OrderBuilder
{
    public static Order Build()
    {   
        var configuration = ConfigurationBuilder.BuildRandom();
        var branch = BranchBuilder.Build(configuration);

        var employee = EmployeeBuilder.Build();
        branch.AddEmployee(employee);

        return new Order(branch, employee, new List<Service>(), OrderStatus.Pending, DateTime.Now);
    }    
    
    public static Order Build(List<Service> services)
    {   
        var configuration = ConfigurationBuilder.BuildRandom();
        var branch = BranchBuilder.Build(configuration);

        var employee = EmployeeBuilder.Build();
        branch.AddEmployee(employee);

        return new Order(branch, employee, services, OrderStatus.Pending, DateTime.Now);
    }

    public static Order Build(DateTime orderTime, Branch branch)
    {
        var employee = EmployeeBuilder.Build();
        branch.AddEmployee(employee);

        return new Order(branch, employee, new List<Service>(), OrderStatus.Pending, orderTime);
    }

    public static Order Build(DateTime orderTime, Branch branch, Employee worker)
    {
        return new Order(branch, worker, new List<Service>(), OrderStatus.Pending, orderTime);
    }

    public static Order Build(DateTime orderTime, Branch branch, Employee employee, List<Service> services)
    {
        var order = new Order(branch, employee, services, OrderStatus.Pending, orderTime)
        {
            RelocatedSchedule = orderTime
        };

        return order;
    }
}
