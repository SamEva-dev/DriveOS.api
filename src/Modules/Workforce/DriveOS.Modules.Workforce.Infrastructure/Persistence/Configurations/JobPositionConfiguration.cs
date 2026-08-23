using DriveOS.Modules.Workforce.Domain.JobPositions;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace DriveOS.Modules.Workforce.Infrastructure.Persistence.Configurations;
internal sealed class JobPositionConfiguration : IEntityTypeConfiguration<JobPosition>
{
    public void Configure(EntityTypeBuilder<JobPosition> b)
    {
        b.ToTable("job_positions");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasConversion(x => x.Value, x => new JobPositionId(x)).ValueGeneratedNever();
        b.Property(x => x.OrganizationId).HasConversion(x => x.Value, x => new OrganizationId(x)).IsRequired();
        b.Property(x => x.Code).HasMaxLength(64).IsRequired();
        b.Property(x => x.Name).HasMaxLength(160).IsRequired();
        b.Property(x => x.Description).HasMaxLength(1000);
        b.Property(x => x.ProfessionalFunction).HasConversion<string>().HasMaxLength(48).IsRequired();
        b.Property(x => x.Status).HasConversion<string>().HasMaxLength(24).IsRequired();
        b.Property(x => x.CreatedByUserId).HasConversion(x => x == null ? (Guid?)null : x.Value.Value, x => x == null ? null : new UserId(x.Value));
        b.Property(x => x.LastModifiedByUserId).HasConversion(x => x == null ? (Guid?)null : x.Value.Value, x => x == null ? null : new UserId(x.Value));
        b.HasIndex(x => new { x.OrganizationId, x.Code }).IsUnique();
        b.HasIndex(x => new { x.OrganizationId, x.ProfessionalFunction, x.Status });
    }
}
