using AutoMapper;
using Core.Domain.Entities;
using Core.Interfaces;
using Infrastructure.Data.Entities;
using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Data.Repositories;

public class UserRepository(UserManager<ApplicationUser> userManager, IMapper mapper) : IUserRepository
{
    public async Task<bool> ExistsByEmail(string email)
    {
        var user = await userManager.FindByEmailAsync(email);

        return user is not null;
    }

    public async Task<User> RegisterAsync(User user, string password)
    {

        var newUser = mapper.Map<ApplicationUser>(user);
        var result = await userManager.CreateAsync(newUser, password);

        if (!result.Succeeded)
        {
            throw new Exception($"Failed to create user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
        }

        return user;
    }
}
