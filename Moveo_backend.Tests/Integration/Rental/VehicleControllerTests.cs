using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Xunit;

namespace Moveo_backend.Tests.Integration.Rental;

public class VehicleControllerTests : IClassFixture<MoveoWebApplicationFactory>
{
    private readonly HttpClient _client;

    public VehicleControllerTests(MoveoWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    // HU07 - TS02 - Listar vehículos (base vacía)
    [Fact]
    public async Task GetAll_Returns200WithEmptyList()
    {
        var response = await _client.GetAsync("/api/v1/vehicles");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Be("[]");
    }

    // TS02 - Obtener vehículo inexistente
    [Fact]
    public async Task GetById_WhenNotFound_Returns404()
    {
        var response = await _client.GetAsync("/api/v1/vehicles/9999");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // TS02 - Eliminar vehículo inexistente
    [Fact]
    public async Task Delete_WhenNotFound_Returns404()
    {
        var response = await _client.DeleteAsync("/api/v1/vehicles/9999");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
