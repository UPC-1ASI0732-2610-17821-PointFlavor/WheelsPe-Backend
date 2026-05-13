using FluentAssertions;
using Moveo_backend.Rental.Domain.Model.ValueObjects;
using Xunit;
using Vehicle = Moveo_backend.Rental.Domain.Model.Aggregates.Vehicle;

namespace Moveo_backend.Tests.Rental.Domain.Model.Aggregates;

public class VehicleTests
{
    private static Money DefaultPrice() => new Money(150m, "USD");
    private static Money DefaultDeposit() => new Money(300m, "USD");
    private static Location DefaultLocation() => new Location("Miraflores", "Av. Principal 123", -12.12, -77.03);

    private Vehicle CreateVehicle(
        int ownerId = 1,
        string brand = "Toyota",
        string model = "Corolla",
        int year = 2022) =>
        new Vehicle(ownerId, brand, model, year, "Blanco", "Automático", "Gasolina", 5,
            "ABC-123", DefaultPrice(), DefaultDeposit(), DefaultLocation());

    // HU07 - Publicar vehículo
    [Fact]
    public void Constructor_WithValidData_SetsAllPropertiesCorrectly()
    {
        var vehicle = CreateVehicle(ownerId: 10, brand: "Toyota", model: "Corolla", year: 2022);

        vehicle.OwnerId.Should().Be(10);
        vehicle.Brand.Should().Be("Toyota");
        vehicle.Model.Should().Be("Corolla");
        vehicle.Year.Should().Be(2022);
        vehicle.Status.Should().Be("active");
        vehicle.IsAvailable.Should().BeTrue();
        vehicle.DailyPrice.Amount.Should().Be(150m);
    }

    [Fact]
    public void Constructor_WithFeaturesList_SerializesCorrectly()
    {
        var features = new List<string> { "GPS", "Bluetooth", "Cámara trasera" };

        var vehicle = new Vehicle(1, "Toyota", "Corolla", 2022, "Blanco", "Automático", "Gasolina", 5,
            "ABC-123", DefaultPrice(), DefaultDeposit(), DefaultLocation(),
            features: features);

        vehicle.Features.Should().BeEquivalentTo(features);
    }

    // HU07 - Escenario 2: estado activo por defecto
    [Fact]
    public void Status_OnCreation_IsActive()
    {
        var vehicle = CreateVehicle();
        vehicle.Status.Should().Be("active");
    }

    // HU08 - Editar información del auto
    [Fact]
    public void PartialUpdate_WithNewPrice_UpdatesOnlyDailyPrice()
    {
        var vehicle = CreateVehicle();
        var originalModel = vehicle.Model;

        vehicle.PartialUpdate(dailyPrice: 200m);

        vehicle.DailyPrice.Amount.Should().Be(200m);
        vehicle.Model.Should().Be(originalModel);
    }

    [Fact]
    public void PartialUpdate_WithNullValues_DoesNotChangeProperties()
    {
        var vehicle = CreateVehicle();
        var originalStatus = vehicle.Status;
        var originalDescription = vehicle.Description;

        vehicle.PartialUpdate();

        vehicle.Status.Should().Be(originalStatus);
        vehicle.Description.Should().Be(originalDescription);
    }

    [Fact]
    public void ChangeStatus_UpdatesStatus()
    {
        var vehicle = CreateVehicle();

        vehicle.ChangeStatus("inactive");

        vehicle.Status.Should().Be("inactive");
    }

    [Fact]
    public void UpdateDetails_ReplacesAllMutableFields()
    {
        var vehicle = CreateVehicle();
        var newPrice = new Money(200m, "USD");
        var newLocation = new Location("Barranco", "Av. Grau 456", -12.15, -77.02);

        vehicle.UpdateDetails(1, "Honda", "Civic", 2023, "Negro", "Manual", "Gasolina",
            4, "XYZ-999", newPrice, DefaultDeposit(), newLocation, "active", "Auto sport",
            null, null, null);

        vehicle.Brand.Should().Be("Honda");
        vehicle.Model.Should().Be("Civic");
        vehicle.DailyPrice.Amount.Should().Be(200m);
        vehicle.Location.District.Should().Be("Barranco");
    }
}
