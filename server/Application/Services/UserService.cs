using Application.DTOs.User;
using Application.Interfaces;
using AutoMapper;
using Core.Domain.Entities;
using Core.Exceptions;
using Core.Interfaces;

namespace Application.Services;

public class UserService(IUserRepository _userRepository, IMapper mapper, ITokenService tokenService)
{
    public async Task<UserDto> Register(RegisterUserDto registerUser)
    {
        if (await _userRepository.ExistsByEmail(registerUser.Email))
        {
            throw new ValidationException("Email already in use.");
        }

        var newUser = await _userRepository.RegisterAsync(mapper.Map<User>(registerUser), registerUser.Password);

        return mapper.Map<UserDto>(newUser);
    }

    public async Task<AuthResponseDto> Login(LoginDto loginDto)
    {
        if (!await _userRepository.ExistsByEmail(loginDto.Email))
        {
            throw new ValidationException("Invalid email or password");
        }

        var user = await _userRepository.FindByEmailAsync(loginDto.Email);

        if (!await _userRepository.CheckPasswordAsync(user.Id, loginDto.Password))
        {
            throw new ValidationException("Invalid email or password");
        }

        var token = tokenService.CreateToken(user);
        var refreshToken = tokenService.CreateRefreshToken();

        await _userRepository.UpdateRefreshTokenAsync(user.Id, refreshToken, DateTime.UtcNow.AddDays(7));

        return new AuthResponseDto { Token = token, RefreshToken = refreshToken, ExpiresAt = DateTime.UtcNow.AddDays(7) };
    }
}

