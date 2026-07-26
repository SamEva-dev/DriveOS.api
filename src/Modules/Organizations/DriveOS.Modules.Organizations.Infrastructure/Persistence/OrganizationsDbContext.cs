using DriveOS.Application.Abstractions.Persistence;
using DriveOS.Modules.Organizations.Domain.Organizations;
using Microsoft.EntityFrameworkCore;

namespace DriveOS.Modules.Organizations.Infrastructure.Persistence;

public sealed class OrganizationsDbContext :
    DbContext,
    IUnitOfWork
{
    public OrganizationsDbContext(
        DbContextOptions<OrganizationsDbContext> options)
        : base(options)
    {
    }

    public DbSet<Organization> Organizations =>
        Set<Organization>();

    protected override void OnModelCreating(
        ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(
            OrganizationsSchema.Name);

        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(OrganizationsDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}