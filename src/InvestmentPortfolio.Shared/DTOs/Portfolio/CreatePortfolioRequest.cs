using System.ComponentModel.DataAnnotations;
using InvestmentPortfolio.Shared.Enums;

namespace InvestmentPortfolio.Shared.DTOs.Portfolio;

public class CreatePortfolioRequest
{
    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string Description { get; set; } = string.Empty;

    [Required]
    public Guid ClientId { get; set; }

    public PortfolioStatus Status { get; set; } = PortfolioStatus.Active;
}
