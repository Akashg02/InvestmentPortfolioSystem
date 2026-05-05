using InvestmentPortfolio.Shared.DTOs.Auth;
using InvestmentPortfolio.Shared.DTOs.Client;
using InvestmentPortfolio.Shared.DTOs.Common;
using InvestmentPortfolio.Shared.DTOs.Investment;
using InvestmentPortfolio.Shared.DTOs.MarketData;
using InvestmentPortfolio.Shared.DTOs.Portfolio;
using InvestmentPortfolio.Shared.DTOs.Transaction;

namespace InvestmentPortfolio.Web.Services;

public interface IApiService
{
    // Auth
    Task<ApiResponse<AuthResponse>> LoginAsync(LoginRequest request);
    Task<ApiResponse<AuthResponse>> RegisterAsync(RegisterRequest request);

    // Clients
    Task<ApiResponse<IEnumerable<ClientDto>>> GetClientsAsync();
    Task<ApiResponse<ClientDto>> GetClientAsync(Guid id);
    Task<ApiResponse<ClientDto>> CreateClientAsync(CreateClientRequest request);
    Task<ApiResponse<ClientDto>> UpdateClientAsync(Guid id, UpdateClientRequest request);
    Task<ApiResponse<bool>> DeleteClientAsync(Guid id);

    // Portfolios
    Task<ApiResponse<IEnumerable<PortfolioDto>>> GetPortfoliosByClientAsync(Guid clientId);
    Task<ApiResponse<PortfolioDto>> GetPortfolioAsync(Guid id);
    Task<ApiResponse<PortfolioDto>> CreatePortfolioAsync(CreatePortfolioRequest request);
    Task RefreshPortfolioPricesAsync(Guid portfolioId);

    // Investments
    Task<ApiResponse<IEnumerable<InvestmentDto>>> GetInvestmentsByPortfolioAsync(Guid portfolioId);
    Task<ApiResponse<InvestmentDto>> AddInvestmentAsync(CreateInvestmentRequest request);

    // Transactions
    Task<ApiResponse<IEnumerable<TransactionDto>>> GetTransactionsByPortfolioAsync(Guid portfolioId);

    // Market Data
    Task<StockQuoteDto?> GetStockQuoteAsync(string symbol);
    Task<MarketSummaryDto> GetMarketSummaryAsync();
}
