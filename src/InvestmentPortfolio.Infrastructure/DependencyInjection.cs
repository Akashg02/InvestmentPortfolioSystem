using InvestmentPortfolio.Core.Entities;
using InvestmentPortfolio.Core.Interfaces.Repositories;
using InvestmentPortfolio.Core.Interfaces.Services;
using InvestmentPortfolio.Core.Services;
using InvestmentPortfolio.Infrastructure.Data;
using InvestmentPortfolio.Infrastructure.ExternalApis;
using InvestmentPortfolio.Infrastructure.Identity;
using InvestmentPortfolio.Infrastructure.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace InvestmentPortfolio.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

        services.AddIdentity<ApplicationUser, IdentityRole>(options =>
        {
            options.Password.RequiredLength = 8;
            options.Password.RequireNonAlphanumeric = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireDigit = true;
            options.User.RequireUniqueEmail = true;
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
        })
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders();

        // Repositories
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IClientRepository, ClientRepository>();
        services.AddScoped<IPortfolioRepository, PortfolioRepository>();
        services.AddScoped<IInvestmentRepository, InvestmentRepository>();

        // Business Services
        services.AddScoped<IClientService, ClientService>();
        services.AddScoped<IPortfolioService, PortfolioService>();
        services.AddScoped<IInvestmentService, InvestmentService>();
        services.AddScoped<ITransactionService, TransactionService>();

        // Infrastructure Services
        services.AddScoped<JwtService>();
        services.AddScoped<DbInitializer>();

        // Market Data
        services.AddHttpClient<IMarketDataService, AlphaVantageMarketDataService>();

        return services;
    }
}
