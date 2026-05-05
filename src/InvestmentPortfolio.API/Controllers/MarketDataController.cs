using InvestmentPortfolio.Core.Interfaces.Services;
using InvestmentPortfolio.Shared.DTOs.MarketData;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InvestmentPortfolio.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MarketDataController : ControllerBase
{
    private readonly IMarketDataService _marketDataService;

    public MarketDataController(IMarketDataService marketDataService) => _marketDataService = marketDataService;

    /// <summary>Get real-time stock quote for a symbol</summary>
    [HttpGet("quote/{symbol}")]
    [ProducesResponseType(typeof(StockQuoteDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetQuote(string symbol)
    {
        var quote = await _marketDataService.GetQuoteAsync(symbol);
        return quote is null ? NotFound($"No quote found for symbol: {symbol}") : Ok(quote);
    }

    /// <summary>Get quotes for multiple symbols</summary>
    [HttpGet("quotes")]
    [ProducesResponseType(typeof(IEnumerable<StockQuoteDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetQuotes([FromQuery] string symbols)
    {
        var symbolList = symbols.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var quotes = await _marketDataService.GetQuotesAsync(symbolList);
        return Ok(quotes);
    }

    /// <summary>Get market summary with major indices</summary>
    [HttpGet("summary")]
    [ProducesResponseType(typeof(MarketSummaryDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMarketSummary()
    {
        var summary = await _marketDataService.GetMarketSummaryAsync();
        return Ok(summary);
    }
}
