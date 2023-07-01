using Domain.Entities.Barbers.Service;
using Domain.ValueObjects.Addresses;

namespace Domain.Entities.Barbers;

internal class Branch
{
    public string Identifier { get; }
    public Address Address { get; set; }
    public string Phone { get; set; }
    public string Email { get; }
    public HashSet<Schedule> Schedule { get; } = new HashSet<Schedule>();
    public HashSet<Category> Category { get; } = new HashSet<Category>();
    public HashSet<Service.Service> Service { get; } = new HashSet<Service.Service>();
    public HashSet<Employee> Barber { get; } = new HashSet<Employee>();
    public bool IsOpened { get; set; }

    public Branch(string identifier, Address address, string phone, string email, bool isOpened)
    {
        Identifier = identifier;
        Address = address;
        Phone = phone;
        Email = email;
        IsOpened = isOpened;
    }

    public void AddSchedule(Schedule schedule)
    {
        Schedule.Add(schedule);
    }

    public void RemoveSchedule(Schedule schedule)
    {
        Schedule.RemoveWhere(existentSchedule => schedule.DayOfWeek == existentSchedule.DayOfWeek);
    }

    public void AddCategory(Category category)
    {
        Category.Add(category);
    }

    public void RemoveCategory(Category category)
    {
        Category.RemoveWhere(existentCategory => category.Id == existentCategory.Id);
    }

    public void AddService(Service.Service service)
    {
        Service.Add(service);
    }

    public void RemoveService(Service.Service service)
    {
        Service.RemoveWhere(existentService => service.Id == existentService.Id);
    }

    public void AddEmployee(Employee employee)
    {
        Barber.Add(employee);
    }

    public void RemoveEmployee(Employee employee)
    {
        Barber.RemoveWhere(existentBarber => employee.Id == existentBarber.Id);
    }
}
