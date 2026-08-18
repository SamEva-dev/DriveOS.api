using DriveOS.Modules.Contracts.Domain.SignatureProcesses;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DriveOS.Modules.Contracts.Infrastructure.Persistence.Configurations;

public sealed class SignatureProcessConfiguration : IEntityTypeConfiguration<SignatureProcess>
{
    public void Configure(EntityTypeBuilder<SignatureProcess> builder)
    {
        builder.ToTable("signature_processes");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasConversion(x => x.Value, x => new SignatureProcessId(x));
        builder.Property(x => x.OrganizationId).HasConversion(x => x.Value, x => new OrganizationId(x)).IsRequired();
        builder.Property(x => x.ContractId).HasConversion(x => x.Value, x => new TrainingContractId(x)).IsRequired();
        builder.Property(x => x.DocumentReference).HasMaxLength(1000).IsRequired();
        builder.Property(x => x.DocumentSha256).HasMaxLength(64).IsRequired();
        builder.Property(x => x.SignatureOrder).HasMaxLength(32).IsRequired();
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(x => x.RequestedByUserId).HasConversion(x => x.Value, x => new UserId(x)).IsRequired();
        builder.HasIndex(x => new { x.OrganizationId, x.ContractId, x.ContractVersionNumber }).IsUnique();

        builder.OwnsMany(x => x.Recipients, recipients =>
        {
            recipients.ToTable("signature_process_recipients");
            recipients.WithOwner().HasForeignKey("signature_process_id");
            recipients.Property<Guid>("id");
            recipients.HasKey("id");
            recipients.Property(x => x.SignatoryId).HasConversion(x => x.Value, x => new TrainingContractSignatoryId(x));
            recipients.Property(x => x.PersonId).HasConversion(x => x.Value, x => new PersonId(x));
            recipients.Property(x => x.RepresentedOrganizationId)
                .HasConversion(x => x.HasValue ? x.Value.Value : (Guid?)null, x => x.HasValue ? new OrganizationId(x.Value) : null);
            recipients.Property(x => x.Kind).HasMaxLength(50);
            recipients.Property(x => x.DisplayName).HasMaxLength(250);
        });

        builder.OwnsMany(x => x.Evidence, evidence =>
        {
            evidence.ToTable("signature_evidence");
            evidence.WithOwner().HasForeignKey(nameof(SignatureEvidence.SignatureProcessId));
            evidence.HasKey(x => x.Id);
            evidence.Property(x => x.Id).HasConversion(x => x.Value, x => new SignatureEvidenceId(x));
            evidence.Property(x => x.SignatureProcessId).HasConversion(x => x.Value, x => new SignatureProcessId(x));
            evidence.Property(x => x.SignatoryId).HasConversion(x => x.Value, x => new TrainingContractSignatoryId(x));
            evidence.Property(x => x.PersonId).HasConversion(x => x.Value, x => new PersonId(x));
            evidence.Property(x => x.DocumentSha256).HasMaxLength(64).IsRequired();
            evidence.Property(x => x.SignatureMethod).HasMaxLength(80).IsRequired();
            evidence.Property(x => x.AuthenticationMethod).HasMaxLength(120).IsRequired();
            evidence.Property(x => x.Provider).HasMaxLength(120).IsRequired();
            evidence.Property(x => x.ProviderSignatureReference).HasMaxLength(250).IsRequired();
            evidence.Property(x => x.CertificateReference).HasMaxLength(500);
            evidence.Property(x => x.IpAddress).HasMaxLength(64);
            evidence.Property(x => x.UserAgent).HasMaxLength(1000);
            evidence.Property(x => x.RecordedByUserId).HasConversion(x => x.Value, x => new UserId(x)).IsRequired();
            evidence.HasIndex(x => new { x.Provider, x.ProviderSignatureReference }).IsUnique();
            evidence.HasIndex(x => x.SignatoryId).IsUnique();
        });

        builder.Navigation(x => x.Recipients).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Navigation(x => x.Evidence).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
