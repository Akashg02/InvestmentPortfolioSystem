using InvestmentPortfolio.Shared.DTOs.Client;
using InvestmentPortfolio.Shared.DTOs.Common;

namespace InvestmentPortfolio.Core.Interfaces.Services;

public interface IClientService
{
    Task<ApiResponse<IEnumerable<ClientDto>>> GetAllClientsAsync(string userId);
    Task<ApiResponse<ClientDto>> GetClientByIdAsync(Guid id);
    Task<ApiResponse<ClientDto>> CreateClientAsync(CreateClientRequest request, string userId);
    Task<ApiResponse<ClientDto>> UpdateClientAsync(Guid id, UpdateClientRequest request);
    Task<ApiResponse<bool>> DeleteClientAsync(Guid id);
}
