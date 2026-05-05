using InvestmentPortfolio.Shared.Enums;

namespace InvestmentPortfolio.Shared.DTOs.Transaction;

public class TransactionDto
{
    public Guid Id { get; set; }
    public Guid PortfolioId { get; set; }
    public Guid? InvestmentId { get; set; }
    public string Symbol { get; set; } = string.Empty;
    public TransactionType Type { get; set; }
    public decimal Quantity { get; set; }
    public decimal Price { get; set; }
    public decimal Amount { get; set; }
    public decimal Fees { get; set; }
    public string Notes { get; set; } = string.Empty;
    public DateTime TransactionDate { get; set; }
    public DateTime CreatedAt { get; set; }
}
