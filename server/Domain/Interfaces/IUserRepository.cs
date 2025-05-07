using Core.Domain.Entities;

namespace Core.Interfaces;

public interface IUserRepository
{
    Task<User> RegisterAsync(User user, string password);
    Task<bool> ExistsByEmail(string email);
}

