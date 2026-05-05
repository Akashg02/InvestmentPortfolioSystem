using FluentAssertions;
using InvestmentPortfolio.Core.Entities;
using InvestmentPortfolio.Core.Interfaces.Repositories;
using InvestmentPortfolio.Core.Interfaces.Services;
using InvestmentPortfolio.Core.Services;
using InvestmentPortfolio.Shared.DTOs.Investment;
using InvestmentPortfolio.Shared.DTOs.MarketData;
using InvestmentPortfolio.Shared.Enums;
using Microsoft.Extensions.Logging;
using Moq;

namespace InvestmentPortfolio.Tests.Unit.Services;

public class InvestmentServiceTests
{
    private readonly Mock<IInvestmentRepository> _investmentRepoMock = new();
    private readonly Mock<IPortfolioRepository> _portfolioRepoMock = new();
    private readonly Mock<IMarketDataService> _marketDataMock = new();
    private readonly Mock<ILogger<InvestmentService>> _loggerMock = new();
    private readonly InvestmentService _sut;

    public InvestmentServiceTests()
    {
        _sut = new InvestmentService(
            _investmentRepoMock.Object,
            _portfolioRepoMock.Object,
            _marketDataMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task AddInvestmentAsync_NewSymbol_CreatesInvestmentWithMarketPrice()
    {
        // Arrange
        var portfolioId = Guid.NewGuid();
        var portfolio = new Portfolio { Id = portfolioId, Status = PortfolioStatus.Active };
        var request = new CreateInvestmentRequest
        {
            PortfolioId = portfolioId,
            Symbol = "AAPL",
            AssetType = AssetType.Stock,
            Quantity = 10,
            PurchasePrice = 145,
            PurchaseDate = DateTime.Today
        };

        _portfolioRepoMock.Setup(r => r.GetByIdAsync(portfolioId)).ReturnsAsync(portfolio);
        _investmentRepoMock.Setup(r => r.GetBySymbolAndPortfolioAsync("AAPL", portfolioId))
            .ReturnsAsync((Investment?)null);
        _marketDataMock.Setup(m => m.GetQuoteAsync("AAPL"))
            .ReturnsAsync(new StockQuoteDto { Symbol = "AAPL", CompanyName = "Apple Inc.", Price = 150, PreviousClose = 148 });
        _investmentRepoMock.Setup(r => r.AddAsync(It.IsAny<Investment>())).Returns(Task.CompletedTask);
        _investmentRepoMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        var result = await _sut.AddInvestmentAsync(request);

        // Assert
        result.Success.Should().BeTrue();
        result.Data!.Symbol.Should().Be("AAPL");
        result.Data.CurrentPrice.Should().Be(150);
        result.Data.AverageCostBasis.Should().Be(145);
    }

    [Fact]
    public async Task AddInvestmentAsync_ExistingSymbol_UpdatesAverageCostBasis()
    {
        // Arrange
        var portfolioId = Guid.NewGuid();
        var existing = new Investment
        {
            Symbol = "AAPL",
            Quantity = 10,
            AverageCostBasis = 140,
            CurrentPrice = 150,
            PreviousClose = 148,
            PortfolioId = portfolioId,
            AssetType = AssetType.Stock
        };
        var request = new CreateInvestmentRequest
        {
            PortfolioId = portfolioId,
            Symbol = "AAPL",
            Quantity = 10,
            PurchasePrice = 160,
            PurchaseDate = DateTime.Today
        };

        _portfolioRepoMock.Setup(r => r.GetByIdAsync(portfolioId))
            .ReturnsAsync(new Portfolio { Id = portfolioId });
        _investmentRepoMock.Setup(r => r.GetBySymbolAndPortfolioAsync("AAPL", portfolioId))
            .ReturnsAsync(existing);
        _marketDataMock.Setup(m => m.GetQuoteAsync("AAPL"))
            .ReturnsAsync(new StockQuoteDto { Price = 155 });
        _investmentRepoMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        var result = await _sut.AddInvestmentAsync(request);

        // Assert
        result.Success.Should().BeTrue();
        existing.Quantity.Should().Be(20); // 10 + 10
        existing.AverageCostBasis.Should().Be(150); // (10*140 + 10*160) / 20
    }

    [Fact]
    public async Task SellInvestmentAsync_PartialSell_ReducesQuantity()
    {
        // Arrange
        var investmentId = Guid.NewGuid();
        var investment = new Investment { Id = investmentId, Quantity = 20, Symbol = "AAPL" };

        _investmentRepoMock.Setup(r => r.GetByIdAsync(investmentId)).ReturnsAsync(investment);
        _investmentRepoMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        var result = await _sut.SellInvestmentAsync(investmentId, 5, 160);

        // Assert
        result.Success.Should().BeTrue();
        investment.Quantity.Should().Be(15);
    }

    [Fact]
    public async Task SellInvestmentAsync_FullSell_MarksAsDeleted()
    {
        // Arrange
        var investmentId = Guid.NewGuid();
        var investment = new Investment { Id = investmentId, Quantity = 10, Symbol = "TSLA" };

        _investmentRepoMock.Setup(r => r.GetByIdAsync(investmentId)).ReturnsAsync(investment);
        _investmentRepoMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        var result = await _sut.SellInvestmentAsync(investmentId, 10, 200);

        // Assert
        result.Success.Should().BeTrue();
        investment.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public async Task SellInvestmentAsync_MoreThanOwned_ReturnsFailure()
    {
        // Arrange
        var investmentId = Guid.NewGuid();
        _investmentRepoMock.Setup(r => r.GetByIdAsync(investmentId))
            .ReturnsAsync(new Investment { Quantity = 5 });

        // Act
        var result = await _sut.SellInvestmentAsync(investmentId, 10, 100);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Cannot sell");
    }
}
