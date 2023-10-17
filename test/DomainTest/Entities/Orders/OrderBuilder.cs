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

        var user = UserBuilder.Build();

        return new Order(branch, employee, new List<Service>(), user, OrderStatus.Pending, DateTime.Now);
    }    
    
    public static Order Build(List<Service> services)
    {   
        var configuration = ConfigurationBuilder.BuildRandom();
        var branch = BranchBuilder.Build(configuration);

        var employee = EmployeeBuilder.Build();
        branch.AddEmployee(employee);

        var user = UserBuilder.Build();

        return new Order(branch, employee, services, user, OrderStatus.Pending, DateTime.Now);
    }

    public static Order Build(DateTime orderTime, Branch branch)
    {
        var employee = EmployeeBuilder.Build();
        branch.AddEmployee(employee);

        var user = UserBuilder.Build();

        return new Order(branch, employee, new List<Service>(), user, OrderStatus.Pending, orderTime);
    }

    public static Order Build(DateTime orderTime, Branch branch, Employee worker)
    {
        var user = UserBuilder.Build();
        return new Order(branch, worker, new List<Service>(), user, OrderStatus.Pending, orderTime);
    }

    public static Order Build(DateTime orderTime, Branch branch, Employee employee, List<Service> services)
    {
        var user = UserBuilder.Build();
        var order = new Order(branch, employee, services, user, OrderStatus.Pending, orderTime)
        {
            RelocatedSchedule = orderTime
        };

        return order;
    }
}
