using AutoMapper;
using Core.Domain.Entities;
using Core.Exceptions;
using Core.Interfaces;
using Infrastructure.Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Data.Repositories;

public class UserRepository(UserManager<ApplicationUser> userManager, IMapper mapper, ILogger<UserRepository> logger) : IUserRepository
{
    public async Task<bool> CheckPasswordAsync(Guid userId, string password, CancellationToken cancellationToken)
    {
        logger.LogInformation("Checking password for user with ID {UserId}", userId);
        try
        {
            var user = await userManager.FindByIdAsync(userId.ToString());
            var result = await userManager.CheckPasswordAsync(user, password);

            logger.LogDebug("Sucessgully checked password for user with ID {UserId}", userId);
            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error checking password for user with ID {UserId}", userId);
            throw;
        }

    }

    public async Task<bool> ExistsByEmail(string email, CancellationToken cancellationToken)
    {
        logger.LogDebug("Checking if user exists with email {Email}", email);
        try
        {
            var user = await userManager.FindByEmailAsync(email);

            logger.LogDebug("Sucessfully checked if user exists with email {Email}", email);

            return user is not null;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error checking if user exists with email {Email}", email);
            throw;
        }

    }

    public async Task<User> FindByEmailAsync(string email, CancellationToken cancellationToken)
    {
        logger.LogDebug("Finding user by email {Email}", email);
        try
        {
            var user = await userManager.FindByEmailAsync(email);

            logger.LogDebug("Successfully found user by email {Email}", email);
            return mapper.Map<User>(user);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error finding user by email {Email}", email);
            throw;
        }
    }

    public Task<User> FindUserByIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public async Task<User> RegisterAsync(User user, string password, CancellationToken cancellationToken)
    {
        logger.LogInformation("Registering user with email {Email}", user.Email);
        try
        {
            var result = await userManager.CreateAsync(mapper.Map<ApplicationUser>(user), password);

            if (!result.Succeeded)
            {
                logger.LogWarning("Failed to create user with email {Email}: {Errors}", user.Email, string.Join(", ", result.Errors.Select(e => e.Description)));
                throw new ValidationException($"Failed to create user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }

            logger.LogInformation("Successfully registered user with email {Email}", user.Email);
            return user;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error registering user with email {Email}", user.Email);
            throw;
        }

    }

    public async Task UpdateRefreshTokenAsync(Guid userId, string refreshToken, DateTime expiryTime, CancellationToken cancellationToken)
    {
        logger.LogInformation("Updating refresh token for user with ID {UserId}", userId);
        try
        {
            var user = await userManager.FindByIdAsync(userId.ToString());
            if (user is not null)
            {
                user.RefreshToken = refreshToken;
                user.RefreshTokenExpiryTime = expiryTime;
                await userManager.UpdateAsync(user);
                logger.LogInformation("Successfully updated refresh token for user with ID {UserId}", userId);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating refresh token for user with ID {UserId}", userId);
            throw;
        }

    }
}
