using InvestmentPortfolio.Shared.DTOs.MarketData;

namespace InvestmentPortfolio.Core.Interfaces.Services;

public interface IMarketDataService
{
    Task<StockQuoteDto?> GetQuoteAsync(string symbol);
    Task<IEnumerable<StockQuoteDto>> GetQuotesAsync(IEnumerable<string> symbols);
    Task<MarketSummaryDto> GetMarketSummaryAsync();
}
