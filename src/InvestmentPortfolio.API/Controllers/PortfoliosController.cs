using InvestmentPortfolio.Core.Interfaces.Services;
using InvestmentPortfolio.Shared.DTOs.Common;
using InvestmentPortfolio.Shared.DTOs.Portfolio;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InvestmentPortfolio.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PortfoliosController : ControllerBase
{
    private readonly IPortfolioService _portfolioService;

    public PortfoliosController(IPortfolioService portfolioService) => _portfolioService = portfolioService;

    /// <summary>Get all portfolios for a client</summary>
    [HttpGet("by-client/{clientId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<PortfolioDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByClient(Guid clientId)
    {
        var result = await _portfolioService.GetPortfoliosByClientAsync(clientId);
        return Ok(result);
    }

    /// <summary>Get portfolio with full investment details</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<PortfolioDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _portfolioService.GetPortfolioByIdAsync(id);
        return result.Success ? Ok(result) : NotFound(result);
    }

    /// <summary>Create a new portfolio</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<PortfolioDto>), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreatePortfolioRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var result = await _portfolioService.CreatePortfolioAsync(request);
        if (!result.Success) return BadRequest(result);

        return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result);
    }

    /// <summary>Update portfolio metadata</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<PortfolioDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(Guid id, [FromBody] CreatePortfolioRequest request)
    {
        var result = await _portfolioService.UpdatePortfolioAsync(id, request);
        return result.Success ? Ok(result) : NotFound(result);
    }

    /// <summary>Delete a portfolio</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _portfolioService.DeletePortfolioAsync(id);
        return result.Success ? NoContent() : NotFound(result);
    }

    /// <summary>Refresh all investment prices in a portfolio</summary>
    [HttpPost("{id:guid}/refresh-prices")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> RefreshPrices(Guid id)
    {
        await _portfolioService.RefreshPortfolioPricesAsync(id);
        return NoContent();
    }
}
