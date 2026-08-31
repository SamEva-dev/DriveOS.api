using DriveOS.Modules.ProfessionalMarketplace.Domain.Proposals;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Text.Json;

namespace DriveOS.Modules.ProfessionalMarketplace.Infrastructure.Persistence.Configurations;
internal sealed class ProfessionalProposalConfiguration:IEntityTypeConfiguration<ProfessionalProposal>
{
    public void Configure(EntityTypeBuilder<ProfessionalProposal>b)
    {
        b.ToTable("professional_proposals");b.HasKey(x=>x.Id);
        b.Property(x=>x.Id).HasConversion(x=>x.Value,x=>new ProfessionalProposalId(x)).ValueGeneratedNever();
        b.Property(x=>x.OrganizationId).HasConversion(x=>x.Value,x=>new OrganizationId(x)).IsRequired();
        b.Property(x=>x.BranchId).HasConversion(x=>x==null?(Guid?)null:x.Value.Value,x=>x==null?null:new BranchId(x.Value));
        b.Property(x=>x.ProfessionalProfileId).HasConversion(x=>x.Value,x=>new ProfessionalProfileId(x)).IsRequired();
        b.Property(x=>x.OpportunityId).HasConversion(x=>x==null?(Guid?)null:x.Value.Value,x=>x==null?null:new ProfessionalOpportunityId(x.Value));
        b.Property(x=>x.Subject).HasMaxLength(180).IsRequired();
        b.Property(x=>x.Message).HasMaxLength(3000).IsRequired();
        b.Property(x=>x.TeachingCategoryCodes).HasColumnType("text[]").IsRequired();
        b.Property(x=>x.EngagementType).HasConversion<string>().HasMaxLength(48).IsRequired();
        b.Property(x=>x.VehicleProvisionMode).HasConversion<string>().HasMaxLength(32).IsRequired();
        b.Property(x=>x.ProposedRate).HasPrecision(18,2);b.Property(x=>x.Currency).HasMaxLength(3);b.Property(x=>x.RateUnit).HasConversion<string>().HasMaxLength(24);
        b.Property(x=>x.Status).HasConversion<string>().HasMaxLength(24).IsRequired();b.Property(x=>x.DecisionReason).HasMaxLength(512);
        var historyComparer=new ValueComparer<ProfessionalProposalRevisionSnapshot[]>(
            (a,c)=>JsonSerializer.Serialize(a,(JsonSerializerOptions?)null)==JsonSerializer.Serialize(c,(JsonSerializerOptions?)null),
            v=>JsonSerializer.Serialize(v,(JsonSerializerOptions?)null).GetHashCode(),
            v=>JsonSerializer.Deserialize<ProfessionalProposalRevisionSnapshot[]>(JsonSerializer.Serialize(v,(JsonSerializerOptions?)null),(JsonSerializerOptions?)null)?? Array.Empty<ProfessionalProposalRevisionSnapshot>());
        b.Property(x=>x.RevisionHistory).HasConversion(
            v=>JsonSerializer.Serialize(v,(JsonSerializerOptions?)null),
            v=>JsonSerializer.Deserialize<ProfessionalProposalRevisionSnapshot[]>(v,(JsonSerializerOptions?)null)?? Array.Empty<ProfessionalProposalRevisionSnapshot>())
            .HasColumnType("jsonb").Metadata.SetValueComparer(historyComparer);
        b.HasIndex(x=>new{x.OrganizationId,x.ProfessionalProfileId,x.Status});
        b.HasIndex(x=>new{x.ProfessionalProfileId,x.Status});
        b.Ignore(x=>x.DomainEvents);
    }
}
