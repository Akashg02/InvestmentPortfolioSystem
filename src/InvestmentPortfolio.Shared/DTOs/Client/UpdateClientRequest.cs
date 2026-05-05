using System.ComponentModel.DataAnnotations;
using InvestmentPortfolio.Shared.Enums;

namespace InvestmentPortfolio.Shared.DTOs.Client;

public class UpdateClientRequest
{
    [MaxLength(100)]
    public string? FirstName { get; set; }

    [MaxLength(100)]
    public string? LastName { get; set; }

    [Phone]
    public string? Phone { get; set; }

    [MaxLength(500)]
    public string? Address { get; set; }

    public RiskLevel? RiskProfile { get; set; }

    public bool? IsActive { get; set; }
}
