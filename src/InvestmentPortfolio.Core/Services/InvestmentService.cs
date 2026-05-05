using InvestmentPortfolio.Core.Entities;
using InvestmentPortfolio.Core.Interfaces.Repositories;
using InvestmentPortfolio.Core.Interfaces.Services;
using InvestmentPortfolio.Shared.DTOs.Common;
using InvestmentPortfolio.Shared.DTOs.Investment;
using InvestmentPortfolio.Shared.Enums;
using Microsoft.Extensions.Logging;

namespace InvestmentPortfolio.Core.Services;

public class InvestmentService : IInvestmentService
{
    private readonly IInvestmentRepository _investmentRepository;
    private readonly IPortfolioRepository _portfolioRepository;
    private readonly IMarketDataService _marketDataService;
    private readonly ILogger<InvestmentService> _logger;

    public InvestmentService(
        IInvestmentRepository investmentRepository,
        IPortfolioRepository portfolioRepository,
        IMarketDataService marketDataService,
        ILogger<InvestmentService> logger)
    {
        _investmentRepository = investmentRepository;
        _portfolioRepository = portfolioRepository;
        _marketDataService = marketDataService;
        _logger = logger;
    }

    public async Task<ApiResponse<IEnumerable<InvestmentDto>>> GetInvestmentsByPortfolioAsync(Guid portfolioId)
    {
        var investments = await _investmentRepository.GetInvestmentsByPortfolioAsync(portfolioId);
        var dtos = investments.Select(MapToDto);
        return ApiResponse<IEnumerable<InvestmentDto>>.Ok(dtos);
    }

    public async Task<ApiResponse<InvestmentDto>> GetInvestmentByIdAsync(Guid id)
    {
        var investment = await _investmentRepository.GetByIdAsync(id);
        if (investment is null)
            return ApiResponse<InvestmentDto>.Fail($"Investment {id} not found.");

        return ApiResponse<InvestmentDto>.Ok(MapToDto(investment));
    }

    public async Task<ApiResponse<InvestmentDto>> AddInvestmentAsync(CreateInvestmentRequest request)
    {
        var portfolio = await _portfolioRepository.GetByIdAsync(request.PortfolioId);
        if (portfolio is null)
            return ApiResponse<InvestmentDto>.Fail("Portfolio not found.");

        var quote = await _marketDataService.GetQuoteAsync(request.Symbol);

        var existing = await _investmentRepository.GetBySymbolAndPortfolioAsync(request.Symbol, request.PortfolioId);

        if (existing is not null)
        {
            // Update average cost basis with weighted average
            var totalQuantity = existing.Quantity + request.Quantity;
            existing.AverageCostBasis = ((existing.Quantity * existing.AverageCostBasis) + (request.Quantity * request.PurchasePrice)) / totalQuantity;
            existing.Quantity = totalQuantity;
            existing.CurrentPrice = quote?.Price ?? request.PurchasePrice;
            existing.AssetName = quote?.CompanyName ?? existing.AssetName;
            existing.UpdatedAt = DateTime.UtcNow;

            _investmentRepository.Update(existing);
            await _investmentRepository.SaveChangesAsync();
            return ApiResponse<InvestmentDto>.Ok(MapToDto(existing), "Investment position updated.");
        }

        var investment = new Investment
        {
            PortfolioId = request.PortfolioId,
            Symbol = request.Symbol.ToUpperInvariant(),
            AssetName = quote?.CompanyName ?? request.Symbol,
            AssetType = request.AssetType,
            Quantity = request.Quantity,
            AverageCostBasis = request.PurchasePrice,
            CurrentPrice = quote?.Price ?? request.PurchasePrice,
            PreviousClose = quote?.PreviousClose ?? request.PurchasePrice,
            PurchaseDate = request.PurchaseDate,
            Notes = request.Notes
        };

        await _investmentRepository.AddAsync(investment);
        await _investmentRepository.SaveChangesAsync();

        _logger.LogInformation("Added investment {Symbol} to portfolio {PortfolioId}", investment.Symbol, request.PortfolioId);
        return ApiResponse<InvestmentDto>.Ok(MapToDto(investment), "Investment added successfully.");
    }

    public async Task<ApiResponse<bool>> SellInvestmentAsync(Guid id, decimal quantity, decimal salePrice)
    {
        var investment = await _investmentRepository.GetByIdAsync(id);
        if (investment is null)
            return ApiResponse<bool>.Fail("Investment not found.");

        if (quantity > investment.Quantity)
            return ApiResponse<bool>.Fail($"Cannot sell {quantity} shares; only {investment.Quantity} held.");

        investment.Quantity -= quantity;
        investment.UpdatedAt = DateTime.UtcNow;

        if (investment.Quantity == 0)
        {
            investment.IsDeleted = true;
        }
        else
        {
            _investmentRepository.Update(investment);
        }

        await _investmentRepository.SaveChangesAsync();
        return ApiResponse<bool>.Ok(true, "Investment sold successfully.");
    }

    public async Task<ApiResponse<bool>> DeleteInvestmentAsync(Guid id)
    {
        var investment = await _investmentRepository.GetByIdAsync(id);
        if (investment is null)
            return ApiResponse<bool>.Fail("Investment not found.");

        investment.IsDeleted = true;
        _investmentRepository.Update(investment);
        await _investmentRepository.SaveChangesAsync();

        return ApiResponse<bool>.Ok(true, "Investment removed.");
    }

    private static InvestmentDto MapToDto(Investment i) => new()
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
