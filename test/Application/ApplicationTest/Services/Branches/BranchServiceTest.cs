using Application.Services.Branches;
using Domain.Entities.Barbers;
using Domain.Entities.Barbers.Service;
using Domain.Exceptions;
using Domain.Exceptions.Messages;
using Domain.ValueObjects.DTO.Barber;
using DomainTest.Entities;
using DomainTest.Entities.Barbers;
using DomainTest.Entities.Barbers.Services;
using DomainTest.ValueObjects.DTO;
using Infra.Repositories.Company;
using Infra.Repositories.CompanyRepository;
using Moq;

namespace ApplicationTest.Services.Branches;

public class BranchServiceTest
{
    [Fact]
    public async void ShouldBeAbleToAddANewService()
    {
        var serviceDTO = ServiceRequestDTOBuilder.Build();

        var createdService = new Service(new Category(string.Empty), string.Empty, string.Empty, (decimal) 0.1, (decimal) 0.1, false, false, TimeSpan.Zero);

        var serviceRepository = new Mock<IServiceRepository>();
        serviceRepository
            .Setup(repository => repository.Add(It.IsAny<Service>()))
            .Callback<Service>((service) =>
            {
                createdService = service;
            });

        var branchRepository = new Mock<IBranchRepository>();
        branchRepository.Setup(repository => repository.Exists(It.IsAny<Guid>())).ReturnsAsync(true);

        var categoryRepository = new Mock<ICategoryRepository>();

        var branchService = new BranchService(serviceRepository.Object, branchRepository.Object, categoryRepository.Object);

        await branchService.Add(serviceDTO);

        Assert.True(createdService.Name.Equals(serviceDTO.Name));
        Assert.True(!createdService.Category?.Id.Equals(Guid.Empty));
        Assert.True(createdService.Category?.Name.Equals(serviceDTO.Category?.Name));
        Assert.True(createdService.Description.Equals(serviceDTO.Description));
        Assert.True(createdService.Active.Equals(serviceDTO.Active));
        Assert.True(createdService.Duration.Equals(serviceDTO.Duration));
        Assert.True(createdService.Price.Equals(serviceDTO.Price));
        Assert.True(createdService.PromotionalPrice.Equals(serviceDTO.PromotionalPrice));
        Assert.True(createdService.IsPromotionActive.Equals(serviceDTO.IsPromotionActive));
    }

    [Fact]
    public async void ShouldBeAbleToCreateANewServiceWithACategoryEmpty()
    {
        var serviceDTO = ServiceRequestDTOBuilder.Build();
        serviceDTO.Category = null;
        var createdService = new Service(new Category(), string.Empty, string.Empty, (decimal) 0.1, (decimal) 0.1, false, false, TimeSpan.Zero);

        var serviceRepository = new Mock<IServiceRepository>();
        serviceRepository.Setup(repository => repository.Add(It.IsAny<Service>()))
            .Callback<Service>((service) =>
            {
                createdService = service;
            });

        var branchRepository = new Mock<IBranchRepository>();
        branchRepository.Setup(repository => repository.Exists(It.IsAny<Guid>())).ReturnsAsync(true);

        var categoryRepository = new Mock<ICategoryRepository>();

        var branchService = new BranchService(serviceRepository.Object, branchRepository.Object, categoryRepository.Object);

        await branchService.Add(serviceDTO);

        Assert.True(createdService.Category is null);
    }

    [Fact]
    public async void ShouldNotBeAbleToCreateANewServiceWithAnInexistentBranch()
    {
        var repository = new Mock<IServiceRepository>();

        var branchRepository = new Mock<IBranchRepository>();

        var categoryRepository = new Mock<ICategoryRepository>();

        var branchService = new BranchService(repository.Object, branchRepository.Object, categoryRepository.Object);
        branchRepository.Setup(repository => repository.Exists(It.IsAny<Guid>())).ReturnsAsync(false);

        var serviceDTO = ServiceRequestDTOBuilder.Build();

        var exception = await Assert.ThrowsAsync<CompanyException>(() => branchService.Add(serviceDTO));

        Assert.True(exception.Message.Equals(CompanyExceptionMessagesResource.BRANCH_NOT_FOUND));
    }

