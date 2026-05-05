using System.ComponentModel.DataAnnotations;
using InvestmentPortfolio.Shared.Enums;

namespace InvestmentPortfolio.Shared.DTOs.Transaction;

public class CreateTransactionRequest
{
    [Required]
    public Guid PortfolioId { get; set; }

    public Guid? InvestmentId { get; set; }

    [MaxLength(20)]
    public string Symbol { get; set; } = string.Empty;

    [Required]
    public TransactionType Type { get; set; }

    [Range(0, double.MaxValue)]
    public decimal Quantity { get; set; }

    [Range(0, double.MaxValue)]
    public decimal Price { get; set; }

    [Required, Range(0.01, double.MaxValue)]
    public decimal Amount { get; set; }

    [Range(0, double.MaxValue)]
    public decimal Fees { get; set; }

    [MaxLength(500)]
    public string Notes { get; set; } = string.Empty;

    [Required]
    public DateTime TransactionDate { get; set; }
}
