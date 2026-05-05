using InvestmentPortfolio.Shared.Enums;

namespace InvestmentPortfolio.Core.Entities;

public class Client : BaseEntity
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}";
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public RiskLevel RiskProfile { get; set; }
    public DateTime DateOfBirth { get; set; }
    public bool IsActive { get; set; } = true;
    public string UserId { get; set; } = string.Empty;

    public ApplicationUser User { get; set; } = null!;
    public ICollection<Portfolio> Portfolios { get; set; } = new List<Portfolio>();
}