    [Fact]
    public async void ShouldNotBeAbleToCreateANewServiceWithBranchIdEmpty()
    {
        var repository = new Mock<IServiceRepository>();

        var branchRepository = new Mock<IBranchRepository>();
        var categoryRepository = new Mock<ICategoryRepository>();

        var branchService = new BranchService(repository.Object, branchRepository.Object, categoryRepository.Object);
        branchRepository.Setup(repository => repository.Exists(Guid.Empty)).ReturnsAsync(false);


        var serviceDTO = ServiceRequestDTOBuilder.Build();
        serviceDTO.BranchId = Guid.Empty;

        var exception = await Assert.ThrowsAsync<CompanyException>(() => branchService.Add(serviceDTO));

        Assert.True(exception.Message.Equals(CompanyExceptionMessagesResource.BRANCH_NOT_FOUND));
    }

    [Fact]
    public async void ShouldNotBeAbleToCreateAServiceWithTheSameName()
    {
        var serviceRepository = new Mock<IServiceRepository>();
        serviceRepository.Setup(repository => repository.Exists(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<Guid>()).Result).Returns(true);

        var branchRepository = new Mock<IBranchRepository>();
        branchRepository.Setup(repository => repository.Exists(It.IsAny<Guid>()).Result).Returns(true);

        var categoryRepository = new Mock<ICategoryRepository>();

        var branchService = new BranchService(serviceRepository.Object, branchRepository.Object, categoryRepository.Object);

        var serviceDTO = ServiceRequestDTOBuilder.Build();

        var exception = await Assert.ThrowsAsync<CompanyException>(() => branchService.Add(serviceDTO));

        Assert.True(exception.Message.Equals(CompanyExceptionMessagesResource.SERVICE_ALREADY_EXISTS));
    }

    [Fact]
    public async void ShouldNotBeAbleToCreateAServiceWithTheSameNameWithCategoryNull()
    {
        var serviceRepository = new Mock<IServiceRepository>();
        serviceRepository.Setup(repository => repository.Exists(It.IsAny<Guid>(), It.IsAny<string>(), Guid.Empty).Result).Returns(true);

        var branchRepository = new Mock<IBranchRepository>();
        branchRepository.Setup(repository => repository.Exists(It.IsAny<Guid>()).Result).Returns(true);

        var categoryRepository = new Mock<ICategoryRepository>();

        var branchService = new BranchService(serviceRepository.Object, branchRepository.Object, categoryRepository.Object);

        var serviceDTO = ServiceRequestDTOBuilder.Build();
        serviceDTO.Category = null;

        var exception = await Assert.ThrowsAsync<CompanyException>(() => branchService.Add(serviceDTO));

        Assert.True(exception.Message.Equals(CompanyExceptionMessagesResource.SERVICE_ALREADY_EXISTS));
    }

    [Fact]
    public async void ShouldBeAbleToUpdateAService()
    {
        var serviceDTO = ServiceUpdateRequestDTOBuilder.Build();
        var serviceUpdated = new Service(new Category(string.Empty), string.Empty, string.Empty,   (decimal) 0.1, (decimal) 0.1, false, false, TimeSpan.Zero);
        serviceDTO.Id = serviceUpdated.Id;

        var serviceRepository = new Mock<IServiceRepository>();
        serviceRepository.Setup(repository => repository.GetById(serviceDTO.BranchId, serviceDTO.Id).Result)
            .Returns(serviceUpdated);
        serviceRepository.Setup(repository => repository.Add(It.IsAny<Service>()));

        var categoryRepository = new Mock<ICategoryRepository>();
        categoryRepository.Setup(repository => repository.Exists(serviceDTO.BranchId, It.IsAny<Guid>()).Result).Returns(true);

        var branchRepository = new Mock<IBranchRepository>();

        var branchService = new BranchService(serviceRepository.Object, branchRepository.Object, categoryRepository.Object);

        await branchService.Update(serviceDTO);

        Assert.True(serviceUpdated.Id.Equals(serviceDTO.Id));
        Assert.True(serviceUpdated.Name.Equals(serviceDTO.Name));
        Assert.True(serviceUpdated.Category?.Id.Equals(serviceDTO.Category?.Id));
        Assert.True(serviceUpdated.Category?.Name.Equals(serviceDTO.Category?.Name));
        Assert.True(serviceUpdated.Description.Equals(serviceDTO.Description));
        Assert.True(serviceUpdated.Active.Equals(serviceDTO.Active));
        Assert.True(serviceUpdated.Duration.Equals(serviceDTO.Duration));
        Assert.True(serviceUpdated.Price.Equals(serviceDTO.Price));
        Assert.True(serviceUpdated.PromotionalPrice.Equals(serviceDTO.PromotionalPrice));
        Assert.True(serviceUpdated.IsPromotionActive.Equals(serviceDTO.IsPromotionActive));
    }

