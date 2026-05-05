using InvestmentPortfolio.Core.Entities;

namespace InvestmentPortfolio.Core.Interfaces.Repositories;

public interface IPortfolioRepository : IRepository<Portfolio>
{
    Task<Portfolio?> GetPortfolioWithInvestmentsAsync(Guid portfolioId);
    Task<IEnumerable<Portfolio>> GetPortfoliosByClientAsync(Guid clientId);
}
