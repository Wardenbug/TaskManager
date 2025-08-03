using Application.DTOs.User;
using Application.Services;
using AutoMapper;
using Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Presentation.DTOs;

namespace Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController(
    UserService userService,
    IMapper mapper,
    ILogger<UsersController> logger,
    ICurrentUserProvider currentUserProvider) : ControllerBase
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


            Response.Cookies.Append("access_token", response.Token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Lax,
                Expires = DateTimeOffset.UtcNow.AddMinutes(1),
                Path = "/"
            });

            Response.Cookies.Append("refresh_token", response.RefreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Lax,
                Expires = DateTimeOffset.UtcNow.AddDays(7),
                Path = "api/users/refresh"
            });


            if (response is null)
            {
                logger.LogWarning("Login failed for user with email: {Email}", loginData.Email);
                return BadRequest(response);
            }

            logger.LogInformation("Successfully logged in user with email: {Email}", loginData.Email);
            return Ok(new UserDto { Id = response.Id, UserName = response.UserName });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during user login for email: {Email}", loginData.Email);
            throw;
        }

    }

    /// <summary>
    /// Gets the current authenticated user's information.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Current user info.</returns>
    [Authorize]
    [ProducesResponseType(typeof(UserDto), 200)]
    [ProducesResponseType(typeof(ErrorResponseDto), 401)]
    [ProducesResponseType(typeof(ErrorResponseDto), 404)]
    [ProducesResponseType(typeof(ErrorResponseDto), 500)]
    [HttpGet("me")]
    public async Task<IActionResult> GetUserById(CancellationToken cancellationToken)
    {
        logger.LogInformation("Attempting to get current user information.");
        try
        {
            var userId = currentUserProvider.GetCurrentUserId();
            logger.LogInformation("Current user ID from provider: {UserId}", userId);

            var user = await userService.GetCurrentUser(cancellationToken);

            logger.LogInformation("Successfully retrieved user information for user ID: {UserId}", userId);

            return Ok(user);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving current user information.");
            throw;
        }
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh(CancellationToken cancellationToken)
    {
        logger.LogInformation("Refresh token request received");
        try
        {
            var refreshToken = Request.Cookies["refresh_token"];

            if (string.IsNullOrEmpty(refreshToken))
            {
                logger.LogWarning("No refresh token found in cookies");
                return Unauthorized();
            }

            var response = await userService.RefreshToken(refreshToken, cancellationToken);

            Response.Cookies.Append("access_token", response.Token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Lax,
                Expires = DateTimeOffset.UtcNow.AddMinutes(1),
                Path = "/"
            });

            Response.Cookies.Append("refresh_token", response.RefreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Lax,
                Expires = DateTimeOffset.UtcNow.AddDays(7),
                Path = "api/users/refresh"
            });

            logger.LogInformation("Successfully refreshed token");
            return Ok(new UserDto { Id = response.Id, UserName = response.UserName });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during token refresh");
            throw;
        }
    }
}

