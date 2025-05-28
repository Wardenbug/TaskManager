using Application.DTOs.User;
using Application.Interfaces;
using AutoMapper;
using Core.Domain.Entities;
using Core.Exceptions;
using Core.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;
using Application.Services;

namespace Application.Tests;

public class UserServiceTests
{
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<ITokenService> _mockTokenService;
    private readonly Mock<ILogger<UserService>> _mockLogger;
    private readonly UserService _userService;

    public UserServiceTests()
    {
        _mockUserRepository = new Mock<IUserRepository>();
        _mockMapper = new Mock<IMapper>();
        _mockTokenService = new Mock<ITokenService>();
        _mockLogger = new Mock<ILogger<UserService>>();

        _userService = new UserService(
            _mockUserRepository.Object,
            _mockMapper.Object,
            _mockTokenService.Object,
            _mockLogger.Object
        );
    }


    // Register tests
    [Fact]
    public async Task Register_ReturnsUserDto_WhenRegistrationIsSuccessful()
    {
        // Arrange
        var registerUserDto = new RegisterUserDto { Email = "test@example.com", UserName = "TestUser", Password = "Password123!" };
        var user = new User { Id = Guid.NewGuid(), Email = "test@example.com", UserName = "TestUser" };
        var userDto = new UserDto { Id = user.Id, UserName = user.UserName };

        _mockUserRepository.Setup(r => r.ExistsByEmail(registerUserDto.Email, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _mockMapper.Setup(m => m.Map<User>(registerUserDto)).Returns(user);
        _mockUserRepository.Setup(r => r.RegisterAsync(It.IsAny<User>(), registerUserDto.Password, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _mockMapper.Setup(m => m.Map<UserDto>(user)).Returns(userDto);

        // Act
        var result = await _userService.Register(registerUserDto, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(userDto.UserName, result.UserName);
    }

    [Fact]
    public async Task Register_ThrowsValidationException_WhenEmailAlreadyInUse()
    {
        // Arrange
        var registerUserDto = new RegisterUserDto { Email = "existing@example.com", UserName = "ExistingUser", Password = "Password123!" };

        _mockUserRepository.Setup(r => r.ExistsByEmail(registerUserDto.Email, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() => _userService.Register(registerUserDto, CancellationToken.None));
    }

    [Fact]
    public async Task Register_ThrowsException_WhenRepositoryFails()
    {
        // Arrange
        var registerUserDto = new RegisterUserDto { Email = "test@example.com", UserName = "TestUser", Password = "Password123!" };
        var expectedException = new InvalidOperationException("Database error");

        _mockUserRepository.Setup(r => r.ExistsByEmail(registerUserDto.Email, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _mockMapper.Setup(m => m.Map<User>(registerUserDto)).Returns(new User());
        _mockUserRepository.Setup(r => r.RegisterAsync(It.IsAny<User>(), registerUserDto.Password, It.IsAny<CancellationToken>())).ThrowsAsync(expectedException);

        // Act & Assert
        var caughtException = await Assert.ThrowsAsync<InvalidOperationException>(() => _userService.Register(registerUserDto, CancellationToken.None));
        Assert.Equal(expectedException, caughtException);
    }

    // Login Tests

    [Fact]
    public async Task Login_ReturnsAuthResponseDto_WhenLoginIsSuccessful()
    {
        // Arrange
        var loginDto = new LoginDto { Email = "test@example.com", Password = "Password123!" };
        var user = new User { Id = Guid.NewGuid(), Email = "test@example.com", UserName = "TestUser" };
        var token = "some_jwt_token";
        var refreshToken = "some_refresh_token";

        _mockUserRepository.Setup(r => r.ExistsByEmail(loginDto.Email, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _mockUserRepository.Setup(r => r.FindByEmailAsync(loginDto.Email, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _mockUserRepository.Setup(r => r.CheckPasswordAsync(user.Id, loginDto.Password, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _mockTokenService.Setup(s => s.CreateToken(user)).Returns(token);
        _mockTokenService.Setup(s => s.CreateRefreshToken()).Returns(refreshToken);
        _mockUserRepository.Setup(r => r.UpdateRefreshTokenAsync(user.Id, refreshToken, It.IsAny<DateTime>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        // Act
        var result = await _userService.Login(loginDto, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(token, result.Token);
        Assert.Equal(refreshToken, result.RefreshToken);
        Assert.True(result.ExpiresAt > DateTime.UtcNow);
    }

    [Fact]
    public async Task Login_ThrowsValidationException_WhenUserDoesNotExist()
    {
        // Arrange
        var loginDto = new LoginDto { Email = "nonexistent@example.com", Password = "Password123!" };

        _mockUserRepository.Setup(r => r.ExistsByEmail(loginDto.Email, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() => _userService.Login(loginDto, CancellationToken.None));
    }

    [Fact]
    public async Task Login_ThrowsValidationException_WhenPasswordIsIncorrect()
    {
        // Arrange
        var loginDto = new LoginDto { Email = "test@example.com", Password = "WrongPassword!" };
        var user = new User { Id = Guid.NewGuid(), Email = "test@example.com" };

        _mockUserRepository.Setup(r => r.ExistsByEmail(loginDto.Email, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _mockUserRepository.Setup(r => r.FindByEmailAsync(loginDto.Email, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _mockUserRepository.Setup(r => r.CheckPasswordAsync(user.Id, loginDto.Password, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() => _userService.Login(loginDto, CancellationToken.None));
    }

    [Fact]
    public async Task Login_ThrowsException_WhenRepositoryFails()
    {
        // Arrange
        var loginDto = new LoginDto { Email = "test@example.com", Password = "Password123!" };
        var expectedException = new InvalidOperationException("Database error during find by email");

        _mockUserRepository.Setup(r => r.ExistsByEmail(loginDto.Email, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _mockUserRepository.Setup(r => r.FindByEmailAsync(loginDto.Email, It.IsAny<CancellationToken>())).ThrowsAsync(expectedException);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _userService.Login(loginDto, CancellationToken.None));
    }
}