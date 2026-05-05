using InvestmentPortfolio.Core.Entities;
using InvestmentPortfolio.Infrastructure.Identity;
using InvestmentPortfolio.Shared.DTOs.Auth;
using InvestmentPortfolio.Shared.DTOs.Common;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace InvestmentPortfolio.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly JwtService _jwtService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        JwtService jwtService,
        ILogger<AuthController> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _jwtService = jwtService;
        _logger = logger;
    }

    /// <summary>Register a new user account</summary>
    [HttpPost("register")]
    [ProducesResponseType(typeof(ApiResponse<AuthResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<AuthResponse>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<AuthResponse>.Fail("Invalid input", ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList()));

        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            return BadRequest(ApiResponse<AuthResponse>.Fail(
                "Registration failed",
                result.Errors.Select(e => e.Description).ToList()));
        }

        await _userManager.AddToRoleAsync(user, "Advisor");
        var roles = await _userManager.GetRolesAsync(user);
        var (token, expiresAt) = _jwtService.GenerateToken(user, roles);

        _logger.LogInformation("New user registered: {Email}", request.Email);

        return CreatedAtAction(nameof(Register), ApiResponse<AuthResponse>.Ok(new AuthResponse
        {
            Token = token,
            RefreshToken = JwtService.GenerateRefreshToken(),
            ExpiresAt = expiresAt,
            UserId = user.Id,
            Email = user.Email!,
            FullName = user.FullName,
            Roles = roles
        }, "Registration successful."));
    }

    /// <summary>Authenticate and receive JWT token</summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(ApiResponse<AuthResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<AuthResponse>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<AuthResponse>.Fail("Invalid input"));

        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null || !user.IsActive)
            return Unauthorized(ApiResponse<AuthResponse>.Fail("Invalid credentials."));

        var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);
        if (!result.Succeeded)
        {
            var reason = result.IsLockedOut ? "Account is locked out." : "Invalid credentials.";
            return Unauthorized(ApiResponse<AuthResponse>.Fail(reason));
        }

        user.LastLoginAt = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);

        var roles = await _userManager.GetRolesAsync(user);
        var (token, expiresAt) = _jwtService.GenerateToken(user, roles);

        _logger.LogInformation("User logged in: {Email}", request.Email);

        return Ok(ApiResponse<AuthResponse>.Ok(new AuthResponse
        {
            Token = token,
            RefreshToken = JwtService.GenerateRefreshToken(),
            ExpiresAt = expiresAt,
            UserId = user.Id,
            Email = user.Email!,
            FullName = user.FullName,
            Roles = roles
        }));
    }
}
