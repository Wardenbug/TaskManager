using Application.DTOs.User;
using Application.Interfaces;
using AutoMapper;
using Core.Domain.Entities;
using Core.Exceptions;
using Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace Application.Services;

public class UserService(
    IUserRepository _userRepository,
    IMapper mapper,
    ICurrentUserProvider _currentUserProvider,
    ITokenService tokenService, ILogger<UserService> logger)
{
    public async Task<UserDto> Register(RegisterUserDto registerUser, CancellationToken cancellationToken)
    {
        logger.LogInformation("Registering user with email {Email}", registerUser.Email);
        try
        {
            if (await _userRepository.ExistsByEmail(registerUser.Email, cancellationToken))
            {
                logger.LogWarning("Email {Email} is already in use", registerUser.Email);
                throw new ValidationException("Email already in use.");
            }

            var newUser = await _userRepository.RegisterAsync(mapper.Map<User>(registerUser), registerUser.Password, cancellationToken);
            logger.LogInformation("Successfully registered user with email {Email}", registerUser.Email);

            return mapper.Map<UserDto>(newUser);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during user registration for email {Email}", registerUser.Email);
            throw;
        }
    }

    public async Task<AuthResponseDto> Login(LoginDto loginDto, CancellationToken cancellationToken)
    {
        logger.LogInformation("Attempting to log in user with email {Email}", loginDto.Email);

        try
        {
            if (!await _userRepository.ExistsByEmail(loginDto.Email, cancellationToken))
            {
                logger.LogWarning("Invalid login attempt for email {Email}: user does not exist", loginDto.Email);
                throw new ValidationException("Invalid email or password");
            }

            var user = await _userRepository.FindByEmailAsync(loginDto.Email, cancellationToken);

            if (!await _userRepository.CheckPasswordAsync(user.Id, loginDto.Password, cancellationToken))
            {
                logger.LogWarning("Invalid login attempt for email {Email}: incorrect password", loginDto.Email);
                throw new ValidationException("Invalid email or password");
            }

            var token = tokenService.CreateToken(user);
            var refreshToken = tokenService.CreateRefreshToken();

            await _userRepository.UpdateRefreshTokenAsync(user.Id, refreshToken, DateTime.UtcNow.AddDays(7), cancellationToken);

            logger.LogInformation("Successfully logged in user {UserId} with email {Email}", user.Id, loginDto.Email);

            return new AuthResponseDto
            {
                Id = user.Id,
                UserName = user.UserName,
                Token = token,
                RefreshToken = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddDays(7)
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during login for email {Email}", loginDto.Email);
            throw;
        }
    }

    public async Task<UserDto> GetCurrentUser(CancellationToken cancellationToken)
    {
        logger.LogInformation("Attempting to get current user");
        try
        {
            var userId = _currentUserProvider.GetCurrentUserId();
            logger.LogInformation("Retrieving user with ID {UserId}", userId);

            if (userId is null)
            {
                logger.LogWarning("Current user ID is null");
                throw new ValidationException("Current user not found.");
            }

            var user = await _userRepository.FindUserByIdAsync(Guid.Parse(userId), cancellationToken);
            if (user is null)
            {
                logger.LogWarning("User with ID {UserId} not found in repository", userId);
                throw new ValidationException("Current user not found.");
            }

            logger.LogInformation("Successfully retrieved current user with ID {UserId}", userId);

            return mapper.Map<UserDto>(user);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during getting current user");
            throw;
        }
    }

    public async Task<AuthResponseDto> RefreshToken(string refreshToken, CancellationToken cancellationToken)
    {
        logger.LogInformation("Attempting to refresh token");
        try
        {
            var user = await _userRepository.GetUserByRefreshTokenAsync(refreshToken, cancellationToken);

            if (user is null)
            {
                logger.LogWarning("Invalid refresh token provided");
                throw new ValidationException("Invalid refresh token");
            }

            var newToken = tokenService.CreateToken(user);
            var newRefreshToken = tokenService.CreateRefreshToken();

            await _userRepository.UpdateRefreshTokenAsync(user.Id, newRefreshToken, DateTime.UtcNow.AddDays(7), cancellationToken);

            logger.LogInformation("Successfully refreshed token for user {UserId}", user.Id);

            return new AuthResponseDto
            {
                Id = user.Id,
                UserName = user.UserName,
                Token = newToken,
                RefreshToken = newRefreshToken,
                ExpiresAt = DateTime.UtcNow.AddDays(7)
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during token refresh");
            throw;
        }
    }
}

