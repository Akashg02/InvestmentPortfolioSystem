using InvestmentPortfolio.Core.Entities;
using InvestmentPortfolio.Core.Interfaces.Repositories;
using InvestmentPortfolio.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace InvestmentPortfolio.Infrastructure.Repositories;

public class InvestmentRepository : Repository<Investment>, IInvestmentRepository
{
    public InvestmentRepository(ApplicationDbContext context) : base(context) { }

    public async Task<IEnumerable<Investment>> GetInvestmentsByPortfolioAsync(Guid portfolioId) =>
        await _dbSet
            .Where(i => i.PortfolioId == portfolioId)
            .OrderBy(i => i.Symbol)
            .ToListAsync();

    public async Task<Investment?> GetBySymbolAndPortfolioAsync(string symbol, Guid portfolioId) =>
        await _dbSet.FirstOrDefaultAsync(i =>
            i.Symbol == symbol.ToUpperInvariant() && i.PortfolioId == portfolioId);

    public async Task<IEnumerable<Investment>> GetInvestmentsBySymbolAsync(string symbol) =>
        await _dbSet.Where(i => i.Symbol == symbol.ToUpperInvariant()).ToListAsync();
}
