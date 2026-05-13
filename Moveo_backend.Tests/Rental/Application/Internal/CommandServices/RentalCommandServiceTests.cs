using FluentAssertions;
using Moq;
using Moveo_backend.Rental.Application.CommandServices;
using Moveo_backend.Rental.Domain.Model.Commands;
using Moveo_backend.Rental.Domain.Services;
using Xunit;
using RentalAggregate = Moveo_backend.Rental.Domain.Model.Aggregates.Rental;

namespace Moveo_backend.Tests.Rental.Application.Internal.CommandServices;

public class RentalCommandServiceTests
{
    private readonly Mock<IRentalService> _rentalServiceMock;
    private readonly RentalCommandService _service;

    public RentalCommandServiceTests()
    {
        _rentalServiceMock = new Mock<IRentalService>();
        _service = new RentalCommandService(_rentalServiceMock.Object);
    }

    // HU11 - Crear reserva
    [Fact]
    public async Task Handle_CreateRentalCommand_CallsServiceAndReturnsRental()
    {
        var command = new CreateRentalCommand(
            VehicleId: 1, RenterId: 2, OwnerId: 3,
            StartDate: DateTime.UtcNow.AddDays(1),
            EndDate: DateTime.UtcNow.AddDays(5),
            TotalPrice: 600m,
            PickupLocation: "Miraflores",
            ReturnLocation: "Miraflores",
            Notes: null,
            AdventureRouteId: null);

        var expectedRental = new RentalAggregate(1, 2, 3,
            DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddDays(5),
            600m, "Miraflores", "Miraflores", null);

        _rentalServiceMock.Setup(s => s.CreateAsync(command))
            .ReturnsAsync(expectedRental);

        var result = await _service.Handle(command);

        result.Should().NotBeNull();
        result.VehicleId.Should().Be(1);
        result.RenterId.Should().Be(2);
        result.Status.Should().Be("pending");
        _rentalServiceMock.Verify(s => s.CreateAsync(command), Times.Once);
    }

    // HU11 - Conflicto de disponibilidad (Delete / Cancel)
    [Fact]
    public async Task Handle_DeleteRentalCommand_CallsDeleteOnService()
    {
        var command = new DeleteRentalCommand(42);
        _rentalServiceMock.Setup(s => s.DeleteAsync(42)).ReturnsAsync(true);

        var result = await _service.Handle(command);

        result.Should().BeTrue();
        _rentalServiceMock.Verify(s => s.DeleteAsync(42), Times.Once);
    }

    [Fact]
    public async Task Handle_DeleteRentalCommand_WhenNotFound_ReturnsFalse()
    {
        var command = new DeleteRentalCommand(99);
        _rentalServiceMock.Setup(s => s.DeleteAsync(99)).ReturnsAsync(false);

        var result = await _service.Handle(command);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_PatchRentalCommand_ReturnsUpdatedRental()
    {
        var rental = new RentalAggregate(1, 2, 3,
            DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddDays(5),
            600m, null, null, null);

        var patchCommand = new PatchRentalCommand(rental.Id, Status: "accepted",
            VehicleRated: null, VehicleRating: null, AcceptedAt: null, CompletedAt: null);

        _rentalServiceMock.Setup(s => s.PatchAsync(patchCommand))
            .ReturnsAsync(rental);

        var result = await _service.Handle(patchCommand);

        result.Should().NotBeNull();
        _rentalServiceMock.Verify(s => s.PatchAsync(patchCommand), Times.Once);
    }
}