    [Fact]
    public async void ShouldNotBeAbleToUpdateAServiceThatDoesNotExists()
    {
        var serviceRepository = new Mock<IServiceRepository>();
        serviceRepository.Setup(repository => repository.GetById(It.IsAny<Guid>(), It.IsAny<Guid>()).Result)
            .Returns<Service>(null);
        var categoryRepository = new Mock<ICategoryRepository>();

        var branchRepository = new Mock<IBranchRepository>();

        var branchService = new BranchService(serviceRepository.Object, branchRepository.Object, categoryRepository.Object);

        var serviceDTO = ServiceUpdateRequestDTOBuilder.Build();

        var exception = await Assert.ThrowsAsync<CompanyException>(() => branchService.Update(serviceDTO));
        Assert.True(exception.Message.Equals(CompanyExceptionMessagesResource.SERVICE_NOT_FOUND));
    }

    [Fact]
    public async void ShouldNotBeAbleToUpdateAServiceWithAInexistentCategory()
    {
        var serviceDTO = ServiceUpdateRequestDTOBuilder.Build();
        var service = new Service(new Category(string.Empty), string.Empty, string.Empty, (decimal) 1.2, (decimal) 1.2, false, false, TimeSpan.Zero);
        serviceDTO.Id = service.Id;

        var serviceRepository = new Mock<IServiceRepository>();
        serviceRepository.Setup(repository => repository.GetById(serviceDTO.BranchId, serviceDTO.Id).Result).Returns(service);

        var branchRepository = new Mock<IBranchRepository>();
        branchRepository.Setup(repository => repository.Exists(serviceDTO.BranchId).Result).Returns(true);

        var categoryRepository = new Mock<ICategoryRepository>();
        categoryRepository.Setup(repository => repository.Exists(It.IsAny<Guid>(), It.IsAny<Guid>()).Result).Returns(false);

        var branchService = new BranchService(serviceRepository.Object, branchRepository.Object, categoryRepository.Object);

        var exception = await Assert.ThrowsAsync<ServiceException>(() => branchService.Update(serviceDTO));

        Assert.True(exception.Message.Equals(ServiceExceptionMessagesResource.CATEGORY_NOT_FOUND));
    }

    [Fact]
    public async void ShouldBeAbleToDisableAService()
    {
        var branchId = Guid.NewGuid();
        var service = ServiceBuilder.Build();
        service.Active = true;

        var serviceRepository = new Mock<IServiceRepository>();
        serviceRepository.Setup(repository => repository.GetById(branchId, service.Id).Result)
            .Returns(service);

        var branchRepository = new Mock<IBranchRepository>();
        branchRepository.Setup(repository => repository.Exists(branchId).Result)
            .Returns(true);

        var categoryRepository = new Mock<ICategoryRepository>();

        var branchService = new BranchService(serviceRepository.Object, branchRepository.Object, categoryRepository.Object);

        await branchService.Disable(branchId, service.Id);

        Assert.True(service.Active.Equals(false));
    }

    [Fact]
    public async void ShouldNotBeAbleToDisableAServiceThatDoesNotExists()
    {
        var branchId = Guid.NewGuid();
        var serviceId = Guid.NewGuid();

        var serviceRepository = new Mock<IServiceRepository>();
        serviceRepository.Setup(repository => repository.GetById(branchId, serviceId).Result).Returns<Service>(null);

        var branchRepository = new Mock<IBranchRepository>();
        branchRepository.Setup(repository => repository.Exists(branchId).Result).Returns(true);

        var categoryRepository = new Mock<ICategoryRepository>();

        var branchService = new BranchService(serviceRepository.Object, branchRepository.Object, categoryRepository.Object);

        var exception = await Assert.ThrowsAsync<CompanyException>(() => branchService.Disable(branchId, serviceId));
        Assert.True(exception.Message.Equals(CompanyExceptionMessagesResource.SERVICE_NOT_FOUND));
    }

    [Fact]
    public async void ShouldNotBeAbleToDisableAServiceWithBranchInexistent()
    {
        var branchId = Guid.NewGuid();
        var serviceId = Guid.NewGuid();

        var serviceRepository = new Mock<IServiceRepository>();

        var branchRepository = new Mock<IBranchRepository>();
        branchRepository.Setup(repository => repository.Exists(branchId).Result).Returns(false);

        var categoryRepository = new Mock<ICategoryRepository>();

        var branchService = new BranchService(serviceRepository.Object, branchRepository.Object, categoryRepository.Object);

        var exception = await Assert.ThrowsAsync<CompanyException>(() => branchService.Disable(branchId, serviceId));
        Assert.True(exception.Message.Equals(CompanyExceptionMessagesResource.BRANCH_NOT_FOUND));
    }

