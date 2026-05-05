using InvestmentPortfolio.Shared.Enums;

namespace InvestmentPortfolio.Core.Entities;

public class Portfolio : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Guid ClientId { get; set; }
    public PortfolioStatus Status { get; set; } = PortfolioStatus.Active;
    public decimal CashBalance { get; set; }

    public Client Client { get; set; } = null!;
    public ICollection<Investment> Investments { get; set; } = new List<Investment>();
    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}
