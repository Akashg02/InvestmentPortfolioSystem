using InvestmentPortfolio.Core.Entities;
using InvestmentPortfolio.Core.Interfaces.Repositories;
using InvestmentPortfolio.Core.Interfaces.Services;
using InvestmentPortfolio.Shared.DTOs.Common;
using InvestmentPortfolio.Shared.DTOs.Investment;
using InvestmentPortfolio.Shared.DTOs.Portfolio;
using Microsoft.Extensions.Logging;

namespace InvestmentPortfolio.Core.Services;

public class PortfolioService : IPortfolioService
{
    private readonly IPortfolioRepository _portfolioRepository;
    private readonly IMarketDataService _marketDataService;
    private readonly ILogger<PortfolioService> _logger;

    public PortfolioService(
        IPortfolioRepository portfolioRepository,
        IMarketDataService marketDataService,
        ILogger<PortfolioService> logger)
    {
        _portfolioRepository = portfolioRepository;
        _marketDataService = marketDataService;
        _logger = logger;
    }

    public async Task<ApiResponse<IEnumerable<PortfolioDto>>> GetPortfoliosByClientAsync(Guid clientId)
    {
        var portfolios = await _portfolioRepository.GetPortfoliosByClientAsync(clientId);
        var dtos = portfolios.Select(MapToDto);
        return ApiResponse<IEnumerable<PortfolioDto>>.Ok(dtos);
    }

    public async Task<ApiResponse<PortfolioDto>> GetPortfolioByIdAsync(Guid id)
    {
        var portfolio = await _portfolioRepository.GetPortfolioWithInvestmentsAsync(id);
        if (portfolio is null)
            return ApiResponse<PortfolioDto>.Fail($"Portfolio {id} not found.");

        return ApiResponse<PortfolioDto>.Ok(MapToDto(portfolio));
    }

    public async Task<ApiResponse<PortfolioDto>> CreatePortfolioAsync(CreatePortfolioRequest request)
    {
        var portfolio = new Portfolio
        {
            Name = request.Name,
            Description = request.Description,
            ClientId = request.ClientId,
            Status = request.Status
        };

        await _portfolioRepository.AddAsync(portfolio);
        await _portfolioRepository.SaveChangesAsync();

        _logger.LogInformation("Created portfolio {PortfolioId} for client {ClientId}", portfolio.Id, request.ClientId);
        return ApiResponse<PortfolioDto>.Ok(MapToDto(portfolio), "Portfolio created successfully.");
    }

    public async Task<ApiResponse<PortfolioDto>> UpdatePortfolioAsync(Guid id, CreatePortfolioRequest request)
    {
        var portfolio = await _portfolioRepository.GetByIdAsync(id);
        if (portfolio is null)
            return ApiResponse<PortfolioDto>.Fail($"Portfolio {id} not found.");

        portfolio.Name = request.Name;
        portfolio.Description = request.Description;
        portfolio.Status = request.Status;
        portfolio.UpdatedAt = DateTime.UtcNow;

        _portfolioRepository.Update(portfolio);
        await _portfolioRepository.SaveChangesAsync();

        return ApiResponse<PortfolioDto>.Ok(MapToDto(portfolio), "Portfolio updated successfully.");
    }

    public async Task<ApiResponse<bool>> DeletePortfolioAsync(Guid id)
    {
        var portfolio = await _portfolioRepository.GetByIdAsync(id);
        if (portfolio is null)
            return ApiResponse<bool>.Fail($"Portfolio {id} not found.");

        portfolio.IsDeleted = true;
        portfolio.UpdatedAt = DateTime.UtcNow;
        _portfolioRepository.Update(portfolio);
        await _portfolioRepository.SaveChangesAsync();

        return ApiResponse<bool>.Ok(true, "Portfolio deleted successfully.");
    }

    public async Task RefreshPortfolioPricesAsync(Guid portfolioId)
    {
        var portfolio = await _portfolioRepository.GetPortfolioWithInvestmentsAsync(portfolioId);
        if (portfolio is null) return;

        var symbols = portfolio.Investments.Select(i => i.Symbol).Distinct();
        var quotes = await _marketDataService.GetQuotesAsync(symbols);
        var quoteMap = quotes.ToDictionary(q => q.Symbol, q => q);

        foreach (var investment in portfolio.Investments)
        {
            if (quoteMap.TryGetValue(investment.Symbol, out var quote))
            {
                investment.PreviousClose = investment.CurrentPrice;
                investment.CurrentPrice = quote.Price;
                investment.PriceLastUpdated = DateTime.UtcNow;
            }
        }

        portfolio.UpdatedAt = DateTime.UtcNow;
        _portfolioRepository.Update(portfolio);
        await _portfolioRepository.SaveChangesAsync();

        _logger.LogInformation("Refreshed prices for portfolio {PortfolioId}", portfolioId);
    }

    private static PortfolioDto MapToDto(Portfolio portfolio)
    {
        var investments = portfolio.Investments?.ToList() ?? new List<Investment>();
        var totalValue = investments.Sum(i => i.CurrentValue);
        var totalCost = investments.Sum(i => i.TotalCost);
        var totalGainLoss = totalValue - totalCost;

        return new PortfolioDto
        {
            Id = portfolio.Id,
            Name = portfolio.Name,
            Description = portfolio.Description,
            ClientId = portfolio.ClientId,
            ClientName = portfolio.Client?.FullName ?? string.Empty,
            Status = portfolio.Status,
            TotalValue = totalValue,
            TotalCost = totalCost,
            TotalGainLoss = totalGainLoss,
            TotalGainLossPercent = totalCost > 0 ? (totalGainLoss / totalCost) * 100 : 0,
            DayGainLoss = investments.Sum(i => i.DayChange * i.Quantity),
            Investments = investments.Select(MapInvestmentToDto).ToList(),
            CreatedAt = portfolio.CreatedAt,
            UpdatedAt = portfolio.UpdatedAt
        };
    }

    private static InvestmentDto MapInvestmentToDto(Investment i) => new()
    {
        Id = i.Id,
        PortfolioId = i.PortfolioId,
        Symbol = i.Symbol,
        AssetName = i.AssetName,
        AssetType = i.AssetType,
        Quantity = i.Quantity,
        AverageCostBasis = i.AverageCostBasis,
        CurrentPrice = i.CurrentPrice,
        CurrentValue = i.CurrentValue,
        TotalCost = i.TotalCost,
        GainLoss = i.GainLoss,
        GainLossPercent = i.GainLossPercent,
        DayChange = i.DayChange,
        DayChangePercent = i.DayChangePercent,
        LastUpdated = i.PriceLastUpdated,
        PurchaseDate = i.PurchaseDate
    };
}
