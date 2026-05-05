using System.Text.Json;
using InvestmentPortfolio.Core.Interfaces.Services;
using InvestmentPortfolio.Shared.DTOs.MarketData;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace InvestmentPortfolio.Infrastructure.ExternalApis;

public class AlphaVantageMarketDataService : IMarketDataService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly ILogger<AlphaVantageMarketDataService> _logger;

    private static readonly Dictionary<string, (StockQuoteDto Quote, DateTime CachedAt)> _cache = new();
    private static readonly TimeSpan _cacheDuration = TimeSpan.FromMinutes(5);

    public AlphaVantageMarketDataService(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<AlphaVantageMarketDataService> logger)
    {
        _httpClient = httpClient;
        _apiKey = configuration["MarketData:AlphaVantageApiKey"] ?? "demo";
        _logger = logger;
    }

    public async Task<StockQuoteDto?> GetQuoteAsync(string symbol)
    {
        var upperSymbol = symbol.ToUpperInvariant();

        if (_cache.TryGetValue(upperSymbol, out var cached) && DateTime.UtcNow - cached.CachedAt < _cacheDuration)
            return cached.Quote;

        try
        {
            var url = $"https://www.alphavantage.co/query?function=GLOBAL_QUOTE&symbol={upperSymbol}&apikey={_apiKey}";
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var doc = JsonDocument.Parse(json);

            if (!doc.RootElement.TryGetProperty("Global Quote", out var quoteEl))
                return GetFallbackQuote(upperSymbol);

            var quote = new StockQuoteDto
            {
                Symbol = upperSymbol,
                CompanyName = upperSymbol,
                Price = ParseDecimal(quoteEl, "05. price"),
                Open = ParseDecimal(quoteEl, "02. open"),
                High = ParseDecimal(quoteEl, "03. high"),
                Low = ParseDecimal(quoteEl, "04. low"),
                PreviousClose = ParseDecimal(quoteEl, "08. previous close"),
                Change = ParseDecimal(quoteEl, "09. change"),
                ChangePercent = ParseDecimalPercent(quoteEl, "10. change percent"),
                Volume = ParseLong(quoteEl, "06. volume"),
                LastUpdated = DateTime.UtcNow
            };

            _cache[upperSymbol] = (quote, DateTime.UtcNow);
            return quote;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to fetch quote for {Symbol}, using fallback", symbol);
            return GetFallbackQuote(upperSymbol);
        }
    }

    public async Task<IEnumerable<StockQuoteDto>> GetQuotesAsync(IEnumerable<string> symbols)
    {
        var tasks = symbols.Select(GetQuoteAsync);
        var results = await Task.WhenAll(tasks);
        return results.Where(q => q is not null).Cast<StockQuoteDto>();
    }

    public async Task<MarketSummaryDto> GetMarketSummaryAsync()
    {
        // Fetch major index quotes
        var spxTask = GetQuoteAsync("SPY");
        var nasdaqTask = GetQuoteAsync("QQQ");
        var dowTask = GetQuoteAsync("DIA");

        await Task.WhenAll(spxTask, nasdaqTask, dowTask);

        var spx = spxTask.Result;
        var nasdaq = nasdaqTask.Result;
        var dow = dowTask.Result;

        return new MarketSummaryDto
        {
            SpxValue = spx?.Price ?? 0,
            SpxChange = spx?.Change ?? 0,
            SpxChangePercent = spx?.ChangePercent ?? 0,
            NasdaqValue = nasdaq?.Price ?? 0,
            NasdaqChange = nasdaq?.Change ?? 0,
            NasdaqChangePercent = nasdaq?.ChangePercent ?? 0,
            DowValue = dow?.Price ?? 0,
            DowChange = dow?.Change ?? 0,
            DowChangePercent = dow?.ChangePercent ?? 0,
            MarketOpen = IsMarketOpen(),
            LastUpdated = DateTime.UtcNow
        };
    }

    private static StockQuoteDto GetFallbackQuote(string symbol)
    {
        var random = new Random(symbol.GetHashCode());
        var basePrice = 100m + random.Next(0, 400);
        var change = (decimal)(random.NextDouble() * 4 - 2);
        return new StockQuoteDto
        {
            Symbol = symbol,
            CompanyName = symbol,
            Price = basePrice + change,
            Change = change,
            ChangePercent = change / basePrice * 100,
            PreviousClose = basePrice,
            Open = basePrice + (decimal)(random.NextDouble() * 2 - 1),
            High = basePrice + Math.Abs(change) + (decimal)(random.NextDouble()),
            Low = basePrice - Math.Abs(change) - (decimal)(random.NextDouble()),
            Volume = random.Next(100000, 10000000),
            LastUpdated = DateTime.UtcNow
        };
    }

    private static bool IsMarketOpen()
    {
        var tzId = OperatingSystem.IsWindows() ? "Eastern Standard Time" : "America/New_York";
        var now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow,
            TimeZoneInfo.FindSystemTimeZoneById(tzId));
        return now.DayOfWeek is not DayOfWeek.Saturday and not DayOfWeek.Sunday
               && now.TimeOfDay >= TimeSpan.FromHours(9.5)
               && now.TimeOfDay <= TimeSpan.FromHours(16);
    }

    private static decimal ParseDecimal(JsonElement el, string key) =>
        el.TryGetProperty(key, out var prop) && decimal.TryParse(prop.GetString(), out var val) ? val : 0;

    private static decimal ParseDecimalPercent(JsonElement el, string key)
    {
        if (!el.TryGetProperty(key, out var prop)) return 0;
        var str = prop.GetString()?.TrimEnd('%') ?? "0";
        return decimal.TryParse(str, out var val) ? val : 0;
    }

    private static long ParseLong(JsonElement el, string key) =>
        el.TryGetProperty(key, out var prop) && long.TryParse(prop.GetString(), out var val) ? val : 0;
}
