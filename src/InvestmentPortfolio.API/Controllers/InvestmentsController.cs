using InvestmentPortfolio.Core.Interfaces.Services;
using InvestmentPortfolio.Shared.DTOs.Common;
using InvestmentPortfolio.Shared.DTOs.Investment;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InvestmentPortfolio.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class InvestmentsController : ControllerBase
{
    private readonly IInvestmentService _investmentService;

    public InvestmentsController(IInvestmentService investmentService) => _investmentService = investmentService;

    /// <summary>Get all investments for a portfolio</summary>
    [HttpGet("by-portfolio/{portfolioId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<InvestmentDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByPortfolio(Guid portfolioId)
    {
        var result = await _investmentService.GetInvestmentsByPortfolioAsync(portfolioId);
        return Ok(result);
    }

    /// <summary>Get a specific investment</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<InvestmentDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _investmentService.GetInvestmentByIdAsync(id);
        return result.Success ? Ok(result) : NotFound(result);
    }

    /// <summary>Add a new investment (buy) to a portfolio</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<InvestmentDto>), StatusCodes.Status201Created)]
    public async Task<IActionResult> Add([FromBody] CreateInvestmentRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var result = await _investmentService.AddInvestmentAsync(request);
        if (!result.Success) return BadRequest(result);

        return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result);
    }

    /// <summary>Sell shares of an investment</summary>
    [HttpPost("{id:guid}/sell")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Sell(Guid id, [FromBody] SellInvestmentRequest request)
    {
        var result = await _investmentService.SellInvestmentAsync(id, request.Quantity, request.SalePrice);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>Remove an investment from portfolio</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _investmentService.DeleteInvestmentAsync(id);
        return result.Success ? NoContent() : NotFound(result);
    }
}

public record SellInvestmentRequest(decimal Quantity, decimal SalePrice);
