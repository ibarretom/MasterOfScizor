using Domain.Entities.Barbers;
using Domain.Entities.Barbers.Service;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities.Orders;

[PrimaryKey(nameof(Id))]
internal class OrderBase
{
    [Column("id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; }

    [Required]
    public Branch Branch { get; }
    
    [Required]
    public Employee Worker { get; }
    
    [NotMapped]
    public List<Service> Services { get; }

    private protected OrderBase() { }
    public OrderBase(Branch branch, Employee worker, List<Service> services)
    {
        Id = Guid.NewGuid();
        Branch = branch;
        Worker = worker;
        Services = services;
    }
}