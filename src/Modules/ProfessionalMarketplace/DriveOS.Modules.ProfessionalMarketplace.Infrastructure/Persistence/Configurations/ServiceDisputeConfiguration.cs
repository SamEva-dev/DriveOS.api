using System.Text.Json;
using DriveOS.Modules.ProfessionalMarketplace.Domain.Disputes;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DriveOS.Modules.ProfessionalMarketplace.Infrastructure.Persistence.Configurations;

internal sealed class ServiceDisputeConfiguration:IEntityTypeConfiguration<ServiceDispute>
{
    public void Configure(EntityTypeBuilder<ServiceDispute>b)
    {
        b.ToTable("service_disputes");
        b.HasKey(x=>x.Id);
        b.Property(x=>x.Id).HasConversion(x=>x.Value,x=>new ServiceDisputeId(x)).ValueGeneratedNever();
        b.Property(x=>x.ServiceEntryId).HasConversion(x=>x.Value,x=>new ServiceEntryId(x)).IsRequired();
        b.Property(x=>x.EngagementId).HasConversion(x=>x.Value,x=>new ProfessionalEngagementId(x)).IsRequired();
        b.Property(x=>x.ProfessionalProfileId).HasConversion(x=>x.Value,x=>new ProfessionalProfileId(x)).IsRequired();
        b.Property(x=>x.ClientOrganizationId).HasConversion(x=>x.Value,x=>new OrganizationId(x)).IsRequired();
        b.Property(x=>x.Reason).HasConversion<string>().HasMaxLength(40).IsRequired();
        b.Property(x=>x.Description).HasMaxLength(3000).IsRequired();
        b.Property(x=>x.Status).HasConversion<string>().HasMaxLength(32).IsRequired();
        b.Property(x=>x.ResolutionOutcome).HasConversion<string>().HasMaxLength(32);
        b.Property(x=>x.Resolution).HasMaxLength(3000);
        b.Property(x=>x.ResolvedByUserId).HasConversion(x=>x==null?(Guid?)null:x.Value.Value,x=>x==null?null:new UserId(x.Value));
        b.Property(x=>x.EscalatedByUserId).HasConversion(x=>x==null?(Guid?)null:x.Value.Value,x=>x==null?null:new UserId(x.Value));
        b.Property(x=>x.EscalationReason).HasMaxLength(1000);

        var evComparer=new ValueComparer<ServiceDisputeEvidence[]>(
            (a,c)=>JsonSerializer.Serialize(a,(JsonSerializerOptions?)null)==JsonSerializer.Serialize(c,(JsonSerializerOptions?)null),
            v=>JsonSerializer.Serialize(v,(JsonSerializerOptions?)null).GetHashCode(),
            v=>JsonSerializer.Deserialize<ServiceDisputeEvidence[]>(JsonSerializer.Serialize(v,(JsonSerializerOptions?)null),(JsonSerializerOptions?)null)??Array.Empty<ServiceDisputeEvidence>());
        b.Property(x=>x.Evidence).HasConversion(
            v=>JsonSerializer.Serialize(v,(JsonSerializerOptions?)null),
            v=>JsonSerializer.Deserialize<ServiceDisputeEvidence[]>(v,(JsonSerializerOptions?)null)??Array.Empty<ServiceDisputeEvidence>())
            .HasColumnType("jsonb").Metadata.SetValueComparer(evComparer);

        var msgComparer=new ValueComparer<ServiceDisputeMessage[]>(
            (a,c)=>JsonSerializer.Serialize(a,(JsonSerializerOptions?)null)==JsonSerializer.Serialize(c,(JsonSerializerOptions?)null),
            v=>JsonSerializer.Serialize(v,(JsonSerializerOptions?)null).GetHashCode(),
            v=>JsonSerializer.Deserialize<ServiceDisputeMessage[]>(JsonSerializer.Serialize(v,(JsonSerializerOptions?)null),(JsonSerializerOptions?)null)??Array.Empty<ServiceDisputeMessage>());
        b.Property(x=>x.Discussion).HasConversion(
            v=>JsonSerializer.Serialize(v,(JsonSerializerOptions?)null),
            v=>JsonSerializer.Deserialize<ServiceDisputeMessage[]>(v,(JsonSerializerOptions?)null)??Array.Empty<ServiceDisputeMessage>())
            .HasColumnType("jsonb").Metadata.SetValueComparer(msgComparer);

        b.HasIndex(x=>new{x.ServiceEntryId,x.Status});
        b.HasIndex(x=>new{x.ClientOrganizationId,x.Status,x.CreatedAtUtc});
        b.HasIndex(x=>new{x.ProfessionalProfileId,x.Status,x.CreatedAtUtc});
        b.Ignore(x=>x.IsClosed);
        b.Ignore(x=>x.DomainEvents);
    }
}
