using Microsoft.EntityFrameworkCore;
using TechMoveSystems.Models;

namespace TechMoveSystems.Data;

public class TechMoveDbContext(DbContextOptions<TechMoveDbContext> options) : DbContext(options)
{
    public DbSet<Client> Clients => Set<Client>();
    public DbSet<Contract> Contracts => Set<Contract>();
    public DbSet<ServiceRequest> ServiceRequests => Set<ServiceRequest>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Client>()
            .Property(client => client.Name)
            .HasMaxLength(120);

        modelBuilder.Entity<Client>()
            .Property(client => client.ContactDetails)
            .HasMaxLength(180);

        modelBuilder.Entity<Client>()
            .Property(client => client.Region)
            .HasMaxLength(80);

        modelBuilder.Entity<Contract>()
            .Property(contract => contract.ServiceLevel)
            .HasMaxLength(80);

        modelBuilder.Entity<Contract>()
            .Property(contract => contract.SignedAgreementPath)
            .HasMaxLength(260);

        modelBuilder.Entity<ServiceRequest>()
            .Property(request => request.Description)
            .HasMaxLength(240);

        modelBuilder.Entity<ServiceRequest>()
            .Property(request => request.CostUsd)
            .HasPrecision(18, 2);

        modelBuilder.Entity<ServiceRequest>()
            .Property(request => request.CostZar)
            .HasPrecision(18, 2);

        modelBuilder.Entity<ServiceRequest>()
            .Property(request => request.ExchangeRate)
            .HasPrecision(18, 4);

        modelBuilder.Entity<Client>()
            .HasMany(client => client.Contracts)
            .WithOne(contract => contract.Client)
            .HasForeignKey(contract => contract.ClientId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Contract>()
            .HasMany(contract => contract.ServiceRequests)
            .WithOne(request => request.Contract)
            .HasForeignKey(request => request.ContractId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
