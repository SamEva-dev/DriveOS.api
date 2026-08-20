using DriveOS.Modules.SchedulingCapacity.Domain.WaitingList;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DriveOS.Modules.SchedulingCapacity.Infrastructure.Persistence.Configurations;

internal sealed class WaitingListEntryConfiguration : IEntityTypeConfiguration<WaitingListEntry>
{
    public void Configure(EntityTypeBuilder<WaitingListEntry> builder)
    {
        builder.ToTable("waiting_list_entries");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasConversion(x => x.Value, x => new WaitingListEntryId(x));
        builder.Property(x => x.OrganizationId).HasConversion(x => x.Value, x => new OrganizationId(x)).IsRequired();
        builder.Property(x => x.StudentId).HasConversion(x => x.Value, x => new PersonId(x)).IsRequired();
        builder.Property(x => x.RequestedSessionType).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(x => x.PreferredBranchId).HasConversion(x => x.HasValue ? x.Value.Value : (Guid?)null, x => x.HasValue ? new BranchId(x.Value) : null);
        builder.Property(x => x.PreferredInstructorId).HasConversion(x => x.HasValue ? x.Value.Value : (Guid?)null, x => x.HasValue ? new UserId(x.Value) : null);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(x => x.PriorityExplanation).HasMaxLength(500).IsRequired();
        builder.Property(x => x.Reason).HasMaxLength(1000).IsRequired();
        builder.Property(x => x.CreatedByUserId).HasConversion(x => x.HasValue ? x.Value.Value : (Guid?)null, x => x.HasValue ? new UserId(x.Value) : null);
        builder.Property(x => x.LastModifiedByUserId).HasConversion(x => x.HasValue ? x.Value.Value : (Guid?)null, x => x.HasValue ? new UserId(x.Value) : null);
        builder.HasMany(x => x.Proposals).WithOne().HasForeignKey(x => x.WaitingListEntryId).OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(x => x.Proposals).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.HasIndex(x => new { x.OrganizationId, x.Status, x.PriorityScore, x.CreatedAtUtc });
        builder.HasIndex(x => new { x.OrganizationId, x.StudentId, x.Status });
        builder.HasIndex(x => new { x.OrganizationId, x.PreferredFromUtc, x.PreferredToUtc });
    }
}
