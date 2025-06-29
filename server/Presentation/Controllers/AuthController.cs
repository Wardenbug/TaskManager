﻿using Application.DTOs.User;
using Application.Services;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Presentation.DTOs;

namespace Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(UserService userService, IMapper mapper, ILogger<AuthController> logger) : ControllerBase
{

    /// <summary>
    /// Registers a new user.
    /// </summary>
    /// <param name="registerData">Registration data.</param>
    /// <param name="cancellationToken">Cancellation token.</param> 
    /// <returns>Registered user info.</returns>
    [ProducesResponseType(typeof(UserDto), 200)]
    [ProducesResponseType(typeof(ErrorResponseDto), 400)]
    [ProducesResponseType(typeof(ErrorResponseDto), 500)]
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto registerData, CancellationToken cancellationToken)
    {
        logger.LogInformation("Registering new user with username: {UserName}", registerData.UserName);

        try
        {
            var user = await userService.Register(mapper.Map<RegisterUserDto>(registerData), cancellationToken);
            logger.LogInformation("Sucessfully registered a new user with username {UserName}", user.UserName);

            return Ok(user);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during user registration for username: {UserName}", registerData.UserName);
            throw;
        }

    }

    /// <summary>
    /// Authenticates a user and returns a JWT token.
    /// </summary>
    /// <param name="loginData">Login data.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Authentication response.</returns>
    [ProducesResponseType(typeof(AuthResponseDto), 200)]
    [ProducesResponseType(typeof(ErrorResponseDto), 400)]
    [ProducesResponseType(typeof(ErrorResponseDto), 401)]
    [ProducesResponseType(typeof(ErrorResponseDto), 500)]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto loginData, CancellationToken cancellationToken)
    {
        logger.LogInformation("User attempting to log in with email: {Email}", loginData.Email);
        try
        {
            var response = await userService.Login(mapper.Map<LoginDto>(loginData), cancellationToken);

            if (response is null)
            {
                logger.LogWarning("Login failed for user with email: {Email}", loginData.Email);
                return BadRequest(response);
            }

            logger.LogInformation("Successfully logged in user with email: {Email}", loginData.Email);
            return Ok(response);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during user login for email: {Email}", loginData.Email);
            throw;
        }

    }
}

