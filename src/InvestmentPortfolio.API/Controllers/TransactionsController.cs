using InvestmentPortfolio.Core.Interfaces.Services;
using InvestmentPortfolio.Shared.DTOs.Common;
using InvestmentPortfolio.Shared.DTOs.Transaction;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InvestmentPortfolio.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TransactionsController : ControllerBase
{
    private readonly ITransactionService _transactionService;

    public TransactionsController(ITransactionService transactionService) => _transactionService = transactionService;

    /// <summary>Get all transactions for a portfolio</summary>
    [HttpGet("by-portfolio/{portfolioId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<TransactionDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByPortfolio(Guid portfolioId)
    {
        var result = await _transactionService.GetTransactionsByPortfolioAsync(portfolioId);
        return Ok(result);
    }

    /// <summary>Get a specific transaction</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<TransactionDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _transactionService.GetTransactionByIdAsync(id);
        return result.Success ? Ok(result) : NotFound(result);
    }

    /// <summary>Record a new transaction</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<TransactionDto>), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateTransactionRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var result = await _transactionService.CreateTransactionAsync(request);
        if (!result.Success) return BadRequest(result);

        return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result);
    }
}
