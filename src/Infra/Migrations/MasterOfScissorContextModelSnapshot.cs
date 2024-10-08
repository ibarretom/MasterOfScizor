﻿// <auto-generated />
using System;
using Infra.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace Infra.Migrations
{
    [DbContext(typeof(MasterOfScissorContext))]
    partial class MasterOfScissorContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.14")
                .HasAnnotation("Relational:MaxIdentifierLength", 64);

            modelBuilder.Entity("Domain.Entities.Barbers.Barber", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("char(36)")
                        .HasColumnName("id");

                    b.Property<string>("Avatar")
                        .IsRequired()
                        .HasColumnType("longtext")
                        .HasColumnName("avatar");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("longtext")
                        .HasColumnName("name");

                    b.Property<DateTime>("created_at")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime(6)");

                    b.Property<Guid>("owner_id")
                        .HasColumnType("char(36)");

                    b.Property<DateTime>("updated_at")
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("datetime(6)");

                    b.HasKey("Id");

                    b.HasIndex("owner_id");

                    b.ToTable("companies", (string)null);
                });

            modelBuilder.Entity("Domain.Entities.Barbers.Branch", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("char(36)")
                        .HasColumnName("id");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("longtext")
                        .HasColumnName("email");

                    b.Property<string>("Identifier")
                        .IsRequired()
                        .HasColumnType("longtext")
                        .HasColumnName("identifier");

                    b.Property<bool>("IsOpened")
                        .HasColumnType("tinyint(1)");

                    b.Property<string>("Phone")
                        .IsRequired()
                        .HasColumnType("longtext")
                        .HasColumnName("phone");

                    b.Property<Guid>("address_id")
                        .HasColumnType("char(36)");

                    b.Property<Guid>("company_id")
                        .HasColumnType("char(36)");

                    b.Property<Guid>("config_id")
                        .HasColumnType("char(36)");

                    b.Property<DateTime>("created_at")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime(6)");

                    b.Property<DateTime>("updated_at")
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("datetime(6)");

                    b.HasKey("Id");

                    b.HasIndex("address_id");

                    b.HasIndex("company_id");

                    b.ToTable("branches", (string)null);
                });

            modelBuilder.Entity("Domain.Entities.Barbers.Configuration", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("char(36)")
                        .HasColumnName("id");

                    b.Property<bool>("AutoAdjustScheduleTime")
                        .HasColumnType("tinyint(1)")
                        .HasColumnName("auto_adjust_schedule_time");

                    b.Property<int>("OrderQueueType")
                        .HasColumnType("int")
                        .HasColumnName("queue_type");

                    b.Property<bool>("QueueAccordingToVirtualQueue")
                        .HasColumnType("tinyint(1)")
                        .HasColumnName("queue_according_to_virtual_queue");

                    b.Property<int?>("QueueLimit")
                        .HasColumnType("int")
                        .HasColumnName("queue_limit");

                    b.Property<TimeSpan>("ScheduleDefaultInterval")
                        .HasColumnType("time(6)")
                        .HasColumnName("schedule_default_interval");

                    b.Property<TimeSpan>("ScheduleDelayTime")
                        .HasColumnType("time(6)")
                        .HasColumnName("schedule_delay_time");

                    b.Property<Guid>("branch_id")
                        .HasColumnType("char(36)");

                    b.Property<DateTime>("created_at")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime(6)");

                    b.Property<DateTime>("updated_at")
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("datetime(6)");

                    b.HasKey("Id");

                    b.HasIndex("branch_id")
                        .IsUnique();

                    b.ToTable("configurations", (string)null);
                });

            modelBuilder.Entity("Domain.Entities.Barbers.Schedule", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("char(36)")
                        .HasColumnName("id");

                    b.Property<TimeOnly>("EndTime")
                        .HasColumnType("time(6)")
                        .HasColumnName("end_time");

                    b.Property<int?>("OverflowingDay")
                        .HasColumnType("int")
                        .HasColumnName("overflowing_day");

                    b.Property<TimeOnly>("StartTime")
                        .HasColumnType("time(6)")
                        .HasColumnName("start_time");

                    b.Property<int>("WeekDay")
                        .HasColumnType("int")
                        .HasColumnName("week_day");

                    b.Property<Guid?>("branch_id")
                        .HasColumnType("char(36)");

                    b.Property<DateTime>("created_at")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime(6)");

                    b.Property<Guid?>("employee_id")
                        .HasColumnType("char(36)");

                    b.Property<DateTime>("updated_at")
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("datetime(6)");

                    b.HasKey("Id");

                    b.HasIndex("branch_id");

                    b.HasIndex("employee_id");

                    b.ToTable("schedules", (string)null);
                });

            modelBuilder.Entity("Domain.Entities.Barbers.Service.Category", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("char(36)")
                        .HasColumnName("id");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("longtext")
                        .HasColumnName("name");

                    b.Property<Guid>("branch_id")
                        .HasColumnType("char(36)");

                    b.Property<DateTime>("created_at")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime(6)");

                    b.Property<DateTime>("updated_at")
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("datetime(6)");

                    b.HasKey("Id");

                    b.HasIndex("branch_id");

                    b.ToTable("categories", (string)null);
                });

            modelBuilder.Entity("Domain.Entities.Barbers.Service.Service", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("char(36)")
                        .HasColumnName("id");

                    b.Property<bool>("Active")
                        .HasColumnType("tinyint(1)")
                        .HasColumnName("is_active");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("longtext")
                        .HasColumnName("description");

                    b.Property<TimeSpan>("Duration")
                        .HasColumnType("time(6)")
                        .HasColumnName("duration");

                    b.Property<bool>("IsPromotionActive")
                        .HasColumnType("tinyint(1)")
                        .HasColumnName("is_promotion_active");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("longtext")
                        .HasColumnName("name");

                    b.Property<decimal>("Price")
                        .HasColumnType("decimal(65,30)")
                        .HasColumnName("price");

                    b.Property<decimal>("PromotionalPrice")
                        .HasColumnType("decimal(65,30)")
                        .HasColumnName("promotional_price");

                    b.Property<Guid?>("branch_id")
                        .HasColumnType("char(36)");

                    b.Property<Guid?>("category_id")
                        .HasColumnType("char(36)");

                    b.Property<DateTime>("created_at")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime(6)");

                    b.Property<DateTime>("updated_at")
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("datetime(6)");

                    b.HasKey("Id");

                    b.HasIndex("branch_id");

                    b.HasIndex("category_id");

                    b.ToTable("services", (string)null);
                });

            modelBuilder.Entity("Domain.Entities.Orders.Order", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("char(36)")
                        .HasColumnName("id");

                    b.Property<DateTime>("RelocatedSchedule")
                        .HasColumnType("datetime(6)")
                        .HasColumnName("relocated_schedule");

                    b.Property<DateTime>("ScheduleTime")
                        .HasColumnType("datetime(6)")
                        .HasColumnName("schedule_time");

                    b.Property<int>("Status")
                        .HasColumnType("int")
                        .HasColumnName("status");

                    b.Property<decimal>("Total")
                        .HasColumnType("decimal(65,30)")
                        .HasColumnName("total");

                    b.Property<Guid>("branch_id")
                        .HasColumnType("char(36)");

                    b.Property<DateTime>("created_at")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime(6)");

                    b.Property<DateTime>("updated_at")
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("datetime(6)");

                    b.Property<Guid>("worker_id")
                        .HasColumnType("char(36)");

                    b.HasKey("Id");

                    b.HasIndex("branch_id");

                    b.HasIndex("worker_id");

                    b.ToTable("orders", (string)null);
                });

            modelBuilder.Entity("Domain.Entities.Orders.OrderJob", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("char(36)")
                        .HasColumnName("id");

                    b.Property<Guid?>("CategoryId")
                        .HasColumnType("char(36)");

                    b.Property<TimeSpan>("Duration")
                        .HasColumnType("time(6)")
                        .HasColumnName("duration");

                    b.Property<bool>("IsPromotionActive")
                        .HasColumnType("tinyint(1)")
                        .HasColumnName("is_promotion_active");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("longtext")
                        .HasColumnName("name");

                    b.Property<decimal>("Price")
                        .HasColumnType("decimal(65,30)")
                        .HasColumnName("price");

                    b.Property<decimal>("PromotionalPrice")
                        .HasColumnType("decimal(65,30)")
                        .HasColumnName("promotional_price");

                    b.Property<DateTime>("created_at")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime(6)");

                    b.Property<Guid>("order_id")
                        .HasColumnType("char(36)");

                    b.Property<Guid>("service_id")
                        .HasColumnType("char(36)");

                    b.Property<DateTime>("updated_at")
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("datetime(6)");

                    b.HasKey("Id");

                    b.HasIndex("CategoryId");

                    b.HasIndex("order_id");

                    b.HasIndex("service_id");

                    b.ToTable("order_services", (string)null);
                });

            modelBuilder.Entity("Domain.Entities.User", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("char(36)")
                        .HasColumnName("id");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasMaxLength(320)
                        .HasColumnType("varchar(320)")
                        .HasColumnName("email");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(500)
                        .HasColumnType("varchar(500)")
                        .HasColumnName("name");

                    b.Property<string>("Password")
                        .IsRequired()
                        .HasColumnType("longtext")
                        .HasColumnName("password");

                    b.Property<string>("Phone")
                        .IsRequired()
                        .HasMaxLength(30)
                        .HasColumnType("varchar(30)")
                        .HasColumnName("phone");

                    b.Property<DateTime>("created_at")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime(6)");

                    b.Property<DateTime>("updated_at")
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("datetime(6)");

                    b.HasKey("Id");

                    b.HasIndex("Id", "Email", "Phone")
                        .IsUnique();

                    b.ToTable("users", (string)null);

                    b.UseTptMappingStrategy();
                });

            modelBuilder.Entity("Domain.ValueObjects.Addresses.Address", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("char(36)")
                        .HasColumnName("id");

                    b.Property<DateTime>("created_at")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime(6)");

                    b.Property<Guid>("localization_id")
                        .HasColumnType("char(36)");

                    b.Property<DateTime>("updated_at")
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("datetime(6)");

                    b.HasKey("Id");

                    b.HasIndex("localization_id");

                    b.ToTable("address", (string)null);
                });

            modelBuilder.Entity("Domain.ValueObjects.Addresses.AddressLocalization", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("char(36)")
                        .HasColumnName("id");

                    b.Property<string>("City")
                        .IsRequired()
                        .HasColumnType("longtext")
                        .HasColumnName("city");

                    b.Property<string>("Country")
                        .IsRequired()
                        .HasColumnType("longtext")
                        .HasColumnName("country");

                    b.Property<string>("Neighborhood")
                        .IsRequired()
                        .HasColumnType("longtext")
                        .HasColumnName("neighborhood");

                    b.Property<string>("State")
                        .IsRequired()
                        .HasColumnType("longtext")
                        .HasColumnName("state");

                    b.Property<string>("Street")
                        .IsRequired()
                        .HasColumnType("longtext")
                        .HasColumnName("street");

                    b.Property<string>("ZipCode")
                        .IsRequired()
                        .HasColumnType("longtext")
                        .HasColumnName("zip_code");

                    b.Property<DateTime>("created_at")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime(6)");

                    b.Property<DateTime>("updated_at")
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("datetime(6)");

                    b.HasKey("Id");

                    b.ToTable("address_localization", (string)null);
                });

            modelBuilder.Entity("Domain.ValueObjects.Barbers.UserRoleEntity", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("char(36)")
                        .HasColumnName("id");

                    b.Property<int>("Role")
                        .HasColumnType("int")
                        .HasColumnName("role");

                    b.Property<DateTime>("created_at")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime(6)");

                    b.Property<DateTime>("updated_at")
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("datetime(6)");

                    b.Property<Guid>("user_id")
                        .HasColumnType("char(36)");

                    b.HasKey("Id");

                    b.HasIndex("user_id");

                    b.ToTable("roles", (string)null);
                });

            modelBuilder.Entity("Domain.ValueObjects.EntityHelper.WorkerServiceRelation", b =>
                {
                    b.Property<Guid>("ServiceId")
                        .HasColumnType("char(36)")
                        .HasColumnName("service_id");

                    b.Property<Guid>("WorkersId")
                        .HasColumnType("char(36)");

                    b.Property<Guid>("WorkerId")
                        .HasColumnType("char(36)")
                        .HasColumnName("worker_id");

                    b.HasKey("ServiceId", "WorkersId");

                    b.HasIndex("WorkersId");

                    b.ToTable("workers_services");
                });

            modelBuilder.Entity("Domain.Entities.Employee", b =>
                {
                    b.HasBaseType("Domain.Entities.User");

                    b.Property<bool>("Active")
                        .HasColumnType("tinyint(1)")
                        .HasColumnName("active");

                    b.Property<string>("Avatar")
                        .IsRequired()
                        .HasColumnType("longtext")
                        .HasColumnName("avatar");

                    b.Property<string>("Document")
                        .IsRequired()
                        .HasColumnType("longtext")
                        .HasColumnName("Document");

                    b.Property<Guid>("branch_id")
                        .HasColumnType("char(36)");

                    b.HasIndex("branch_id");

                    b.ToTable("employees", (string)null);
                });

            modelBuilder.Entity("Domain.Entities.Barbers.Barber", b =>
                {
                    b.HasOne("Domain.Entities.User", "Owner")
                        .WithMany()
                        .HasForeignKey("owner_id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Owner");
                });

            modelBuilder.Entity("Domain.Entities.Barbers.Branch", b =>
                {
                    b.HasOne("Domain.ValueObjects.Addresses.Address", "Address")
                        .WithMany()
                        .HasForeignKey("address_id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Domain.Entities.Barbers.Barber", "Company")
                        .WithMany("Branch")
                        .HasForeignKey("company_id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Address");

                    b.Navigation("Company");
                });

            modelBuilder.Entity("Domain.Entities.Barbers.Configuration", b =>
                {
                    b.HasOne("Domain.Entities.Barbers.Branch", "Branch")
                        .WithOne("Configuration")
                        .HasForeignKey("Domain.Entities.Barbers.Configuration", "branch_id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Branch");
                });

            modelBuilder.Entity("Domain.Entities.Barbers.Schedule", b =>
                {
                    b.HasOne("Domain.Entities.Barbers.Branch", "Branch")
                        .WithMany("Schedule")
                        .HasForeignKey("branch_id");

                    b.HasOne("Domain.Entities.Employee", "Employee")
                        .WithMany("LunchInterval")
                        .HasForeignKey("employee_id")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.Navigation("Branch");

                    b.Navigation("Employee");
                });

            modelBuilder.Entity("Domain.Entities.Barbers.Service.Category", b =>
                {
                    b.HasOne("Domain.Entities.Barbers.Branch", "Branch")
                        .WithMany("Category")
                        .HasForeignKey("branch_id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Branch");
                });

            modelBuilder.Entity("Domain.Entities.Barbers.Service.Service", b =>
                {
                    b.HasOne("Domain.Entities.Barbers.Branch", "Branch")
                        .WithMany("Service")
                        .HasForeignKey("branch_id")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("Domain.Entities.Barbers.Service.Category", "Category")
                        .WithMany("Services")
                        .HasForeignKey("category_id")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.Navigation("Branch");

                    b.Navigation("Category");
                });

            modelBuilder.Entity("Domain.Entities.Orders.Order", b =>
                {
                    b.HasOne("Domain.Entities.Barbers.Branch", "Branch")
                        .WithMany("Orders")
                        .HasForeignKey("branch_id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Domain.Entities.Employee", "Worker")
                        .WithMany("Orders")
                        .HasForeignKey("worker_id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Branch");

                    b.Navigation("Worker");
                });

            modelBuilder.Entity("Domain.Entities.Orders.OrderJob", b =>
                {
                    b.HasOne("Domain.Entities.Barbers.Service.Category", "Category")
                        .WithMany()
                        .HasForeignKey("CategoryId");

                    b.HasOne("Domain.Entities.Orders.Order", "Order")
                        .WithMany("OrderServices")
                        .HasForeignKey("order_id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Domain.Entities.Barbers.Service.Service", "Service")
                        .WithMany()
                        .HasForeignKey("service_id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Category");

                    b.Navigation("Order");

                    b.Navigation("Service");
                });

            modelBuilder.Entity("Domain.ValueObjects.Addresses.Address", b =>
                {
                    b.HasOne("Domain.ValueObjects.Addresses.AddressLocalization", "Localization")
                        .WithMany()
                        .HasForeignKey("localization_id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.OwnsOne("Domain.ValueObjects.Addresses.AddressIdentifier", "Identifier", b1 =>
                        {
                            b1.Property<Guid>("AddressId")
                                .HasColumnType("char(36)");

                            b1.Property<string>("Complement")
                                .IsRequired()
                                .HasMaxLength(20)
                                .HasColumnType("varchar(20)")
                                .HasColumnName("complement");

                            b1.Property<string>("Number")
                                .IsRequired()
                                .HasColumnType("longtext")
                                .HasColumnName("number");

                            b1.HasKey("AddressId");

                            b1.ToTable("address");

                            b1.WithOwner("Address")
                                .HasForeignKey("AddressId");

                            b1.Navigation("Address");
                        });

                    b.Navigation("Identifier")
                        .IsRequired();

                    b.Navigation("Localization");
                });

            modelBuilder.Entity("Domain.ValueObjects.Barbers.UserRoleEntity", b =>
                {
                    b.HasOne("Domain.Entities.User", "User")
                        .WithMany("Roles")
                        .HasForeignKey("user_id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("Domain.ValueObjects.EntityHelper.WorkerServiceRelation", b =>
                {
                    b.HasOne("Domain.Entities.Barbers.Service.Service", null)
                        .WithMany()
                        .HasForeignKey("ServiceId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Domain.Entities.Employee", null)
                        .WithMany()
                        .HasForeignKey("WorkersId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Domain.Entities.Employee", b =>
                {
                    b.HasOne("Domain.Entities.User", null)
                        .WithOne()
                        .HasForeignKey("Domain.Entities.Employee", "Id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Domain.Entities.Barbers.Branch", "Branch")
                        .WithMany("Barber")
                        .HasForeignKey("branch_id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Branch");
                });

            modelBuilder.Entity("Domain.Entities.Barbers.Barber", b =>
                {
                    b.Navigation("Branch");
                });

            modelBuilder.Entity("Domain.Entities.Barbers.Branch", b =>
                {
                    b.Navigation("Barber");

                    b.Navigation("Category");

                    b.Navigation("Configuration")
                        .IsRequired();

                    b.Navigation("Orders");

                    b.Navigation("Schedule");

                    b.Navigation("Service");
                });

            modelBuilder.Entity("Domain.Entities.Barbers.Service.Category", b =>
                {
                    b.Navigation("Services");
                });

            modelBuilder.Entity("Domain.Entities.Orders.Order", b =>
                {
                    b.Navigation("OrderServices");
                });

            modelBuilder.Entity("Domain.Entities.User", b =>
                {
                    b.Navigation("Roles");
                });

            modelBuilder.Entity("Domain.Entities.Employee", b =>
                {
                    b.Navigation("LunchInterval");

                    b.Navigation("Orders");
                });
#pragma warning restore 612, 618
        }
    }
}
