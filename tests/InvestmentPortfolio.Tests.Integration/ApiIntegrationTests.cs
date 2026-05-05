using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using InvestmentPortfolio.Shared.DTOs.Auth;
using InvestmentPortfolio.Shared.DTOs.Client;
using InvestmentPortfolio.Shared.DTOs.Common;
using InvestmentPortfolio.Shared.Enums;
using Microsoft.AspNetCore.Mvc.Testing;

namespace InvestmentPortfolio.Tests.Integration;

public class ApiIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public ApiIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task HealthCheck_ReturnsHealthy()
    {
        var response = await _client.GetAsync("/health");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Register_ValidRequest_Returns201()
    {
        var request = new RegisterRequest
        {
            Email = $"test_{Guid.NewGuid():N}@example.com",
            Password = "Test@123456!",
            FirstName = "Integration",
            LastName = "Test"
        };

        var response = await _client.PostAsJsonAsync("/api/auth/register", request);
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var body = await response.Content.ReadFromJsonAsync<ApiResponse<AuthResponse>>();
        body!.Success.Should().BeTrue();
        body.Data!.Token.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Login_InvalidCredentials_Returns401()
    {
        var request = new LoginRequest
        {
            Email = "nobody@example.com",
            Password = "WrongPassword123!"
        };

        var response = await _client.PostAsJsonAsync("/api/auth/login", request);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Clients_RequiresAuthentication_Returns401()
    {
        var response = await _client.GetAsync("/api/clients");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Register_ThenLogin_ThenCreateClient_FlowSucceeds()
    {
        // Register
        var email = $"flow_{Guid.NewGuid():N}@test.com";
        var registerRequest = new RegisterRequest
        {
            Email = email,
            Password = "Flow@123456!",
            FirstName = "Flow",
            LastName = "User"
        };

        var registerResponse = await _client.PostAsJsonAsync("/api/auth/register", registerRequest);
        var registerBody = await registerResponse.Content.ReadFromJsonAsync<ApiResponse<AuthResponse>>();

        // Authenticate subsequent requests
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", registerBody!.Data!.Token);

        // Create client
        var clientRequest = new CreateClientRequest
        {
            FirstName = "Jane",
            LastName = "Smith",
            Email = $"jane_{Guid.NewGuid():N}@example.com",
            Phone = "555-9876",
            RiskProfile = RiskLevel.Moderate,
            DateOfBirth = new DateTime(1990, 3, 20)
        };

        var clientResponse = await _client.PostAsJsonAsync("/api/clients", clientRequest);
        clientResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var clientBody = await clientResponse.Content.ReadFromJsonAsync<ApiResponse<ClientDto>>();
        clientBody!.Success.Should().BeTrue();
        clientBody.Data!.FirstName.Should().Be("Jane");
    }
}
