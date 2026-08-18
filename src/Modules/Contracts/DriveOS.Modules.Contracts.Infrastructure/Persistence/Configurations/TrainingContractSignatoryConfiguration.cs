using DriveOS.Modules.Contracts.Domain.TrainingContracts;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DriveOS.Modules.Contracts.Infrastructure.Persistence.Configurations;

internal sealed class TrainingContractSignatoryConfiguration : IEntityTypeConfiguration<TrainingContractSignatory>
{
    public void Configure(EntityTypeBuilder<TrainingContractSignatory> b)
    {
        b.ToTable("training_contract_signatories");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasConversion(x => x.Value, x => new TrainingContractSignatoryId(x));
        b.Property(x => x.ContractId).HasConversion(x => x.Value, x => new TrainingContractId(x));
        b.Property(x => x.PersonId).HasConversion(x => x.Value, x => new PersonId(x));
        b.Property(x => x.RepresentedOrganizationId).HasConversion(x => x == null ? (Guid?)null : x.Value.Value, x => x.HasValue ? new OrganizationId(x.Value) : null);
        b.Property(x => x.DisplayName).HasMaxLength(250).IsRequired();
        b.Property(x => x.AuthorityReference).HasMaxLength(250);
        b.Property(x => x.AuthorityRejectionReason).HasMaxLength(500);
        b.Property(x => x.AuthorityVerifiedByUserId).HasConversion(x => x == null ? (Guid?)null : x.Value.Value, x => x.HasValue ? new UserId(x.Value) : null);
        b.HasIndex(x => new { x.ContractId, x.SigningOrder });
        b.HasIndex(x => new { x.ContractId, x.PersonId, x.Kind }).IsUnique();
    }
}
