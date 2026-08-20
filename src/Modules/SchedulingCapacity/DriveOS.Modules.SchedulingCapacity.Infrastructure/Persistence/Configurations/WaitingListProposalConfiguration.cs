using DriveOS.Modules.SchedulingCapacity.Domain.WaitingList;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DriveOS.Modules.SchedulingCapacity.Infrastructure.Persistence.Configurations;

internal sealed class WaitingListProposalConfiguration : IEntityTypeConfiguration<WaitingListProposal>
{
    public void Configure(EntityTypeBuilder<WaitingListProposal> builder)
    {
        builder.ToTable("waiting_list_proposals");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasConversion(x => x.Value, x => new WaitingListProposalId(x));
        builder.Property(x => x.WaitingListEntryId).HasConversion(x => x.Value, x => new WaitingListEntryId(x)).IsRequired();
        builder.Property(x => x.OrganizationId).HasConversion(x => x.Value, x => new OrganizationId(x)).IsRequired();
        builder.Property(x => x.BranchId).HasConversion(x => x.HasValue ? x.Value.Value : (Guid?)null, x => x.HasValue ? new BranchId(x.Value) : null);
        builder.Property(x => x.InstructorId).HasConversion(x => x.HasValue ? x.Value.Value : (Guid?)null, x => x.HasValue ? new UserId(x.Value) : null);
        builder.Property(x => x.FulfilledBookingId).HasConversion(x => x.HasValue ? x.Value.Value : (Guid?)null, x => x.HasValue ? new BookingId(x.Value) : null);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(x => x.SlotKey).HasMaxLength(180).IsRequired();
        builder.Property(x => x.ActiveHoldKey).HasMaxLength(180);
        builder.Property(x => x.DecisionReason).HasMaxLength(500);
        builder.HasIndex(x => x.ActiveHoldKey).IsUnique();
        builder.HasIndex(x => new { x.WaitingListEntryId, x.Status, x.ExpiresAtUtc });
    }
}
