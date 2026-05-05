namespace InvestmentPortfolio.Shared.DTOs.MarketData;

public class StockQuoteDto
{
    public string Symbol { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public decimal Change { get; set; }
    public decimal ChangePercent { get; set; }
    public decimal Open { get; set; }
    public decimal High { get; set; }
    public decimal Low { get; set; }
    public decimal PreviousClose { get; set; }
    public long Volume { get; set; }
    public decimal MarketCap { get; set; }
    public decimal PeRatio { get; set; }
    public decimal FiftyTwoWeekHigh { get; set; }
    public decimal FiftyTwoWeekLow { get; set; }
    public DateTime LastUpdated { get; set; }
}
