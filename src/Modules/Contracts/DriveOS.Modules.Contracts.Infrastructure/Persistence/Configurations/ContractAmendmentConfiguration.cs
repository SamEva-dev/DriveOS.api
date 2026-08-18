using DriveOS.Modules.Contracts.Domain.ContractAmendments;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DriveOS.Modules.Contracts.Infrastructure.Persistence.Configurations;

internal sealed class ContractAmendmentConfiguration : IEntityTypeConfiguration<ContractAmendment>
{
    public void Configure(EntityTypeBuilder<ContractAmendment> b)
    {
        b.ToTable("contract_amendments");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasConversion(x => x.Value, x => new ContractAmendmentId(x));
        b.Property(x => x.OrganizationId).HasConversion(x => x.Value, x => new OrganizationId(x)).IsRequired();
        b.Property(x => x.ContractId).HasConversion(x => x.Value, x => new TrainingContractId(x)).IsRequired();
        b.HasIndex(x => new { x.OrganizationId, x.ContractId, x.AmendmentNumber }).IsUnique();
        b.Property(x => x.Reason).HasMaxLength(500).IsRequired();
        b.Property(x => x.Currency).HasMaxLength(3).IsRequired();
        b.Property(x => x.TotalAmount).HasPrecision(18, 2);
        b.Property(x => x.SignedDocumentReference).HasMaxLength(500);
        b.Property(x => x.SignedDocumentSha256).HasMaxLength(64);
        b.Property(x => x.SignatureRecordedByUserId).HasConversion(x => x == null ? (Guid?)null : x.Value.Value, x => x.HasValue ? new UserId(x.Value) : null);
        b.Property(x => x.AppliedByUserId).HasConversion(x => x == null ? (Guid?)null : x.Value.Value, x => x.HasValue ? new UserId(x.Value) : null);
        b.Property(x => x.CancelledByUserId).HasConversion(x => x == null ? (Guid?)null : x.Value.Value, x => x.HasValue ? new UserId(x.Value) : null);
        b.Property(x => x.CancellationReason).HasMaxLength(500);
        b.Property(x => x.CreatedByUserId).HasConversion(x => x == null ? (Guid?)null : x.Value.Value, x => x.HasValue ? new UserId(x.Value) : null);
        b.Property(x => x.LastModifiedByUserId).HasConversion(x => x == null ? (Guid?)null : x.Value.Value, x => x.HasValue ? new UserId(x.Value) : null);
        b.OwnsOne(x => x.TermsSnapshot, terms => terms.ToJson("terms_snapshot"));
    }
}
