using Core.Domain.Entities;

namespace Core.Interfaces;

public interface IUserRepository
{
    Task<User> RegisterAsync(User user, string password, CancellationToken cancellationToken);
    Task<bool> ExistsByEmail(string email, CancellationToken cancellationToken);
    Task<User> FindByEmailAsync(string email, CancellationToken cancellationToken);
    Task<bool> CheckPasswordAsync(Guid userId, string password, CancellationToken cancellationToken);
    Task UpdateRefreshTokenAsync(Guid userId, string refreshToken, DateTime expiryTime, CancellationToken cancellationToken);
    Task<User> FindUserByIdAsync(Guid userId, CancellationToken cancellationToken);
}

