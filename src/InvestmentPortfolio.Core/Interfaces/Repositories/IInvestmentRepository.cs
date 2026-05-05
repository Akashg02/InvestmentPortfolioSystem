using InvestmentPortfolio.Core.Entities;

namespace InvestmentPortfolio.Core.Interfaces.Repositories;

public interface IInvestmentRepository : IRepository<Investment>
{
    Task<IEnumerable<Investment>> GetInvestmentsByPortfolioAsync(Guid portfolioId);
    Task<Investment?> GetBySymbolAndPortfolioAsync(string symbol, Guid portfolioId);
    Task<IEnumerable<Investment>> GetInvestmentsBySymbolAsync(string symbol);
}
