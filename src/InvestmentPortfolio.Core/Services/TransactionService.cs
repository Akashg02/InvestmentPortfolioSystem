using InvestmentPortfolio.Core.Entities;
using InvestmentPortfolio.Core.Interfaces.Repositories;
using InvestmentPortfolio.Core.Interfaces.Services;
using InvestmentPortfolio.Shared.DTOs.Common;
using InvestmentPortfolio.Shared.DTOs.Transaction;
using Microsoft.Extensions.Logging;

namespace InvestmentPortfolio.Core.Services;

public class TransactionService : ITransactionService
{
    private readonly IRepository<Transaction> _transactionRepository;
    private readonly ILogger<TransactionService> _logger;

    public TransactionService(IRepository<Transaction> transactionRepository, ILogger<TransactionService> logger)
    {
        _transactionRepository = transactionRepository;
        _logger = logger;
    }

    public async Task<ApiResponse<IEnumerable<TransactionDto>>> GetTransactionsByPortfolioAsync(Guid portfolioId)
    {
        var transactions = await _transactionRepository.FindAsync(t => t.PortfolioId == portfolioId);
        return ApiResponse<IEnumerable<TransactionDto>>.Ok(transactions.Select(MapToDto));
    }

    public async Task<ApiResponse<TransactionDto>> GetTransactionByIdAsync(Guid id)
    {
        var transaction = await _transactionRepository.GetByIdAsync(id);
        if (transaction is null)
            return ApiResponse<TransactionDto>.Fail($"Transaction {id} not found.");

        return ApiResponse<TransactionDto>.Ok(MapToDto(transaction));
    }

    public async Task<ApiResponse<TransactionDto>> CreateTransactionAsync(CreateTransactionRequest request)
    {
        var transaction = new Transaction
        {
            PortfolioId = request.PortfolioId,
            InvestmentId = request.InvestmentId,
            Symbol = request.Symbol,
            Type = request.Type,
            Quantity = request.Quantity,
            Price = request.Price,
            Amount = request.Amount,
            Fees = request.Fees,
            Notes = request.Notes,
            TransactionDate = request.TransactionDate
        };

        await _transactionRepository.AddAsync(transaction);
        await _transactionRepository.SaveChangesAsync();

        _logger.LogInformation("Created {Type} transaction for portfolio {PortfolioId}", request.Type, request.PortfolioId);
        return ApiResponse<TransactionDto>.Ok(MapToDto(transaction), "Transaction recorded.");
    }

    private static TransactionDto MapToDto(Transaction t) => new()
    {
        Id = t.Id,
        PortfolioId = t.PortfolioId,
        InvestmentId = t.InvestmentId,
        Symbol = t.Symbol,
        Type = t.Type,
        Quantity = t.Quantity,
        Price = t.Price,
        Amount = t.Amount,
        Fees = t.Fees,
        Notes = t.Notes,
        TransactionDate = t.TransactionDate,
        CreatedAt = t.CreatedAt
    };
}
