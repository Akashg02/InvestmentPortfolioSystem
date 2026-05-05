using InvestmentPortfolio.Shared.DTOs.Common;
using InvestmentPortfolio.Shared.DTOs.Portfolio;

namespace InvestmentPortfolio.Core.Interfaces.Services;

public interface IPortfolioService
{
    Task<ApiResponse<IEnumerable<PortfolioDto>>> GetPortfoliosByClientAsync(Guid clientId);
    Task<ApiResponse<PortfolioDto>> GetPortfolioByIdAsync(Guid id);
    Task<ApiResponse<PortfolioDto>> CreatePortfolioAsync(CreatePortfolioRequest request);
    Task<ApiResponse<PortfolioDto>> UpdatePortfolioAsync(Guid id, CreatePortfolioRequest request);
    Task<ApiResponse<bool>> DeletePortfolioAsync(Guid id);
    Task RefreshPortfolioPricesAsync(Guid portfolioId);
}
