using DriveOS.Modules.Contracts.Domain.TrainingContracts;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace DriveOS.Modules.Contracts.Infrastructure.Persistence.Configurations;
internal sealed class TrainingContractVersionConfiguration : IEntityTypeConfiguration<TrainingContractVersion>
{
    public void Configure(EntityTypeBuilder<TrainingContractVersion> b)
    {
        b.ToTable("training_contract_versions"); b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasConversion(x => x.Value, x => new TrainingContractVersionId(x));
        b.Property(x => x.ContractId).HasConversion(x => x.Value, x => new TrainingContractId(x));
        b.HasIndex(x => new { x.ContractId, x.VersionNumber }).IsUnique();
        b.Property(x => x.SourceOfferId).HasConversion(x => x.Value, x => new CommercialOfferId(x));
        b.Property(x => x.Currency).HasMaxLength(3).IsRequired(); b.Property(x => x.TotalAmount).HasPrecision(18, 2);
        b.Property(x => x.RevisionReason).HasMaxLength(500);
        b.Property(x => x.CreatedByUserId).HasConversion(x => x == null ? (Guid?)null : x.Value.Value, x => x.HasValue ? new UserId(x.Value) : null);
        b.OwnsOne(x => x.TermsSnapshot, terms => { terms.ToJson("terms_snapshot"); });
        b.OwnsMany(x => x.Parties, parties =>
        {
            parties.ToTable("training_contract_version_parties"); parties.WithOwner().HasForeignKey("contract_version_id");
            parties.Property<Guid>("id"); parties.HasKey("id");
            parties.Property(x => x.PersonId).HasConversion(x => x == null ? (Guid?)null : x.Value.Value, x => x.HasValue ? new PersonId(x.Value) : null);
            parties.Property(x => x.OrganizationId).HasConversion(x => x == null ? (Guid?)null : x.Value.Value, x => x.HasValue ? new OrganizationId(x.Value) : null);
            parties.Property(x => x.DisplayName).HasMaxLength(250).IsRequired(); parties.Property(x => x.LegalReference).HasMaxLength(150);
        });
        b.Navigation(x => x.Parties).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
