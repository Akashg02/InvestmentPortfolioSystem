using FluentAssertions;
using InvestmentPortfolio.Core.Entities;
using InvestmentPortfolio.Core.Interfaces.Repositories;
using InvestmentPortfolio.Core.Interfaces.Services;
using InvestmentPortfolio.Core.Services;
using InvestmentPortfolio.Shared.DTOs.Portfolio;
using InvestmentPortfolio.Shared.Enums;
using Microsoft.Extensions.Logging;
using Moq;

namespace InvestmentPortfolio.Tests.Unit.Services;

public class PortfolioServiceTests
{
    private readonly Mock<IPortfolioRepository> _portfolioRepoMock = new();
    private readonly Mock<IMarketDataService> _marketDataMock = new();
    private readonly Mock<ILogger<PortfolioService>> _loggerMock = new();
    private readonly PortfolioService _sut;

    public PortfolioServiceTests()
    {
        _sut = new PortfolioService(_portfolioRepoMock.Object, _marketDataMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task GetPortfolioByIdAsync_ExistingPortfolio_ReturnsSuccessWithData()
    {
        // Arrange
        var portfolioId = Guid.NewGuid();
        var portfolio = new Portfolio
        {
            Id = portfolioId,
            Name = "Test Portfolio",
            Status = PortfolioStatus.Active,
            ClientId = Guid.NewGuid(),
            Investments = new List<Investment>()
        };

        _portfolioRepoMock.Setup(r => r.GetPortfolioWithInvestmentsAsync(portfolioId))
            .ReturnsAsync(portfolio);

        // Act
        var result = await _sut.GetPortfolioByIdAsync(portfolioId);

        // Assert
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Name.Should().Be("Test Portfolio");
    }

    [Fact]
    public async Task GetPortfolioByIdAsync_NonExistingPortfolio_ReturnsFailure()
    {
        // Arrange
        _portfolioRepoMock.Setup(r => r.GetPortfolioWithInvestmentsAsync(It.IsAny<Guid>()))
            .ReturnsAsync((Portfolio?)null);

        // Act
        var result = await _sut.GetPortfolioByIdAsync(Guid.NewGuid());

        // Assert
        result.Success.Should().BeFalse();
        result.Data.Should().BeNull();
    }

    [Fact]
    public async Task CreatePortfolioAsync_ValidRequest_CreatesAndReturnsPortfolio()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        var request = new CreatePortfolioRequest
        {
            Name = "Growth Portfolio",
            Description = "Aggressive growth strategy",
            ClientId = clientId
        };

        _portfolioRepoMock.Setup(r => r.AddAsync(It.IsAny<Portfolio>())).Returns(Task.CompletedTask);
        _portfolioRepoMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        var result = await _sut.CreatePortfolioAsync(request);

        // Assert
        result.Success.Should().BeTrue();
        result.Data!.Name.Should().Be("Growth Portfolio");
        result.Data.ClientId.Should().Be(clientId);
        _portfolioRepoMock.Verify(r => r.AddAsync(It.IsAny<Portfolio>()), Times.Once);
        _portfolioRepoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task CreatePortfolioAsync_CalculatesTotalValueCorrectly()
    {
        // Arrange
        var portfolioId = Guid.NewGuid();
        var portfolio = new Portfolio
        {
            Id = portfolioId,
            Name = "Test",
            Investments = new List<Investment>
            {
                new() { Symbol = "AAPL", Quantity = 10, CurrentPrice = 150, AverageCostBasis = 100 },
                new() { Symbol = "MSFT", Quantity = 5, CurrentPrice = 300, AverageCostBasis = 200 }
            }
        };

        _portfolioRepoMock.Setup(r => r.GetPortfolioWithInvestmentsAsync(portfolioId))
            .ReturnsAsync(portfolio);

        // Act
        var result = await _sut.GetPortfolioByIdAsync(portfolioId);

        // Assert
        result.Data!.TotalValue.Should().Be(1500 + 1500);  // 10*150 + 5*300
        result.Data.TotalCost.Should().Be(1000 + 1000);    // 10*100 + 5*200
        result.Data.TotalGainLoss.Should().Be(1000);        // (1500-1000) + (1500-1000)
    }

    [Fact]
    public async Task DeletePortfolioAsync_ExistingPortfolio_SoftDeletesAndReturnsSuccess()
    {
        // Arrange
        var portfolioId = Guid.NewGuid();
        var portfolio = new Portfolio { Id = portfolioId, Name = "Test" };

        _portfolioRepoMock.Setup(r => r.GetByIdAsync(portfolioId)).ReturnsAsync(portfolio);
        _portfolioRepoMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        var result = await _sut.DeletePortfolioAsync(portfolioId);

        // Assert
        result.Success.Should().BeTrue();
        portfolio.IsDeleted.Should().BeTrue();
    }
}
