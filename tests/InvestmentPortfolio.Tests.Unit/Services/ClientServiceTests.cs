using FluentAssertions;
using InvestmentPortfolio.Core.Entities;
using InvestmentPortfolio.Core.Interfaces.Repositories;
using InvestmentPortfolio.Core.Services;
using InvestmentPortfolio.Shared.DTOs.Client;
using InvestmentPortfolio.Shared.Enums;
using Microsoft.Extensions.Logging;
using Moq;

namespace InvestmentPortfolio.Tests.Unit.Services;

public class ClientServiceTests
{
    private readonly Mock<IClientRepository> _clientRepoMock = new();
    private readonly Mock<IPortfolioRepository> _portfolioRepoMock = new();
    private readonly Mock<ILogger<ClientService>> _loggerMock = new();
    private readonly ClientService _sut;

    public ClientServiceTests()
    {
        _sut = new ClientService(_clientRepoMock.Object, _portfolioRepoMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task CreateClientAsync_NewEmail_CreatesClientSuccessfully()
    {
        // Arrange
        var userId = "user-123";
        var request = new CreateClientRequest
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            Phone = "555-1234",
            RiskProfile = RiskLevel.Moderate,
            DateOfBirth = new DateTime(1985, 6, 15)
        };

        _clientRepoMock.Setup(r => r.EmailExistsAsync(request.Email, null)).ReturnsAsync(false);
        _clientRepoMock.Setup(r => r.AddAsync(It.IsAny<Client>())).Returns(Task.CompletedTask);
        _clientRepoMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        var result = await _sut.CreateClientAsync(request, userId);

        // Assert
        result.Success.Should().BeTrue();
        result.Data!.FirstName.Should().Be("John");
        result.Data.Email.Should().Be("john.doe@example.com");
        _clientRepoMock.Verify(r => r.AddAsync(It.Is<Client>(c => c.UserId == userId)), Times.Once);
    }

    [Fact]
    public async Task CreateClientAsync_DuplicateEmail_ReturnsFailure()
    {
        // Arrange
        var request = new CreateClientRequest { Email = "existing@example.com" };
        _clientRepoMock.Setup(r => r.EmailExistsAsync(request.Email, null)).ReturnsAsync(true);

        // Act
        var result = await _sut.CreateClientAsync(request, "user-1");

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("email already exists");
        _clientRepoMock.Verify(r => r.AddAsync(It.IsAny<Client>()), Times.Never);
    }

    [Fact]
    public async Task UpdateClientAsync_ExistingClient_UpdatesFieldsCorrectly()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        var client = new Client
        {
            Id = clientId,
            FirstName = "Old",
            LastName = "Name",
            RiskProfile = RiskLevel.Conservative
        };
        var request = new UpdateClientRequest
        {
            FirstName = "New",
            RiskProfile = RiskLevel.Aggressive
        };

        _clientRepoMock.Setup(r => r.GetByIdAsync(clientId)).ReturnsAsync(client);
        _clientRepoMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        var result = await _sut.UpdateClientAsync(clientId, request);

        // Assert
        result.Success.Should().BeTrue();
        client.FirstName.Should().Be("New");
        client.RiskProfile.Should().Be(RiskLevel.Aggressive);
        client.LastName.Should().Be("Name"); // unchanged
    }

    [Fact]
    public async Task DeleteClientAsync_ExistingClient_SoftDeletes()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        var client = new Client { Id = clientId, IsDeleted = false };

        _clientRepoMock.Setup(r => r.GetByIdAsync(clientId)).ReturnsAsync(client);
        _clientRepoMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        var result = await _sut.DeleteClientAsync(clientId);

        // Assert
        result.Success.Should().BeTrue();
        client.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public async Task GetAllClientsAsync_ReturnsClientsForUser()
    {
        // Arrange
        var userId = "user-abc";
        var clients = new List<Client>
        {
            new() { FirstName = "Alice", LastName = "Smith", Email = "alice@test.com", UserId = userId },
            new() { FirstName = "Bob", LastName = "Jones", Email = "bob@test.com", UserId = userId }
        };

        _clientRepoMock.Setup(r => r.GetClientsByUserAsync(userId)).ReturnsAsync(clients);

        // Act
        var result = await _sut.GetAllClientsAsync(userId);

        // Assert
        result.Success.Should().BeTrue();
        result.Data.Should().HaveCount(2);
    }
}
