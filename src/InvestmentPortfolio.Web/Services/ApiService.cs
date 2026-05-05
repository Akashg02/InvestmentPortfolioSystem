using System.Net.Http.Headers;
using System.Net.Http.Json;
using Blazored.LocalStorage;
using InvestmentPortfolio.Shared.DTOs.Auth;
using InvestmentPortfolio.Shared.DTOs.Client;
using InvestmentPortfolio.Shared.DTOs.Common;
using InvestmentPortfolio.Shared.DTOs.Investment;
using InvestmentPortfolio.Shared.DTOs.MarketData;
using InvestmentPortfolio.Shared.DTOs.Portfolio;
using InvestmentPortfolio.Shared.DTOs.Transaction;

namespace InvestmentPortfolio.Web.Services;

public class ApiService : IApiService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILocalStorageService _localStorage;
    private readonly ILogger<ApiService> _logger;

    public ApiService(IHttpClientFactory httpClientFactory, ILocalStorageService localStorage, ILogger<ApiService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _localStorage = localStorage;
        _logger = logger;
    }

    private async Task<HttpClient> GetClientAsync()
    {
        var client = _httpClientFactory.CreateClient("API");
        var token = await _localStorage.GetItemAsStringAsync("authToken");
        if (!string.IsNullOrEmpty(token))
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    private async Task<ApiResponse<T>> GetAsync<T>(string url)
    {
        try
        {
            var client = await GetClientAsync();
            var result = await client.GetFromJsonAsync<ApiResponse<T>>(url);
            return result ?? ApiResponse<T>.Fail("No response from server.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GET {Url} failed", url);
            return ApiResponse<T>.Fail(ex.Message);
        }
    }

    private async Task<ApiResponse<T>> PostAsync<T>(string url, object body)
    {
        try
        {
            var client = await GetClientAsync();
            var response = await client.PostAsJsonAsync(url, body);
            return await response.Content.ReadFromJsonAsync<ApiResponse<T>>()
                   ?? ApiResponse<T>.Fail("No response.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "POST {Url} failed", url);
            return ApiResponse<T>.Fail(ex.Message);
        }
    }

    private async Task<ApiResponse<T>> PutAsync<T>(string url, object body)
    {
        try
        {
            var client = await GetClientAsync();
            var response = await client.PutAsJsonAsync(url, body);
            return await response.Content.ReadFromJsonAsync<ApiResponse<T>>()
                   ?? ApiResponse<T>.Fail("No response.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "PUT {Url} failed", url);
            return ApiResponse<T>.Fail(ex.Message);
        }
    }

    // Auth
    public Task<ApiResponse<AuthResponse>> LoginAsync(LoginRequest request) =>
        PostAsync<AuthResponse>("api/auth/login", request);

    public Task<ApiResponse<AuthResponse>> RegisterAsync(RegisterRequest request) =>
        PostAsync<AuthResponse>("api/auth/register", request);

    // Clients
    public Task<ApiResponse<IEnumerable<ClientDto>>> GetClientsAsync() =>
        GetAsync<IEnumerable<ClientDto>>("api/clients");

    public Task<ApiResponse<ClientDto>> GetClientAsync(Guid id) =>
        GetAsync<ClientDto>($"api/clients/{id}");

    public Task<ApiResponse<ClientDto>> CreateClientAsync(CreateClientRequest request) =>
        PostAsync<ClientDto>("api/clients", request);

    public Task<ApiResponse<ClientDto>> UpdateClientAsync(Guid id, UpdateClientRequest request) =>
        PutAsync<ClientDto>($"api/clients/{id}", request);

    public async Task<ApiResponse<bool>> DeleteClientAsync(Guid id)
    {
        var client = await GetClientAsync();
        var response = await client.DeleteAsync($"api/clients/{id}");
        return response.IsSuccessStatusCode
            ? ApiResponse<bool>.Ok(true)
            : ApiResponse<bool>.Fail("Delete failed.");
    }

    // Portfolios
    public Task<ApiResponse<IEnumerable<PortfolioDto>>> GetPortfoliosByClientAsync(Guid clientId) =>
        GetAsync<IEnumerable<PortfolioDto>>($"api/portfolios/by-client/{clientId}");

    public Task<ApiResponse<PortfolioDto>> GetPortfolioAsync(Guid id) =>
        GetAsync<PortfolioDto>($"api/portfolios/{id}");

    public Task<ApiResponse<PortfolioDto>> CreatePortfolioAsync(CreatePortfolioRequest request) =>
        PostAsync<PortfolioDto>("api/portfolios", request);

    public async Task RefreshPortfolioPricesAsync(Guid portfolioId)
    {
        var client = await GetClientAsync();
        await client.PostAsync($"api/portfolios/{portfolioId}/refresh-prices", null);
    }

    // Investments
    public Task<ApiResponse<IEnumerable<InvestmentDto>>> GetInvestmentsByPortfolioAsync(Guid portfolioId) =>
        GetAsync<IEnumerable<InvestmentDto>>($"api/investments/by-portfolio/{portfolioId}");

    public Task<ApiResponse<InvestmentDto>> AddInvestmentAsync(CreateInvestmentRequest request) =>
        PostAsync<InvestmentDto>("api/investments", request);

    // Transactions
    public Task<ApiResponse<IEnumerable<TransactionDto>>> GetTransactionsByPortfolioAsync(Guid portfolioId) =>
        GetAsync<IEnumerable<TransactionDto>>($"api/transactions/by-portfolio/{portfolioId}");

    // Market Data
    public async Task<StockQuoteDto?> GetStockQuoteAsync(string symbol)
    {
        try
        {
            var client = await GetClientAsync();
            return await client.GetFromJsonAsync<StockQuoteDto>($"api/market-data/quote/{symbol}");
        }
        catch { return null; }
    }

    public async Task<MarketSummaryDto> GetMarketSummaryAsync()
    {
        try
        {
            var client = await GetClientAsync();
            return await client.GetFromJsonAsync<MarketSummaryDto>("api/market-data/summary")
                   ?? new MarketSummaryDto();
        }
        catch { return new MarketSummaryDto(); }
    }
}
