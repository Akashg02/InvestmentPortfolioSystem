using InvestmentPortfolio.Shared.Enums;

namespace InvestmentPortfolio.Shared.DTOs.Client;

public class ClientDto
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}";
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public RiskLevel RiskProfile { get; set; }
    public DateTime DateOfBirth { get; set; }
    public decimal TotalPortfolioValue { get; set; }
    public decimal TotalGainLoss { get; set; }
    public decimal TotalGainLossPercent { get; set; }
    public int PortfolioCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; }
}
