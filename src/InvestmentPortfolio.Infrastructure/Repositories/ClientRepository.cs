using InvestmentPortfolio.Core.Entities;
using InvestmentPortfolio.Core.Interfaces.Repositories;
using InvestmentPortfolio.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace InvestmentPortfolio.Infrastructure.Repositories;

public class ClientRepository : Repository<Client>, IClientRepository
{
    public ClientRepository(ApplicationDbContext context) : base(context) { }

    public async Task<Client?> GetClientWithPortfoliosAsync(Guid clientId) =>
        await _dbSet
            .Include(c => c.Portfolios.Where(p => !p.IsDeleted))
                .ThenInclude(p => p.Investments.Where(i => !i.IsDeleted))
            .FirstOrDefaultAsync(c => c.Id == clientId);

    public async Task<IEnumerable<Client>> GetClientsByUserAsync(string userId) =>
        await _dbSet
            .Where(c => c.UserId == userId)
            .Include(c => c.Portfolios.Where(p => !p.IsDeleted))
                .ThenInclude(p => p.Investments.Where(i => !i.IsDeleted))
            .OrderBy(c => c.LastName)
            .ToListAsync();

    public async Task<bool> EmailExistsAsync(string email, Guid? excludeId = null) =>
        await _dbSet.AnyAsync(c =>
            c.Email == email && (excludeId == null || c.Id != excludeId));
}
