using InvestmentPortfolio.Core.Entities;
using InvestmentPortfolio.Core.Interfaces.Repositories;
using InvestmentPortfolio.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace InvestmentPortfolio.Infrastructure.Repositories;

public class PortfolioRepository : Repository<Portfolio>, IPortfolioRepository
{
    public PortfolioRepository(ApplicationDbContext context) : base(context) { }

    public async Task<Portfolio?> GetPortfolioWithInvestmentsAsync(Guid portfolioId) =>
        await _dbSet
            .Include(p => p.Client)
            .Include(p => p.Investments.Where(i => !i.IsDeleted))
            .Include(p => p.Transactions.Where(t => !t.IsDeleted))
            .FirstOrDefaultAsync(p => p.Id == portfolioId);

    public async Task<IEnumerable<Portfolio>> GetPortfoliosByClientAsync(Guid clientId) =>
        await _dbSet
            .Where(p => p.ClientId == clientId)
            .Include(p => p.Investments.Where(i => !i.IsDeleted))
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
}
