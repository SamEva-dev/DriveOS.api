using System.Text.Json;
using DriveOS.Modules.Contracts.Domain.ProfessionalServiceContracts;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DriveOS.Modules.Contracts.Infrastructure.Persistence.Configurations;

internal sealed class ProfessionalServiceContractConfiguration:IEntityTypeConfiguration<ProfessionalServiceContract>
{
    public void Configure(EntityTypeBuilder<ProfessionalServiceContract>b)
    {
        b.ToTable("professional_service_contracts");
        b.HasKey(x=>x.Id);
        b.Property(x=>x.Id).HasConversion(x=>x.Value,x=>new ProfessionalServiceContractId(x)).ValueGeneratedNever();
        b.Property(x=>x.OrganizationId).HasConversion(x=>x.Value,x=>new OrganizationId(x)).IsRequired();
        b.Property(x=>x.EngagementId).HasConversion(x=>x.Value,x=>new ProfessionalEngagementId(x)).IsRequired();
        b.Property(x=>x.ProfessionalProfileId).HasConversion(x=>x.Value,x=>new ProfessionalProfileId(x)).IsRequired();
        b.Property(x=>x.ContractNumber).HasMaxLength(120).IsRequired();
        b.Property(x=>x.ContractType).HasMaxLength(80).IsRequired();
        b.Property(x=>x.SignatureOrder).HasConversion<string>().HasMaxLength(24).IsRequired();
        b.Property(x=>x.TermsSnapshotJson).HasColumnType("jsonb").IsRequired();
        b.Property(x=>x.Status).HasConversion<string>().HasMaxLength(32).IsRequired();
        b.Property(x=>x.DocumentReference).HasMaxLength(500);
        b.Property(x=>x.DocumentSha256).HasMaxLength(64);
        b.Property(x=>x.TerminationReason).HasMaxLength(512);

        var comparer=new ValueComparer<ProfessionalServiceContractSignatory[]>(
            (a,c)=>JsonSerializer.Serialize(a,(JsonSerializerOptions?)null)==JsonSerializer.Serialize(c,(JsonSerializerOptions?)null),
            v=>JsonSerializer.Serialize(v,(JsonSerializerOptions?)null).GetHashCode(),
            v=>JsonSerializer.Deserialize<ProfessionalServiceContractSignatory[]>(
                JsonSerializer.Serialize(v,(JsonSerializerOptions?)null),(JsonSerializerOptions?)null)??Array.Empty<ProfessionalServiceContractSignatory>());

        b.Property(x=>x.Signatories).HasConversion(
            v=>JsonSerializer.Serialize(v,(JsonSerializerOptions?)null),
            v=>JsonSerializer.Deserialize<ProfessionalServiceContractSignatory[]>(v,(JsonSerializerOptions?)null)??Array.Empty<ProfessionalServiceContractSignatory>())
            .HasColumnType("jsonb")
            .Metadata.SetValueComparer(comparer);

        var versionsComparer=new ValueComparer<ProfessionalServiceContractVersionSnapshot[]>(
            (a,c)=>JsonSerializer.Serialize(a,(JsonSerializerOptions?)null)==JsonSerializer.Serialize(c,(JsonSerializerOptions?)null),
            v=>JsonSerializer.Serialize(v,(JsonSerializerOptions?)null).GetHashCode(),
            v=>JsonSerializer.Deserialize<ProfessionalServiceContractVersionSnapshot[]>(
                JsonSerializer.Serialize(v,(JsonSerializerOptions?)null),(JsonSerializerOptions?)null)??Array.Empty<ProfessionalServiceContractVersionSnapshot>());

        b.Property(x=>x.PreviousVersions).HasConversion(
            v=>JsonSerializer.Serialize(v,(JsonSerializerOptions?)null),
            v=>JsonSerializer.Deserialize<ProfessionalServiceContractVersionSnapshot[]>(v,(JsonSerializerOptions?)null)??Array.Empty<ProfessionalServiceContractVersionSnapshot>())
            .HasColumnType("jsonb")
            .Metadata.SetValueComparer(versionsComparer);

        b.HasIndex(x=>x.EngagementId).IsUnique();
        b.HasIndex(x=>new{x.OrganizationId,x.Status});
        b.Ignore(x=>x.DomainEvents);
    }
}
