﻿using Domain.Entities.Barbers.Service;
using Domain.Entities.Orders;
using Domain.Exceptions;
using Domain.Exceptions.Messages;
using Domain.ValueObjects.Addresses;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Infra")]
namespace Domain.Entities.Barbers;

[PrimaryKey(nameof(Id))]
internal class Branch
{
    [Column("id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; }

    [Column("identifier")]
    public string Identifier { get; init; }

    public Address Address { get; set; }

    [Required]
    [Column("phone")]
    public string Phone { get; set; }

    [Required]
    [Column("email")]
    public string Email { get; init; }
    public HashSet<Schedule> Schedule { get; } = new();
    public HashSet<Category> Category { get; } = new HashSet<Category>();
    public HashSet<Service.Service> Service { get; } = new HashSet<Service.Service>();
    public HashSet<Employee> Barber { get; } = new HashSet<Employee>();
    public List<Order> Orders { get; } = new List<Order>();
    public bool IsOpened { get; set; }
    public Configuration Configuration { get; private set; }
    public Barber Company { get; init; }

    private Branch() { }
    public Branch(string identifier, Address address, string phone, string email, bool isOpened, Configuration configuration)
    {
        Id = Guid.NewGuid();
        Identifier = identifier;
        Address = address;
        Phone = phone;
        Email = email;
        IsOpened = isOpened;
        Configuration = configuration;
    }

    public void AddEmployeeLunchInterval(Schedule lunchInterval, Guid employeeId)
    {
        if (!IsValidLunchTime(lunchInterval))
            throw new CompanyException(CompanyExceptionMessagesResource.INVALID_LUNCH_TIME_FOR_BRANCH);

        var employee = Barber.FirstOrDefault(barber => barber.Id == employeeId) ?? throw new CompanyException(CompanyExceptionMessagesResource.EMPLOYEE_NOT_FOUND);

        employee.AddLunchInterval(lunchInterval);
    }

    private bool IsValidLunchTime(Schedule lunchInterval)
    {
        var scheduleForThisTime = Schedule.FirstOrDefault(schedule => schedule.Includes(lunchInterval));

        if (scheduleForThisTime == default)
            return false;

        return true;
    }

    public void AddSchedule(Schedule schedule)
    {
        Schedule.Add(schedule);
    }

    public void RemoveSchedule(Schedule schedule)
    {
        Schedule.RemoveWhere(existentSchedule => schedule.WeekDay == existentSchedule.WeekDay);
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

    public void AddService(Guid serviceId, Guid employeeId)
    {
        var employee = Barber.FirstOrDefault(employee => employeeId.Equals(employee.Id)) 
                        ?? throw new CompanyException(CompanyExceptionMessagesResource.EMPLOYEE_NOT_FOUND);

        var service = Service.FirstOrDefault(service => serviceId.Equals(service.Id)) 
                        ?? throw new CompanyException(CompanyExceptionMessagesResource.SERVICE_NOT_FOUND);

        employee.AddServices(new List<Service.Service>() { service });
    }

    public Schedule? GetScheduleFor(DateTime day)
    {
        return Schedule.FirstOrDefault(schedule => schedule.Includes(day));
    }

    public HashSet<Schedule> GetScheduleFor(DayOfWeek dayOfWeek)
    {
        return Schedule.Where(schedule => schedule.Includes(dayOfWeek)).ToHashSet();
    }
}
