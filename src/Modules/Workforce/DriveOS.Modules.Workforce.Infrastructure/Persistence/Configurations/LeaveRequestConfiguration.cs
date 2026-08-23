using DriveOS.Modules.Workforce.Domain.LeaveRequests;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace DriveOS.Modules.Workforce.Infrastructure.Persistence.Configurations;
internal sealed class LeaveRequestConfiguration : IEntityTypeConfiguration<LeaveRequest>
{
    public void Configure(EntityTypeBuilder<LeaveRequest> b)
    {
        b.ToTable("leave_requests"); b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasConversion(x => x.Value, x => new LeaveRequestId(x)).ValueGeneratedNever();
        b.Property(x => x.OrganizationId).HasConversion(x => x.Value, x => new OrganizationId(x)).IsRequired();
        b.Property(x => x.EmployeeId).HasConversion(x => x.Value, x => new EmployeeId(x)).IsRequired();
        b.Property(x => x.LeavePolicyId).HasConversion(x => x.Value, x => new LeavePolicyId(x)).IsRequired();
        b.Property(x => x.PolicyCode).HasMaxLength(64).IsRequired(); b.Property(x => x.CountryCode).HasMaxLength(2).IsRequired();
        b.Property(x => x.StartDate).IsRequired(); b.Property(x => x.EndDate).IsRequired();
        b.Property(x => x.StartPortion).HasConversion<string>().HasMaxLength(16).IsRequired(); b.Property(x => x.EndPortion).HasConversion<string>().HasMaxLength(16).IsRequired();
        b.Property(x => x.Reason).HasMaxLength(1000); b.Property(x => x.EvidenceDocumentId).HasConversion(x => x == null ? (Guid?)null : x.Value.Value, x => x == null ? null : new DocumentId(x.Value));
        b.Property(x => x.Status).HasConversion<string>().HasMaxLength(24).IsRequired();
        b.Property(x => x.DecidedByUserId).HasConversion(x => x == null ? (Guid?)null : x.Value.Value, x => x == null ? null : new UserId(x.Value));
        b.Property(x => x.CancelledByUserId).HasConversion(x => x == null ? (Guid?)null : x.Value.Value, x => x == null ? null : new UserId(x.Value));
        b.Property(x => x.CreatedByUserId).HasConversion(x => x == null ? (Guid?)null : x.Value.Value, x => x == null ? null : new UserId(x.Value));
        b.Property(x => x.LastModifiedByUserId).HasConversion(x => x == null ? (Guid?)null : x.Value.Value, x => x == null ? null : new UserId(x.Value));
        b.Property(x => x.DecisionReason).HasMaxLength(1000);
        b.HasIndex(x => new { x.OrganizationId, x.EmployeeId, x.Status });
        b.HasIndex(x => new { x.OrganizationId, x.StartDate, x.EndDate });
        b.HasIndex(x => new { x.OrganizationId, x.LeavePolicyId, x.Status });
    }
}