    [Fact]
    public async void ShouldBeAbleToRetrieveAServiceById()
    {
        var branchId = Guid.NewGuid();
        var service = ServiceBuilder.Build();

        var serviceRepository = new Mock<IServiceRepository>();
        serviceRepository.Setup(repository => repository.GetById(branchId, service.Id).Result)
            .Returns(service);

        var branchRepository = new Mock<IBranchRepository>();
        branchRepository.Setup(repository => repository.Exists(branchId).Result)
            .Returns(true);

        var categoryRepository = new Mock<ICategoryRepository>();

        var branchService = new BranchService(serviceRepository.Object, branchRepository.Object, categoryRepository.Object);

        var serviceRetrieved = await branchService.GetSingle(branchId, service.Id);

        Assert.True(serviceRetrieved.Items.Count().Equals(1));
        Assert.True(serviceRetrieved.Items.First().Equals(service));
    }

    [Fact]
    public async void ShouldBeAbleToRetrieveAllServicesRegistered()
    {
        var branchId = Guid.NewGuid();
        var services = new List<Service>() { 
            ServiceBuilder.Build(),
            ServiceBuilder.Build(),
            ServiceBuilder.Build(),
            ServiceBuilder.Build()
        };

        var serviceRepository = new Mock<IServiceRepository>();
        serviceRepository.Setup(repository => repository.GetAll(branchId).Result)
            .Returns(services);
        
        var branchRepository = new Mock<IBranchRepository>();
        branchRepository.Setup(repository => repository.Exists(branchId).Result)
            .Returns(true);
        
        var categoryRepository = new Mock<ICategoryRepository>();
        
        var branchService = new BranchService(serviceRepository.Object, branchRepository.Object, categoryRepository.Object);
        
        var servicesRetrieved = await branchService.GetAll(branchId);
        
        Assert.True(servicesRetrieved.Items.Count().Equals(4));
        
        foreach (var service in servicesRetrieved.Items)
        {
            Assert.Contains(service, services);
        }
    }

    [Fact]
    public async Task ShouldBeAbleToAddALunchScheduleForAWorker()
    {
        var configuration = ConfigurationBuilder.BuildWithScheduleWithNoDelay();
        var branch = BranchBuilder.Build(configuration, true);
        branch.AddSchedule(new Schedule(new TimeOnly(9, 0), new TimeOnly(18, 0), DayOfWeek.Monday));

        var employee = EmployeeBuilder.Build();
        branch.AddEmployee(employee);

        var branchRepository = new Mock<IBranchRepository>();
        branchRepository.Setup(repository => repository.GetBy(branch.Id).Result).Returns(branch);

        var serviceRepository = new Mock<IServiceRepository>();

        var categoryRepository = new Mock<ICategoryRepository>();

        var branchService = new BranchService(serviceRepository.Object, branchRepository.Object, categoryRepository.Object);

        var lunchInterval = new LunchIntervalDTO(new TimeOnly(12, 0), new TimeOnly(13, 0), DayOfWeek.Monday, branch.Id, employee.Id);

        await branchService.Add(lunchInterval);

        Assert.Contains(new Schedule(lunchInterval.StartTime, lunchInterval.EndTime, lunchInterval.WeekDay), employee.LunchInterval);
    }

    [Fact]
    public async Task ShouldThrowsWhenBranchIsNotFound()
    {
        var configuration = ConfigurationBuilder.BuildWithScheduleWithNoDelay();
        var branch = BranchBuilder.Build(configuration, true);
        branch.AddSchedule(new Schedule(new TimeOnly(9, 0), new TimeOnly(18, 0), DayOfWeek.Monday));

        var employee = EmployeeBuilder.Build();
        branch.AddEmployee(employee);

        var branchRepository = new Mock<IBranchRepository>();

        var serviceRepository = new Mock<IServiceRepository>();

        var categoryRepository = new Mock<ICategoryRepository>();

        var branchService = new BranchService(serviceRepository.Object, branchRepository.Object, categoryRepository.Object);

        var lunchInterval = new LunchIntervalDTO(new TimeOnly(12, 0), new TimeOnly(13, 0), DayOfWeek.Monday, branch.Id, employee.Id);

        var exception = await Assert.ThrowsAsync<CompanyException>(() => branchService.Add(lunchInterval));

        Assert.True(exception.Message.Equals(CompanyExceptionMessagesResource.BRANCH_NOT_FOUND));
    }
}
