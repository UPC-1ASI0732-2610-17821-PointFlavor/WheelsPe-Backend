using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Moveo_backend.IAM.Application.Internal;
using Moveo_backend.IAM.Domain.Model.Commands;
using Moveo_backend.IAM.Infrastructure.Hashing;
using Moveo_backend.Shared.Infrastructure.Persistence.EFC.Configuration;
using Moveo_backend.UserManagement.Domain.Model.Aggregates;
using Moveo_backend.UserManagement.Domain.Model.Commands;
using Xunit;

namespace Moveo_backend.Tests.IAM.Application.Internal;

public class AuthServiceTests
{
    private readonly Mock<IHashingService> _hashingServiceMock;
    private readonly AppDbContext _dbContext;
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _dbContext = new AppDbContext(options);
        _hashingServiceMock = new Mock<IHashingService>();
        _authService = new AuthService(_dbContext, _hashingServiceMock.Object);
    }

    // HU05 / HU06 - TS01
    [Fact]
    public async Task RegisterAsync_WithNewEmail_ReturnsAuthenticatedUser()
    {
        _hashingServiceMock.Setup(h => h.HashPassword("Pass123!")).Returns("hashed");

        var command = new RegisterCommand("Juan", "García", "juan@example.com", "Pass123!", null, null, null, "tenant");

        var result = await _authService.RegisterAsync(command);

        result.Should().NotBeNull();
        result!.Email.Should().Be("juan@example.com");
        result.Role.Should().Be("tenant");
    }

    [Fact]
    public async Task RegisterAsync_WithDuplicateEmail_ReturnsNull()
    {
        _hashingServiceMock.Setup(h => h.HashPassword(It.IsAny<string>())).Returns("hashed");
        await _dbContext.Users.AddAsync(new User(new CreateUserCommand(
            "Ana", "López", "ana@example.com", "hashed", "", "", "", "tenant")));
        await _dbContext.SaveChangesAsync();

        var command = new RegisterCommand("Pedro", "García", "ana@example.com", "Pass123!", null, null, null, "owner");

        var result = await _authService.RegisterAsync(command);

        result.Should().BeNull();
    }

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ReturnsAuthenticatedUser()
    {
        _hashingServiceMock.Setup(h => h.HashPassword("Pass123!")).Returns("hashed");
        _hashingServiceMock.Setup(h => h.VerifyPassword("Pass123!", "hashed")).Returns(true);

        await _dbContext.Users.AddAsync(new User(new CreateUserCommand(
            "Juan", "García", "juan@example.com", "hashed", "", "", "", "tenant")));
        await _dbContext.SaveChangesAsync();

        var result = await _authService.LoginAsync(new LoginCommand("juan@example.com", "Pass123!"));

        result.Should().NotBeNull();
        result!.Email.Should().Be("juan@example.com");
    }

    [Fact]
    public async Task LoginAsync_WithNonExistentEmail_ReturnsNull()
    {
        var result = await _authService.LoginAsync(new LoginCommand("ghost@example.com", "Pass123!"));

        result.Should().BeNull();
    }

    [Fact]
    public async Task LoginAsync_WithWrongPassword_ReturnsNull()
    {
        _hashingServiceMock.Setup(h => h.VerifyPassword("Wrong!", "hashed")).Returns(false);
        await _dbContext.Users.AddAsync(new User(new CreateUserCommand(
            "Juan", "García", "juan@example.com", "hashed", "", "", "", "tenant")));
        await _dbContext.SaveChangesAsync();

        var result = await _authService.LoginAsync(new LoginCommand("juan@example.com", "Wrong!"));

        result.Should().BeNull();
    }

    [Fact]
    public async Task ChangePasswordAsync_WithCorrectCurrentPassword_ReturnsTrue()
    {
        _hashingServiceMock.Setup(h => h.VerifyPassword("OldPass!", "hashed_old")).Returns(true);
        _hashingServiceMock.Setup(h => h.HashPassword("NewPass!")).Returns("hashed_new");

        var user = new User(new CreateUserCommand("Juan", "G", "juan@example.com", "hashed_old", "", "", "", "tenant"));
        await _dbContext.Users.AddAsync(user);
        await _dbContext.SaveChangesAsync();

        var command = new AuthChangePasswordCommand("OldPass!", "NewPass!");
        var result = await _authService.ChangePasswordAsync(user.Id, command);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task ChangePasswordAsync_WithWrongCurrentPassword_ReturnsFalse()
    {
        _hashingServiceMock.Setup(h => h.VerifyPassword("Wrong!", "hashed_old")).Returns(false);

        var user = new User(new CreateUserCommand("Juan", "G", "juan@example.com", "hashed_old", "", "", "", "tenant"));
        await _dbContext.Users.AddAsync(user);
        await _dbContext.SaveChangesAsync();

        var command = new AuthChangePasswordCommand("Wrong!", "NewPass!");
        var result = await _authService.ChangePasswordAsync(user.Id, command);

        result.Should().BeFalse();
    }
}
