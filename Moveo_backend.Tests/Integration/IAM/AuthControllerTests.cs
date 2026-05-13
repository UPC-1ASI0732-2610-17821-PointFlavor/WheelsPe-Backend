using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Xunit;

namespace Moveo_backend.Tests.Integration.IAM;

public class AuthControllerTests : IClassFixture<MoveoWebApplicationFactory>
{
    private readonly HttpClient _client;

    public AuthControllerTests(MoveoWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    // HU05 / HU06 - TS01 - Registro exitoso
    [Fact]
    public async Task Register_WithValidData_Returns201()
    {
        var body = new
        {
            firstName = "Ana",
            lastName = "Torres",
            email = $"ana_{Guid.NewGuid():N}@moveo.com",
            password = "Secure123!",
            role = "tenant"
        };

        var response = await _client.PostAsJsonAsync("/api/v1/auth/register", body);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    // TS01 - Registro con email duplicado
    [Fact]
    public async Task Register_WithDuplicateEmail_Returns409()
    {
        var email = $"dup_{Guid.NewGuid():N}@moveo.com";
        var body = new
        {
            firstName = "Pedro",
            lastName = "García",
            email,
            password = "Pass123!",
            role = "owner"
        };

        await _client.PostAsJsonAsync("/api/v1/auth/register", body);
        var response = await _client.PostAsJsonAsync("/api/v1/auth/register", body);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    // TS01 - Login con credenciales inválidas
    [Fact]
    public async Task Login_WithUnknownEmail_Returns401()
    {
        var body = new { email = "ghost@example.com", password = "WrongPass!" };

        var response = await _client.PostAsJsonAsync("/api/v1/auth/login", body);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
