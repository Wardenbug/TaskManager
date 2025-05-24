using Application.DTOs.User;
using Application.Interfaces;
using AutoMapper;
using Core.Domain.Entities;
using Core.Exceptions;
using Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace Application.Services;

public class UserService(IUserRepository _userRepository, IMapper mapper, ITokenService tokenService, ILogger<UserService> logger)
{
    public async Task<UserDto> Register(RegisterUserDto registerUser)
    {
        logger.LogInformation("Registering user with email {Email}", registerUser.Email);
        try
        {
            if (await _userRepository.ExistsByEmail(registerUser.Email))
            {
                logger.LogWarning("Email {Email} is already in use", registerUser.Email);
                throw new ValidationException("Email already in use.");
            }

            var newUser = await _userRepository.RegisterAsync(mapper.Map<User>(registerUser), registerUser.Password);
            logger.LogInformation("Successfully registered user with email {Email}", registerUser.Email);

            return mapper.Map<UserDto>(newUser);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during user registration for email {Email}", registerUser.Email);
            throw;
        }
    }

    public async Task<AuthResponseDto> Login(LoginDto loginDto)
    {
        logger.LogInformation("Attempting to log in user with email {Email}", loginDto.Email);

        try
        {
            if (!await _userRepository.ExistsByEmail(loginDto.Email))
            {
                logger.LogWarning("Invalid login attempt for email {Email}: user does not exist", loginDto.Email);
                throw new ValidationException("Invalid email or password");
            }

            var user = await _userRepository.FindByEmailAsync(loginDto.Email);

            if (!await _userRepository.CheckPasswordAsync(user.Id, loginDto.Password))
            {
                logger.LogWarning("Invalid login attempt for email {Email}: incorrect password", loginDto.Email);
                throw new ValidationException("Invalid email or password");
            }

            var token = tokenService.CreateToken(user);
            var refreshToken = tokenService.CreateRefreshToken();

            await _userRepository.UpdateRefreshTokenAsync(user.Id, refreshToken, DateTime.UtcNow.AddDays(7));

            logger.LogInformation("Successfully logged in user {UserId} with email {Email}", user.Id, loginDto.Email);

            return new AuthResponseDto { Token = token, RefreshToken = refreshToken, ExpiresAt = DateTime.UtcNow.AddDays(7) };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during login for email {Email}", loginDto.Email);
            throw;
        }
    }
}

