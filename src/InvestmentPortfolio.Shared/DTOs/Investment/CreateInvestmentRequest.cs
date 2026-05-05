using System.ComponentModel.DataAnnotations;
using InvestmentPortfolio.Shared.Enums;

namespace InvestmentPortfolio.Shared.DTOs.Investment;

public class CreateInvestmentRequest
{
    [Required]
    public Guid PortfolioId { get; set; }

    [Required, MaxLength(20)]
    public string Symbol { get; set; } = string.Empty;

    [Required]
    public AssetType AssetType { get; set; }

    [Required, Range(0.001, double.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
    public decimal Quantity { get; set; }

    [Required, Range(0.01, double.MaxValue, ErrorMessage = "Purchase price must be greater than 0")]
    public decimal PurchasePrice { get; set; }

    [Required]
    public DateTime PurchaseDate { get; set; }

    [MaxLength(500)]
    public string Notes { get; set; } = string.Empty;
}
