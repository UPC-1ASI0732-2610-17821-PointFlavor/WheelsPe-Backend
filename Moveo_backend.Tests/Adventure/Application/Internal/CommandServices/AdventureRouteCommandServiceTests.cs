using FluentAssertions;
using Moq;
using Moveo_backend.Adventure.Application.Internal.CommandServices;
using Moveo_backend.Adventure.Domain.Model.Aggregate;
using Moveo_backend.Adventure.Domain.Model.Commands;
using Moveo_backend.Adventure.Domain.Repositories;
using Moveo_backend.Shared.Domain.Repositories;
using Xunit;

namespace Moveo_backend.Tests.Adventure.Application.Internal.CommandServices;

public class AdventureRouteCommandServiceTests
{
    private readonly Mock<IAdventureRouteRepository> _repositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly AdventureRouteCommandService _service;

    public AdventureRouteCommandServiceTests()
    {
        _repositoryMock = new Mock<IAdventureRouteRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _unitOfWorkMock.Setup(u => u.CompleteAsync()).Returns(Task.CompletedTask);
        _service = new AdventureRouteCommandService(_repositoryMock.Object, _unitOfWorkMock.Object);
    }

    private static CreateAdventureRouteCommand MakeCreateCommand(string name = "Ruta Miraflores") =>
        new(OwnerId: 1, Name: name, Title: "Circuito costero", Description: "Desc",
            StartLocation: "Lima", EndLocation: "Chorrillos", Type: "city",
            Duration: 3, Difficulty: "easy", EstimatedCost: 80m);

    // HU07 / HU10 - Crear ruta de aventura
    [Fact]
    public async Task Handle_CreateAdventureRouteCommand_WhenNameIsNew_ReturnsRoute()
    {
        _repositoryMock.Setup(r => r.ExistsByNameAsync("Ruta Miraflores")).ReturnsAsync(false);
        _repositoryMock.Setup(r => r.AddAsync(It.IsAny<AdventureRoute>())).Returns(Task.CompletedTask);

        var result = await _service.Handle(MakeCreateCommand());

        result.Should().NotBeNull();
        result!.Name.Should().Be("Ruta Miraflores");
        result.Difficulty.Should().Be("easy");
        _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_CreateAdventureRouteCommand_WhenNameExists_ThrowsException()
    {
        _repositoryMock.Setup(r => r.ExistsByNameAsync("Ruta Miraflores")).ReturnsAsync(true);

        var act = async () => await _service.Handle(MakeCreateCommand());

        await act.Should().ThrowAsync<Exception>()
            .WithMessage("*already exists*");
    }

    [Fact]
    public async Task Handle_UpdateAdventureRouteCommand_WhenNotFound_ReturnsNull()
    {
        _repositoryMock.Setup(r => r.FindByIdAsync(999)).ReturnsAsync((AdventureRoute?)null);

        var command = new UpdateAdventureRouteCommand(
            Id: 999, Name: "X", Title: "X", Description: "X",
            StartLocation: "X", EndLocation: "X", Type: "city",
            Duration: 1, Difficulty: "easy", EstimatedCost: 0m);

        var result = await _service.Handle(command);

        result.Should().BeNull();
        _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Never);
    }

    [Fact]
    public async Task Handle_DeleteAdventureRouteCommand_WhenExists_ReturnsTrue()
    {
        var route = new AdventureRoute(MakeCreateCommand());
        _repositoryMock.Setup(r => r.FindByIdAsync(route.Id)).ReturnsAsync(route);

        var result = await _service.Handle(new DeleteAdventureRouteCommand(route.Id));

        result.Should().BeTrue();
        _repositoryMock.Verify(r => r.Remove(route), Times.Once);
        _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_DeleteAdventureRouteCommand_WhenNotFound_ReturnsFalse()
    {
        _repositoryMock.Setup(r => r.FindByIdAsync(999)).ReturnsAsync((AdventureRoute?)null);

        var result = await _service.Handle(new DeleteAdventureRouteCommand(999));

        result.Should().BeFalse();
        _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Never);
    }
}
