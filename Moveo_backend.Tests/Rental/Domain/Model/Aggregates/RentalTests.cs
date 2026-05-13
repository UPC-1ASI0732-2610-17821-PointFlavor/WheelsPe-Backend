using FluentAssertions;
using Xunit;
using RentalAggregate = Moveo_backend.Rental.Domain.Model.Aggregates.Rental;

namespace Moveo_backend.Tests.Rental.Domain.Model.Aggregates;

public class RentalTests
{
    private RentalAggregate CreatePendingRental() =>
        new RentalAggregate(
            vehicleId: 1,
            renterId: 2,
            ownerId: 3,
            startDate: DateTime.UtcNow.AddDays(1),
            endDate: DateTime.UtcNow.AddDays(5),
            totalPrice: 600m,
            pickupLocation: "Miraflores",
            returnLocation: "Miraflores",
            notes: null);

    // HU11 - Reservar auto
    [Fact]
    public void Constructor_SetsStatusToPending()
    {
        var rental = CreatePendingRental();
        rental.Status.Should().Be("pending");
        rental.VehicleRated.Should().BeFalse();
    }

    [Fact]
    public void Accept_FromPending_ChangesStatusToAccepted()
    {
        var rental = CreatePendingRental();

        rental.Accept();

        rental.Status.Should().Be("accepted");
        rental.AcceptedAt.Should().NotBeNull();
    }

    [Fact]
    public void Accept_FromNonPending_ThrowsInvalidOperationException()
    {
        var rental = CreatePendingRental();
        rental.Accept();

        var act = () => rental.Accept();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Can only accept pending rentals");
    }

    [Fact]
    public void Activate_FromAccepted_ChangesStatusToActive()
    {
        var rental = CreatePendingRental();
        rental.Accept();

        rental.Activate();

        rental.Status.Should().Be("active");
    }

    [Fact]
    public void Activate_FromPending_ThrowsInvalidOperationException()
    {
        var rental = CreatePendingRental();

        var act = () => rental.Activate();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Can only activate accepted rentals");
    }

    // HU13 / HU14 - Completar alquiler
    [Fact]
    public void Complete_FromActive_ChangesStatusToCompleted()
    {
        var rental = CreatePendingRental();
        rental.Accept();
        rental.Activate();

        rental.Complete();

        rental.Status.Should().Be("completed");
        rental.CompletedAt.Should().NotBeNull();
    }

    [Fact]
    public void Complete_FromNonActive_ThrowsInvalidOperationException()
    {
        var rental = CreatePendingRental();

        var act = () => rental.Complete();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Can only complete active rentals");
    }

    [Fact]
    public void Cancel_FromPending_ChangesStatusToCancelled()
    {
        var rental = CreatePendingRental();

        rental.Cancel();

        rental.Status.Should().Be("cancelled");
    }

    [Fact]
    public void Cancel_FromCompleted_ThrowsInvalidOperationException()
    {
        var rental = CreatePendingRental();
        rental.Accept();
        rental.Activate();
        rental.Complete();

        var act = () => rental.Cancel();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Cannot cancel completed rentals");
    }

    // HU12 - Calificar propietario / vehículo
    [Fact]
    public void RateVehicle_WithValidRating_SetsRatingAndFlag()
    {
        var rental = CreatePendingRental();

        rental.RateVehicle(5);

        rental.VehicleRated.Should().BeTrue();
        rental.VehicleRating.Should().Be(5);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(6)]
    [InlineData(-1)]
    public void RateVehicle_WithInvalidRating_ThrowsArgumentException(int invalidRating)
    {
        var rental = CreatePendingRental();

        var act = () => rental.RateVehicle(invalidRating);

        act.Should().Throw<ArgumentException>()
            .WithMessage("Rating must be between 1 and 5");
    }

    [Fact]
    public void PartialUpdate_WithStatus_UpdatesStatus()
    {
        var rental = CreatePendingRental();

        rental.PartialUpdate(status: "accepted");

        rental.Status.Should().Be("accepted");
    }
}
