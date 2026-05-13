using FluentAssertions;
using Moq;
using Moveo_backend.UserManagement.Application.CommandServices;
using Moveo_backend.UserManagement.Domain.Model.Aggregates;
using Moveo_backend.UserManagement.Domain.Model.Commands;
using Moveo_backend.UserManagement.Domain.Repositories;
using Xunit;

namespace Moveo_backend.Tests.UserManagement.Application.Internal.CommandServices;

public class UserCommandServiceTests
{
    private readonly Mock<IUserRepository> _repositoryMock;
    private readonly UserCommandService _service;

    public UserCommandServiceTests()
    {
        _repositoryMock = new Mock<IUserRepository>();
        _repositoryMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);
        _service = new UserCommandService(_repositoryMock.Object);
    }

    private static CreateUserCommand DefaultCreateCommand(string email = "juan@example.com") =>
        new CreateUserCommand("Juan", "García", email, "hashed", "999000111", "12345678", "LIC-001", "tenant");

    // HU05 / HU06 - TS01: Registro
    [Fact]
    public async Task HandleCreate_WithNewEmail_CreatesUserSuccessfully()
    {
        _repositoryMock.Setup(r => r.ExistsByEmailAsync("juan@example.com")).ReturnsAsync(false);
        _repositoryMock.Setup(r => r.AddAsync(It.IsAny<User>())).Returns(Task.CompletedTask);

        var result = await _service.HandleCreate(DefaultCreateCommand());

        result.Should().NotBeNull();
        result.Email.Should().Be("juan@example.com");
        result.Role.Should().Be("tenant");
        _repositoryMock.Verify(r => r.AddAsync(It.IsAny<User>()), Times.Once);
        _repositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task HandleCreate_WithDuplicateEmail_ThrowsInvalidOperationException()
    {
        _repositoryMock.Setup(r => r.ExistsByEmailAsync("juan@example.com")).ReturnsAsync(true);

        var act = async () => await _service.HandleCreate(DefaultCreateCommand());

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*already exists*");
    }

    // HU05 - Propietario registrado con rol correcto
    [Fact]
    public async Task HandleCreate_AsOwner_SetsOwnerRole()
    {
        var cmd = new CreateUserCommand("Ana", "López", "ana@example.com", "hashed",
            "988000222", "87654321", "LIC-002", "owner");

        _repositoryMock.Setup(r => r.ExistsByEmailAsync("ana@example.com")).ReturnsAsync(false);
        _repositoryMock.Setup(r => r.AddAsync(It.IsAny<User>())).Returns(Task.CompletedTask);

        var result = await _service.HandleCreate(cmd);

        result.Role.Should().Be("owner");
    }

    // HU08 - Actualizar datos de usuario
    [Fact]
    public async Task HandleUpdate_WithExistingUser_UpdatesAndReturnsUser()
    {
        var existingUser = new User(DefaultCreateCommand());
        _repositoryMock.Setup(r => r.FindByIdAsync(existingUser.Id)).ReturnsAsync(existingUser);

        var updateCmd = new UpdateUserCommand(
            Id: existingUser.Id,
            FirstName: "Carlos",
            LastName: "López",
            Phone: "977111222",
            Avatar: null,
            Preferences: null,
            BankAccount: null);

        var result = await _service.HandleUpdate(updateCmd);

        result.FirstName.Should().Be("Carlos");
        result.Phone.Should().Be("977111222");
        _repositoryMock.Verify(r => r.Update(existingUser), Times.Once);
    }

    [Fact]
    public async Task HandleUpdate_WithNonExistentUser_ThrowsKeyNotFoundException()
    {
        _repositoryMock.Setup(r => r.FindByIdAsync(999)).ReturnsAsync((User?)null);

        var updateCmd = new UpdateUserCommand(Id: 999, FirstName: "X",
            LastName: null, Phone: null, Avatar: null, Preferences: null, BankAccount: null);

        var act = async () => await _service.HandleUpdate(updateCmd);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    // HU05/HU06 - Eliminar usuario
    [Fact]
    public async Task HandleDelete_WithExistingUser_RemovesUser()
    {
        var user = new User(DefaultCreateCommand());
        _repositoryMock.Setup(r => r.FindByIdAsync(user.Id)).ReturnsAsync(user);

        await _service.HandleDelete(new DeleteUserCommand(user.Id));

        _repositoryMock.Verify(r => r.Remove(user), Times.Once);
        _repositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task HandleDelete_WithNonExistentUser_ThrowsKeyNotFoundException()
    {
        _repositoryMock.Setup(r => r.FindByIdAsync(999)).ReturnsAsync((User?)null);

        var act = async () => await _service.HandleDelete(new DeleteUserCommand(999));

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}
