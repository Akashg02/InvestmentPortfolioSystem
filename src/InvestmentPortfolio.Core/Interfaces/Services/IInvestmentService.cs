using InvestmentPortfolio.Shared.DTOs.Common;
using InvestmentPortfolio.Shared.DTOs.Investment;

namespace InvestmentPortfolio.Core.Interfaces.Services;

public interface IInvestmentService
{
    Task<ApiResponse<IEnumerable<InvestmentDto>>> GetInvestmentsByPortfolioAsync(Guid portfolioId);
    Task<ApiResponse<InvestmentDto>> GetInvestmentByIdAsync(Guid id);
    Task<ApiResponse<InvestmentDto>> AddInvestmentAsync(CreateInvestmentRequest request);
    Task<ApiResponse<bool>> SellInvestmentAsync(Guid id, decimal quantity, decimal salePrice);
    Task<ApiResponse<bool>> DeleteInvestmentAsync(Guid id);
}
