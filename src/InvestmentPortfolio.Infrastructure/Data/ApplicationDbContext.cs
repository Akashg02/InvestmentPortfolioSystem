using InvestmentPortfolio.Core.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace InvestmentPortfolio.Infrastructure.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<Client> Clients => Set<Client>();
    public DbSet<Portfolio> Portfolios => Set<Portfolio>();
    public DbSet<Investment> Investments => Set<Investment>();
    public DbSet<Transaction> Transactions => Set<Transaction>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Client>(e =>
        {
            e.HasKey(c => c.Id);
            e.HasIndex(c => c.Email).IsUnique();
            e.Property(c => c.FirstName).HasMaxLength(100).IsRequired();
            e.Property(c => c.LastName).HasMaxLength(100).IsRequired();
            e.Property(c => c.Email).HasMaxLength(256).IsRequired();
            e.HasQueryFilter(c => !c.IsDeleted);
            e.HasOne(c => c.User).WithMany(u => u.Clients).HasForeignKey(c => c.UserId);
            e.HasMany(c => c.Portfolios).WithOne(p => p.Client).HasForeignKey(p => p.ClientId);
        });

        builder.Entity<Portfolio>(e =>
        {
            e.HasKey(p => p.Id);
            e.Property(p => p.Name).HasMaxLength(200).IsRequired();
            e.Property(p => p.CashBalance).HasColumnType("decimal(18,4)");
            e.HasQueryFilter(p => !p.IsDeleted);
            e.HasMany(p => p.Investments).WithOne(i => i.Portfolio).HasForeignKey(i => i.PortfolioId);
            e.HasMany(p => p.Transactions).WithOne(t => t.Portfolio).HasForeignKey(t => t.PortfolioId);
        });

        builder.Entity<Investment>(e =>
        {
            e.HasKey(i => i.Id);
            e.Property(i => i.Symbol).HasMaxLength(20).IsRequired();
            e.Property(i => i.Quantity).HasColumnType("decimal(18,6)");
            e.Property(i => i.AverageCostBasis).HasColumnType("decimal(18,4)");
            e.Property(i => i.CurrentPrice).HasColumnType("decimal(18,4)");
            e.Property(i => i.PreviousClose).HasColumnType("decimal(18,4)");
            e.HasIndex(i => new { i.PortfolioId, i.Symbol });
            e.HasQueryFilter(i => !i.IsDeleted);
            e.Ignore(i => i.CurrentValue);
            e.Ignore(i => i.TotalCost);
            e.Ignore(i => i.GainLoss);
            e.Ignore(i => i.GainLossPercent);
            e.Ignore(i => i.DayChange);
            e.Ignore(i => i.DayChangePercent);
        });

        builder.Entity<Transaction>(e =>
        {
            e.HasKey(t => t.Id);
            e.Property(t => t.Amount).HasColumnType("decimal(18,4)");
            e.Property(t => t.Price).HasColumnType("decimal(18,4)");
            e.Property(t => t.Quantity).HasColumnType("decimal(18,6)");
            e.Property(t => t.Fees).HasColumnType("decimal(18,4)");
            e.HasQueryFilter(t => !t.IsDeleted);
        });
    }
}
