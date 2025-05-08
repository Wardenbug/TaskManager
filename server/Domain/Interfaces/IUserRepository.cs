using Core.Domain.Entities;

namespace Core.Interfaces;

public interface IUserRepository
{
    Task<User> RegisterAsync(User user, string password);
    Task<bool> ExistsByEmail(string email);
    Task<User> FindByEmailAsync(string email);
    Task<bool> CheckPasswordAsync(Guid userId, string password);
    Task UpdateRefreshTokenAsync(Guid userId, string refreshToken, DateTime expiryTime);
    Task<User> FindUserByIdAsync(Guid userId);
}

