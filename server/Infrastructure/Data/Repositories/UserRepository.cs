using AutoMapper;
using Core.Domain.Entities;
using Core.Interfaces;
using Infrastructure.Data.Entities;
using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Data.Repositories;

public class UserRepository(UserManager<ApplicationUser> userManager, IMapper mapper) : IUserRepository
{
    public async Task<bool> CheckPasswordAsync(Guid userId, string password)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        var result = await userManager.CheckPasswordAsync(user, password);

        return result;
    }

    public async Task<bool> ExistsByEmail(string email)
    {
        var user = await userManager.FindByEmailAsync(email);

        return user is not null;
    }

    public async Task<User> FindByEmailAsync(string email)
    {
        var user = await userManager.FindByEmailAsync(email);
        return mapper.Map<User>(user);
    }

    public Task<User> FindUserByIdAsync(Guid userId)
    {
        throw new NotImplementedException();
    }

    public async Task<User> RegisterAsync(User user, string password)
    {
        var result = await userManager.CreateAsync(mapper.Map<ApplicationUser>(user), password);

        if (!result.Succeeded)
        {
            throw new Exception($"Failed to create user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
        }

        return user;
    }

    public async Task UpdateRefreshTokenAsync(Guid userId, string refreshToken, DateTime expiryTime)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is not null)
        {
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = expiryTime;
            await userManager.UpdateAsync(user);
        }
    }
}
