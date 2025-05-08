using Application.DTOs.User;
using Application.Interfaces;
using Application.Services;
using AutoMapper;
using Core.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Presentation.DTOs;

namespace Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(ITokenService _tokenService, UserService userService, IMapper mapper) : ControllerBase
{

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto registerData)
    {
        var user = await userService.Register(mapper.Map<RegisterUserDto>(registerData));
        return Ok(user);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto loginData)
    {
        var response = await userService.Login(mapper.Map<LoginDto>(loginData));

        if (response is null)
        {
            return BadRequest(response);
        }

        return Ok(response);
    }
}

