using Domain.Entities;

namespace Infra.Repositories.UserRepository;

public interface IUserRepository
{
    Task Create(User user);
    Task<User> FindByCredential(string credential);
}
