using Domain.Entities;
using Domain.Entities.Barbers;
using Domain.Entities.Barbers.Service;
using Domain.Entities.Orders;
using Domain.ValueObjects.Addresses;
using Domain.ValueObjects.Barbers;
using Domain.ValueObjects.EntityHelper;
using Microsoft.EntityFrameworkCore;
namespace Infra.Database;

internal class MasterOfScissorContext : DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<Employee> Employees { get; set; }
    public DbSet<Barber> Companies { get; set; }
    public DbSet<Configuration> Configurations { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Service> Services { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<Schedule> Schedules { get; set; }

    public MasterOfScissorContext(DbContextOptions<MasterOfScissorContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(MasterOfScissorContext).Assembly);

        modelBuilder.Entity<User>(eb =>
        {
            eb.ToTable("users");
            eb.HasMany(user => user.Roles).WithOne(role => role.User).OnDelete(DeleteBehavior.Cascade).HasForeignKey("user_id");
            eb.Property<DateTime>("created_at").ValueGeneratedOnAdd();
            eb.Property<DateTime>("updated_at").ValueGeneratedOnAddOrUpdate();
        });

        modelBuilder.Entity<UserRoleEntity>(eb =>
        {
            eb.ToTable("roles");
            eb.HasOne(role => role.User).WithMany(user => user.Roles).HasForeignKey("user_id");
            eb.Property<DateTime>("created_at").ValueGeneratedOnAdd();
            eb.Property<DateTime>("updated_at").ValueGeneratedOnAddOrUpdate();
        });

        modelBuilder.Entity<Barber>(eb =>
        {
            eb.ToTable("companies");
            eb.HasMany(company => company.Branch)
                .WithOne(branch => branch.Company)
                .OnDelete(DeleteBehavior.Cascade);
            eb.HasOne(company => company.Owner).WithMany().HasForeignKey("owner_id");
            eb.Property<DateTime>("created_at").ValueGeneratedOnAdd();
            eb.Property<DateTime>("updated_at").ValueGeneratedOnAddOrUpdate();
        });

        modelBuilder.Entity<Branch>(eb =>
        {
            eb.ToTable("branches");
            eb.HasOne(branch => branch.Company)
                .WithMany(company => company.Branch)
                .HasForeignKey("company_id");
            eb.HasMany(branch => branch.Barber)
                .WithOne(barber => barber.Branch)
                .HasForeignKey("branch_id");
            eb.HasMany(branch => branch.Service)
                .WithOne(service => service.Branch)
                .HasForeignKey("branch_id")
                .OnDelete(DeleteBehavior.Cascade);
            eb.HasOne(branch => branch.Address).WithMany().HasForeignKey("address_id");
            eb.HasMany(branch => branch.Category).WithOne(category => category.Branch).HasForeignKey("branch_id");
            eb.HasMany(branch => branch.Orders).WithOne(order => order.Branch).HasForeignKey("branch_id");
            eb.HasMany(branch => branch.Schedule).WithOne(schedule => schedule.Branch).HasForeignKey("branch_id");
            eb.HasOne(branch => branch.Configuration)
                .WithOne(configuration => configuration.Branch)
                .HasForeignKey<Branch>("config_id");
            eb.Property<DateTime>("created_at").ValueGeneratedOnAdd();
            eb.Property<DateTime>("updated_at").ValueGeneratedOnAddOrUpdate();
        });

        modelBuilder.Entity<AddressLocalization>(eb =>
        {
            eb.ToTable("address_localization");
            eb.Property<DateTime>("created_at").ValueGeneratedOnAdd();
            eb.Property<DateTime>("updated_at").ValueGeneratedOnAddOrUpdate();

        });

        modelBuilder.Entity<Address>(eb =>
        {
            eb.ToTable("address");
            eb.HasOne(address => address.Localization).WithMany().HasForeignKey("localization_id");
            eb.OwnsOne(eb => eb.Identifier);
            eb.Property<DateTime>("created_at").ValueGeneratedOnAdd();
            eb.Property<DateTime>("updated_at").ValueGeneratedOnAddOrUpdate();
        });

        modelBuilder.Entity<Schedule>(eb =>
        {
            eb.ToTable("schedules");
            eb.HasOne(schedule => schedule.Branch)
                .WithMany(branch => branch.Schedule)
                .HasForeignKey("branch_id");
            eb.HasOne(schedule => schedule.Employee)
                .WithMany(employee => employee.LunchInterval)
                .HasForeignKey("employee_id");
            eb.Property<DateTime>("updated_at").ValueGeneratedOnAddOrUpdate();
            eb.Property<DateTime>("created_at").ValueGeneratedOnAdd();
        });

        modelBuilder.Entity<Service>(eb =>
        {
            eb.ToTable("services");
            eb.HasMany(service => service.Workers)
                .WithMany(employee => employee.Services)
                .UsingEntity<WorkerServiceRelation>();
            eb.HasOne(service => service.Branch)
                .WithMany(branch => branch.Service)
                .HasForeignKey("branch_id");
            eb.HasOne(service => service.Category)
                .WithMany(category => category.Services)
                .HasForeignKey("category_id");
            eb.Property<DateTime>("updated_at").ValueGeneratedOnAddOrUpdate();
            eb.Property<DateTime>("created_at").ValueGeneratedOnAdd();
        });

        modelBuilder.Entity<Category>(eb =>
        {
            eb.ToTable("categories");
            eb.HasMany(category => category.Services)
                .WithOne(service => service.Category)
                .OnDelete(DeleteBehavior.Cascade);
            eb.HasOne(category => category.Branch)
                .WithMany(branch => branch.Category)
                .HasForeignKey("branch_id");
            eb.Property<DateTime>("updated_at").ValueGeneratedOnAddOrUpdate();
            eb.Property<DateTime>("created_at").ValueGeneratedOnAdd();
        });

        modelBuilder.Entity<Order>(eb =>
        {
            eb.ToTable("orders");
            eb.HasOne(order => order.Branch)
                .WithMany(branch => branch.Orders)
                .HasForeignKey("branch_id");
            eb.HasOne(order => order.Worker)
            .WithMany(worker => worker.Orders)
                .HasForeignKey("worker_id");
            eb.HasMany(order => order.OrderServices)
                .WithOne(orderService => orderService.Order)
                .HasForeignKey("order_id");
            eb.Property<DateTime>("updated_at").ValueGeneratedOnAddOrUpdate();
            eb.Property<DateTime>("created_at").ValueGeneratedOnAdd();
        });
        
        modelBuilder.Entity<OrderJob>(eb =>
        {
            eb.ToTable("order_services");
            eb.HasOne(orderService => orderService.Order)
                .WithMany(order => order.OrderServices)
                .HasForeignKey("order_id");
            eb.HasOne(order => order.Service).WithMany()
                .HasForeignKey("service_id");
            eb.Property<DateTime>("updated_at").ValueGeneratedOnAddOrUpdate();
            eb.Property<DateTime>("created_at").ValueGeneratedOnAdd();
        });

        modelBuilder.Entity<Configuration>(eb =>
        {
            eb.ToTable("configurations");
            eb.HasOne(configuration => configuration.Branch)
                .WithOne(branch => branch.Configuration)
                .HasForeignKey<Configuration>("branch_id");
            eb.Property<DateTime>("created_at").ValueGeneratedOnAdd();
            eb.Property<DateTime>("updated_at").ValueGeneratedOnAddOrUpdate();
        });

        modelBuilder.Entity<Employee>(eb =>
        {
            eb.ToTable("employees");
            eb.HasMany(employee => employee.LunchInterval)
                .WithOne(lunchInterval => lunchInterval.Employee)
                .OnDelete(DeleteBehavior.Cascade);
            eb.HasMany(employee => employee.Services)
                .WithMany(service => service.Workers)
                .UsingEntity<WorkerServiceRelation>();
            eb.Property<DateTime>("updated_at").ValueGeneratedOnAddOrUpdate();
        });

    }
}
