using InvestmentPortfolio.Core.Entities;
using InvestmentPortfolio.Core.Interfaces.Repositories;
using InvestmentPortfolio.Core.Interfaces.Services;
using InvestmentPortfolio.Shared.DTOs.Client;
using InvestmentPortfolio.Shared.DTOs.Common;
using Microsoft.Extensions.Logging;

namespace InvestmentPortfolio.Core.Services;

public class ClientService : IClientService
{
    private readonly IClientRepository _clientRepository;
    private readonly IPortfolioRepository _portfolioRepository;
    private readonly ILogger<ClientService> _logger;

    public ClientService(
        IClientRepository clientRepository,
        IPortfolioRepository portfolioRepository,
        ILogger<ClientService> logger)
    {
        _clientRepository = clientRepository;
        _portfolioRepository = portfolioRepository;
        _logger = logger;
    }

    public async Task<ApiResponse<IEnumerable<ClientDto>>> GetAllClientsAsync(string userId)
    {
        var clients = await _clientRepository.GetClientsByUserAsync(userId);
        var dtos = clients.Select(MapToDto);
        return ApiResponse<IEnumerable<ClientDto>>.Ok(dtos);
    }

    public async Task<ApiResponse<ClientDto>> GetClientByIdAsync(Guid id)
    {
        var client = await _clientRepository.GetClientWithPortfoliosAsync(id);
        if (client is null)
            return ApiResponse<ClientDto>.Fail($"Client {id} not found.");

        return ApiResponse<ClientDto>.Ok(MapToDto(client));
    }

    public async Task<ApiResponse<ClientDto>> CreateClientAsync(CreateClientRequest request, string userId)
    {
        if (await _clientRepository.EmailExistsAsync(request.Email))
            return ApiResponse<ClientDto>.Fail("A client with this email already exists.");

        var client = new Client
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            Phone = request.Phone,
            Address = request.Address,
            RiskProfile = request.RiskProfile,
            DateOfBirth = request.DateOfBirth,
            UserId = userId
        };

        await _clientRepository.AddAsync(client);
        await _clientRepository.SaveChangesAsync();

        _logger.LogInformation("Created client {ClientId} for user {UserId}", client.Id, userId);
        return ApiResponse<ClientDto>.Ok(MapToDto(client), "Client created successfully.");
    }

    public async Task<ApiResponse<ClientDto>> UpdateClientAsync(Guid id, UpdateClientRequest request)
    {
        var client = await _clientRepository.GetByIdAsync(id);
        if (client is null)
            return ApiResponse<ClientDto>.Fail($"Client {id} not found.");

        if (request.FirstName is not null) client.FirstName = request.FirstName;
        if (request.LastName is not null) client.LastName = request.LastName;
        if (request.Phone is not null) client.Phone = request.Phone;
        if (request.Address is not null) client.Address = request.Address;
        if (request.RiskProfile.HasValue) client.RiskProfile = request.RiskProfile.Value;
        if (request.IsActive.HasValue) client.IsActive = request.IsActive.Value;
        client.UpdatedAt = DateTime.UtcNow;

        _clientRepository.Update(client);
        await _clientRepository.SaveChangesAsync();

        return ApiResponse<ClientDto>.Ok(MapToDto(client), "Client updated successfully.");
    }

    public async Task<ApiResponse<bool>> DeleteClientAsync(Guid id)
    {
        var client = await _clientRepository.GetByIdAsync(id);
        if (client is null)
            return ApiResponse<bool>.Fail($"Client {id} not found.");

        client.IsDeleted = true;
        client.UpdatedAt = DateTime.UtcNow;
        _clientRepository.Update(client);
        await _clientRepository.SaveChangesAsync();

        return ApiResponse<bool>.Ok(true, "Client deleted successfully.");
    }

    private static ClientDto MapToDto(Client client)
    {
        var totalValue = client.Portfolios?.Sum(p =>
            p.Investments?.Sum(i => i.CurrentValue) ?? 0) ?? 0;

        var totalCost = client.Portfolios?.Sum(p =>
            p.Investments?.Sum(i => i.TotalCost) ?? 0) ?? 0;

        return new ClientDto
        {
            Id = client.Id,
            FirstName = client.FirstName,
            LastName = client.LastName,
            Email = client.Email,
            Phone = client.Phone,
            Address = client.Address,
            RiskProfile = client.RiskProfile,
            DateOfBirth = client.DateOfBirth,
            TotalPortfolioValue = totalValue,
            TotalGainLoss = totalValue - totalCost,
            TotalGainLossPercent = totalCost > 0 ? ((totalValue - totalCost) / totalCost) * 100 : 0,
            PortfolioCount = client.Portfolios?.Count ?? 0,
            CreatedAt = client.CreatedAt,
            IsActive = client.IsActive
        };
    }
}
