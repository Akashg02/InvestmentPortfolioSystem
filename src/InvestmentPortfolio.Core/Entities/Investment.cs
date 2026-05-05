using InvestmentPortfolio.Shared.Enums;

namespace InvestmentPortfolio.Core.Entities;

public class Investment : BaseEntity
{
    public Guid PortfolioId { get; set; }
    public string Symbol { get; set; } = string.Empty;
    public string AssetName { get; set; } = string.Empty;
    public AssetType AssetType { get; set; }
    public decimal Quantity { get; set; }
    public decimal AverageCostBasis { get; set; }
    public decimal CurrentPrice { get; set; }
    public decimal PreviousClose { get; set; }
    public string Notes { get; set; } = string.Empty;
    public DateTime PurchaseDate { get; set; }
    public DateTime PriceLastUpdated { get; set; } = DateTime.UtcNow;

    public decimal CurrentValue => Quantity * CurrentPrice;
    public decimal TotalCost => Quantity * AverageCostBasis;
    public decimal GainLoss => CurrentValue - TotalCost;
    public decimal GainLossPercent => TotalCost > 0 ? (GainLoss / TotalCost) * 100 : 0;
    public decimal DayChange => CurrentPrice - PreviousClose;
    public decimal DayChangePercent => PreviousClose > 0 ? (DayChange / PreviousClose) * 100 : 0;

    public Portfolio Portfolio { get; set; } = null!;
    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}
