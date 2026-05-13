using FluentAssertions;
using Moq;
using Moveo_backend.Rental.Domain.Model.Commands;
using Moveo_backend.Rental.Domain.Services;
using Reqnroll;
using RentalAggregate = Moveo_backend.Rental.Domain.Model.Aggregates.Rental;

namespace Moveo_backend.Tests.BDD.Rental;

[Binding]
public class RentalRequestStepDefinitions
{
    private readonly Mock<IRentalService> _rentalServiceMock = new();
    private RentalAggregate _createdRental = null!;
    private RentalAggregate? _queriedRental;
    private bool _deleteResult;

    [Given(@"the rental service accepts new rental requests")]
    public void GivenTheRentalServiceAcceptsNewRentalRequests()
    {
        _rentalServiceMock
            .Setup(s => s.CreateAsync(It.IsAny<CreateRentalCommand>()))
            .ReturnsAsync((CreateRentalCommand cmd) =>
                new RentalAggregate(cmd.VehicleId, cmd.RenterId, cmd.OwnerId,
                    cmd.StartDate, cmd.EndDate, cmd.TotalPrice,
                    cmd.PickupLocation, cmd.ReturnLocation, cmd.Notes, cmd.AdventureRouteId));
    }

    [Given(@"the rental service has no rental with id (.*)")]
    public void GivenTheRentalServiceHasNoRentalWithId(int id)
    {
        _rentalServiceMock
            .Setup(s => s.GetByIdAsync(id))
            .ReturnsAsync((RentalAggregate?)null);
    }

    [Given(@"a rental with id (.*) exists in the service")]
    public void GivenARentalWithIdExistsInTheService(int id)
    {
        _rentalServiceMock
            .Setup(s => s.DeleteAsync(id))
            .ReturnsAsync(true);
    }

    [When(@"I create a rental for vehicle (.*) by renter (.*) from owner (.*) with total price (.*)")]
    public async Task WhenICreateARental(int vehicleId, int renterId, int ownerId, decimal price)
    {
        var command = new CreateRentalCommand(
            vehicleId, renterId, ownerId,
            DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddDays(5),
            price, null, null, null, null);

        _createdRental = await _rentalServiceMock.Object.CreateAsync(command);
    }

    [When(@"I query the rental with id (.*)")]
    public async Task WhenIQueryTheRentalWithId(int id)
    {
        _queriedRental = await _rentalServiceMock.Object.GetByIdAsync(id);
    }

    [When(@"I delete the rental with id (.*)")]
    public async Task WhenIDeleteTheRentalWithId(int id)
    {
        _deleteResult = await _rentalServiceMock.Object.DeleteAsync(id);
    }

    [Then(@"the created rental should have status ""(.*)""")]
    public void ThenTheCreatedRentalShouldHaveStatus(string status)
    {
        _createdRental.Should().NotBeNull();
        _createdRental.Status.Should().Be(status);
    }

    [Then(@"the created rental should reference vehicle (.*)")]
    public void ThenTheCreatedRentalShouldReferenceVehicle(int vehicleId)
    {
        _createdRental.VehicleId.Should().Be(vehicleId);
    }

    [Then(@"the rental query result should be null")]
    public void ThenTheRentalQueryResultShouldBeNull()
    {
        _queriedRental.Should().BeNull();
    }

    [Then(@"the delete result should be true")]
    public void ThenTheDeleteResultShouldBeTrue()
    {
        _deleteResult.Should().BeTrue();
    }
}
