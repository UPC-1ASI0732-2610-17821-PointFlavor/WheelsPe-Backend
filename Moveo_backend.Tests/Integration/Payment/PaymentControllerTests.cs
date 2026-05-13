using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Xunit;

namespace Moveo_backend.Tests.Integration.Payment;

public class PaymentControllerTests : IClassFixture<MoveoWebApplicationFactory>
{
    private readonly HttpClient _client;

    public PaymentControllerTests(MoveoWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    // HU11 / HU14 - TS03 - Listar pagos (base vacía)
    [Fact]
    public async Task GetAll_Returns200WithEmptyList()
    {
        var response = await _client.GetAsync("/api/v1/payments");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Be("[]");
    }

    // TS03 - Obtener pago inexistente
    [Fact]
    public async Task GetById_WhenNotFound_Returns404()
    {
        var response = await _client.GetAsync("/api/v1/payments/9999");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // TS03 - Crear pago con datos válidos
    [Fact]
    public async Task CreatePayment_WithValidData_Returns201()
    {
        var body = new
        {
            payerId = 1,
            recipientId = 2,
            rentalId = 10,
            amount = 450.00m,
            currency = "PEN",
            method = "card",
            type = "rental"
        };

        var response = await _client.PostAsJsonAsync("/api/v1/payments", body);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }
}
