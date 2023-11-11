using Domain.Entities;
using Domain.Entities.Barbers;
using Domain.Entities.Barbers.Service;
using Domain.Entities.Orders;
using DomainTest.Entities.Barbers;

namespace DomainTest.Entities.Orders;

internal static class OrderBaseBuilder
{
    public static OrderBase Build()
    {
        var configuration = ConfigurationBuilder.BuildRandom();
        var branch = BranchBuilder.Build(configuration);

        var employee = EmployeeBuilder.Build();
        branch.AddEmployee(employee);

        return new OrderBase(branch, employee, new List<Service>());
    }

    public static OrderBase Build(List<Service> services)
    {
        var configuration = ConfigurationBuilder.BuildRandom();
        var branch = BranchBuilder.Build(configuration);

        var employee = EmployeeBuilder.Build();
        branch.AddEmployee(employee);

        return new OrderBase(branch, employee, services);
    }

    public static OrderBase Build(Branch branch)
    {
        var employee = EmployeeBuilder.Build();
        branch.AddEmployee(employee);

        return new OrderBase(branch, employee, new List<Service>());
    }

    public static OrderBase Build(Branch branch, Employee worker)
    {
        return new OrderBase(branch, worker, new List<Service>());
    }

    public static OrderBase Build(Branch branch, Employee employee, List<Service> services)
    {
        return new OrderBase(branch, employee, services);
    }
}
