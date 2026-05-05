namespace InvestmentPortfolio.Shared.DTOs.MarketData;

public class MarketSummaryDto
{
    public decimal SpxValue { get; set; }
    public decimal SpxChange { get; set; }
    public decimal SpxChangePercent { get; set; }
    public decimal NasdaqValue { get; set; }
    public decimal NasdaqChange { get; set; }
    public decimal NasdaqChangePercent { get; set; }
    public decimal DowValue { get; set; }
    public decimal DowChange { get; set; }
    public decimal DowChangePercent { get; set; }
    public bool MarketOpen { get; set; }
    public DateTime LastUpdated { get; set; }
    public List<StockQuoteDto> TopGainers { get; set; } = new();
    public List<StockQuoteDto> TopLosers { get; set; } = new();
}
