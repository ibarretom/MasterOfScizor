using Application.Services.Branches;
using Domain.Entities.Barbers.Service;
using Domain.Exceptions;
using Domain.Exceptions.Messages;
using DomainTest.Entities.Barber.Services;
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

        var createdService = new Service(Guid.NewGuid(), new Category(string.Empty, Guid.Empty), string.Empty, string.Empty, (decimal) 0.1, (decimal) 0.1, false, false, TimeSpan.Zero);

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
        Assert.True(createdService.Category?.CompanyId.Equals(serviceDTO.Category?.CompanyId));
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
        var createdService = new Service(Guid.NewGuid(), new Category(), string.Empty, string.Empty, (decimal) 0.1, (decimal) 0.1, false, false, TimeSpan.Zero);

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
    public async void ShoudlNotBeAbleToCreateAServiceWithTheSameNameWithCategoryNull()
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
        var serviceUpdated = new Service(serviceDTO.BranchId, new Category(string.Empty, Guid.Empty), string.Empty, string.Empty,   (decimal) 0.1, (decimal) 0.1, false, false, TimeSpan.Zero);
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
        Assert.True(serviceUpdated.BranchId.Equals(serviceDTO.BranchId));
        Assert.True(serviceUpdated.Name.Equals(serviceDTO.Name));
        Assert.True(serviceUpdated.Category?.Id.Equals(serviceDTO.Category?.Id));
        Assert.True(serviceUpdated.Category?.CompanyId.Equals(serviceDTO.Category?.CompanyId));
        Assert.True(serviceUpdated.Category?.Name.Equals(serviceDTO.Category?.Name));
        Assert.True(serviceUpdated.Description.Equals(serviceDTO.Description));
        Assert.True(serviceUpdated.Active.Equals(serviceDTO.Active));
        Assert.True(serviceUpdated.Duration.Equals(serviceDTO.Duration));
        Assert.True(serviceUpdated.Price.Equals(serviceDTO.Price));
        Assert.True(serviceUpdated.PromotionalPrice.Equals(serviceDTO.PromotionalPrice));
        Assert.True(serviceUpdated.IsPromotionActive.Equals(serviceDTO.IsPromotionActive));
    }

    [Fact]
    public async void ShoulNotBeAbleToUpdateAServiceThatDoesntExists()
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
        var service = new Service(serviceDTO.BranchId, new Category(string.Empty, Guid.Empty), string.Empty, string.Empty, (decimal) 1.2, (decimal) 1.2, false, false, TimeSpan.Zero);
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
        var service = ServiceBuilder.Build();
        service.Active = true;

        var serviceRepository = new Mock<IServiceRepository>();
        serviceRepository.Setup(repository => repository.GetById(service.BranchId, service.Id).Result)
            .Returns(service);

        var branchRepository = new Mock<IBranchRepository>();
        branchRepository.Setup(repository => repository.Exists(service.BranchId).Result)
            .Returns(true);

        var categoryRepository = new Mock<ICategoryRepository>();

        var branchService = new BranchService(serviceRepository.Object, branchRepository.Object, categoryRepository.Object);

        await branchService.Disable(service.BranchId, service.Id);

        Assert.True(service.Active.Equals(false));
    }

    [Fact]
    public async void ShouldNotBeAbleToDisableAServiceThatDoesntExists()
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
        var service = ServiceBuilder.Build(Guid.NewGuid());

        var serviceRepository = new Mock<IServiceRepository>();
        serviceRepository.Setup(repository => repository.GetById(service.BranchId, service.Id).Result)
            .Returns(service);

        var branchRepository = new Mock<IBranchRepository>();
        branchRepository.Setup(repository => repository.Exists(service.BranchId).Result)
            .Returns(true);

        var categoryRepository = new Mock<ICategoryRepository>();

        var branchService = new BranchService(serviceRepository.Object, branchRepository.Object, categoryRepository.Object);

        var serviceRetrieved = await branchService.GetSingle(service.BranchId, service.Id);

        Assert.True(serviceRetrieved.Items.Count().Equals(1));
        Assert.True(serviceRetrieved.Items.First().Equals(service));
    }

    [Fact]
    public async void ShouldBeAbleToRetrieveAllServicesRegistred()
    {
        var branchId = Guid.NewGuid();
        var services = new List<Service>() { 
            ServiceBuilder.Build(branchId),
            ServiceBuilder.Build(branchId),
            ServiceBuilder.Build(branchId),
            ServiceBuilder.Build(branchId)
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
}
