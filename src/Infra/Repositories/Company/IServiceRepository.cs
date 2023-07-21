﻿using Domain.Entities.Barbers.Service;

namespace Infra.Repositories.Company;

internal interface IServiceRepository
{
    Task Add(Service service);
    Task<bool> Exists(Guid branchId, string name, Guid categoryId);
}
