using Application.DTOs.User;
using AutoMapper;
using Core.Domain.Entities;
using Core.Interfaces;

namespace Application.Services;

public class UserService(IUserRepository _userRepository, IMapper mapper)
{
    public async Task<UserDto> Register(RegisterUserDto registerUser)
    {
        if (await _userRepository.ExistsByEmail(registerUser.Email))
        {
            throw new Exception("Email already in use.");
        }

        var newUser = await _userRepository.RegisterAsync(mapper.Map<User>(registerUser), registerUser.Password);

        return mapper.Map<UserDto>(newUser);
    }
}

