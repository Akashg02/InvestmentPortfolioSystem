using InvestmentPortfolio.Core.Entities;

namespace InvestmentPortfolio.Core.Interfaces.Repositories;

public interface IClientRepository : IRepository<Client>
{
    Task<Client?> GetClientWithPortfoliosAsync(Guid clientId);
    Task<IEnumerable<Client>> GetClientsByUserAsync(string userId);
    Task<bool> EmailExistsAsync(string email, Guid? excludeId = null);
}
