namespace Infra.Repositories.Company;

internal interface ICategoryRepository
{
    Task<bool> Exists(Guid BranchId, Guid categoryId);
}
