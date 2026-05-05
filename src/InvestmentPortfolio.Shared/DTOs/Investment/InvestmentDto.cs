using InvestmentPortfolio.Shared.Enums;

namespace InvestmentPortfolio.Shared.DTOs.Investment;

public class InvestmentDto
{
    public Guid Id { get; set; }
    public Guid PortfolioId { get; set; }
    public string Symbol { get; set; } = string.Empty;
    public string AssetName { get; set; } = string.Empty;
    public AssetType AssetType { get; set; }
    public decimal Quantity { get; set; }
    public decimal AverageCostBasis { get; set; }
    public decimal CurrentPrice { get; set; }
    public decimal CurrentValue { get; set; }
    public decimal TotalCost { get; set; }
    public decimal GainLoss { get; set; }
    public decimal GainLossPercent { get; set; }
    public decimal DayChange { get; set; }
    public decimal DayChangePercent { get; set; }
    public decimal PortfolioWeight { get; set; }
    public DateTime LastUpdated { get; set; }
    public DateTime PurchaseDate { get; set; }
}
