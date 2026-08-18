using DriveOS.Modules.Contracts.Domain.TrainingContracts;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace DriveOS.Modules.Contracts.Infrastructure.Persistence.Configurations;
internal sealed class TrainingContractConfiguration : IEntityTypeConfiguration<TrainingContract>
{
    public void Configure(EntityTypeBuilder<TrainingContract> b)
    {
        b.ToTable("training_contracts");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasConversion(x => x.Value, x => new TrainingContractId(x));
        b.Property(x => x.OrganizationId).HasConversion(x => x.Value, x => new OrganizationId(x)).IsRequired();
        b.Property(x => x.BranchId).HasConversion(x => x.Value, x => new BranchId(x)).IsRequired();
        b.Property(x => x.StudentId).HasConversion(x => x.Value, x => new PersonId(x)).IsRequired();
        b.Property(x => x.SourceOfferId).HasConversion(x => x.Value, x => new CommercialOfferId(x)).IsRequired();
        b.Property(x => x.ContractNumber).HasMaxLength(80).IsRequired();
        b.HasIndex(x => new { x.OrganizationId, x.ContractNumber }).IsUnique();
        b.Property(x => x.Currency).HasMaxLength(3).IsRequired();
        b.Property(x => x.TotalAmount).HasPrecision(18, 2);
        b.Property(x => x.GeneratedDocumentReference).HasMaxLength(500);
        b.Property(x => x.GeneratedDocumentFileName).HasMaxLength(180);
        b.Property(x => x.GeneratedDocumentContentType).HasMaxLength(100);
        b.Property(x => x.GeneratedDocumentSha256).HasMaxLength(64);
        b.Property(x => x.GeneratedByUserId).HasConversion(x => x == null ? (Guid?)null : x.Value.Value, x => x.HasValue ? new UserId(x.Value) : null);
        b.Property(x => x.ActivatedByUserId).HasConversion(x => x == null ? (Guid?)null : x.Value.Value, x => x.HasValue ? new UserId(x.Value) : null);
        b.Property(x => x.SuspensionReason).HasMaxLength(500);
        b.Property(x => x.SuspendedByUserId).HasConversion(x => x == null ? (Guid?)null : x.Value.Value, x => x.HasValue ? new UserId(x.Value) : null);
        b.Property(x => x.TerminationReason).HasMaxLength(500);
        b.Property(x => x.TerminatedByUserId).HasConversion(x => x == null ? (Guid?)null : x.Value.Value, x => x.HasValue ? new UserId(x.Value) : null);
        b.Property(x => x.CompletionNote).HasMaxLength(500);
        b.Property(x => x.CompletedByUserId).HasConversion(x => x == null ? (Guid?)null : x.Value.Value, x => x.HasValue ? new UserId(x.Value) : null);
        b.Property(x => x.ExpiredByUserId).HasConversion(x => x == null ? (Guid?)null : x.Value.Value, x => x.HasValue ? new UserId(x.Value) : null);
        b.Property(x => x.CreatedByUserId).HasConversion(x => x == null ? (Guid?)null : x.Value.Value, x => x.HasValue ? new UserId(x.Value) : null);
        b.Property(x => x.LastModifiedByUserId).HasConversion(x => x == null ? (Guid?)null : x.Value.Value, x => x.HasValue ? new UserId(x.Value) : null);
        b.OwnsOne(x => x.TermsSnapshot, terms => { terms.ToJson("terms_snapshot"); });
        b.OwnsMany(x => x.Parties, parties =>
        {
            parties.ToTable("training_contract_parties");
            parties.WithOwner().HasForeignKey("contract_id");
            parties.Property<Guid>("id"); parties.HasKey("id");
            parties.Property(x => x.PersonId).HasConversion(x => x == null ? (Guid?)null : x.Value.Value, x => x.HasValue ? new PersonId(x.Value) : null);
            parties.Property(x => x.OrganizationId).HasConversion(x => x == null ? (Guid?)null : x.Value.Value, x => x.HasValue ? new OrganizationId(x.Value) : null);
            parties.Property(x => x.DisplayName).HasMaxLength(250).IsRequired();
            parties.Property(x => x.LegalReference).HasMaxLength(150);
        });
        b.Navigation(x => x.Parties).UsePropertyAccessMode(PropertyAccessMode.Field);
        b.Navigation(x => x.Versions).UsePropertyAccessMode(PropertyAccessMode.Field);
        b.Navigation(x => x.Signatories).UsePropertyAccessMode(PropertyAccessMode.Field);
        b.HasMany(x => x.Versions).WithOne().HasForeignKey(x => x.ContractId).OnDelete(DeleteBehavior.Cascade);
        b.HasMany(x => x.Signatories).WithOne().HasForeignKey(x => x.ContractId).OnDelete(DeleteBehavior.Cascade);
    }
}
