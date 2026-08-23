using DriveOS.Modules.Workforce.Domain.WorkingTime;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DriveOS.Modules.Workforce.Infrastructure.Persistence.Configurations;

internal sealed class WorkingTimePolicyConfiguration : IEntityTypeConfiguration<WorkingTimePolicy>
{
    public void Configure(EntityTypeBuilder<WorkingTimePolicy> b)
    {
        b.ToTable("working_time_policies");

        b.HasKey(x => x.Id);

        b.Property(x => x.Id)
            .HasConversion(x => x.Value, x => new WorkingTimePolicyId(x))
            .ValueGeneratedNever();

        b.Property(x => x.OrganizationId)
            .HasConversion(x => x.Value, x => new OrganizationId(x))
            .IsRequired();

        b.Property(x => x.EmployeeId)
            .HasConversion(x => x.Value, x => new EmployeeId(x))
            .IsRequired();

        b.Property(x => x.CreatedByUserId)
            .HasConversion(
                x => x.HasValue ? x.Value.Value : (Guid?)null,
                x => x.HasValue ? new UserId(x.Value) : null);

        b.Property(x => x.LastModifiedByUserId)
            .HasConversion(
                x => x.HasValue ? x.Value.Value : (Guid?)null,
                x => x.HasValue ? new UserId(x.Value) : null);

        b.Property(x => x.ContractualWeeklyHours)
            .HasPrecision(8, 2);

        b.Property(x => x.ContractualDailyHours)
            .HasPrecision(8, 2);

        b.Property(x => x.Status)
            .HasConversion<int>();

        b.HasIndex(x => new
        {
            x.OrganizationId,
            x.EmployeeId,
            x.EffectiveFrom
        });
    }
}
