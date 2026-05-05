using InvestmentPortfolio.Shared.DTOs.Common;
using InvestmentPortfolio.Shared.DTOs.Transaction;

namespace InvestmentPortfolio.Core.Interfaces.Services;

public interface ITransactionService
{
    Task<ApiResponse<IEnumerable<TransactionDto>>> GetTransactionsByPortfolioAsync(Guid portfolioId);
    Task<ApiResponse<TransactionDto>> GetTransactionByIdAsync(Guid id);
    Task<ApiResponse<TransactionDto>> CreateTransactionAsync(CreateTransactionRequest request);
}
