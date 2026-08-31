using DriveOS.Modules.ProfessionalMarketplace.Domain.ServiceStatements;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Text.Json;

namespace DriveOS.Modules.ProfessionalMarketplace.Infrastructure.Persistence.Configurations;
internal sealed class ServiceStatementConfiguration:IEntityTypeConfiguration<ServiceStatement>
{
    public void Configure(EntityTypeBuilder<ServiceStatement>b)
    {
        b.ToTable("service_statements");b.HasKey(x=>x.Id);
        b.Property(x=>x.Id).HasConversion(x=>x.Value,x=>new ServiceStatementId(x)).ValueGeneratedNever();
        b.Property(x=>x.EngagementId).HasConversion(x=>x.Value,x=>new ProfessionalEngagementId(x)).IsRequired();
        b.Property(x=>x.ProfessionalProfileId).HasConversion(x=>x.Value,x=>new ProfessionalProfileId(x)).IsRequired();
        b.Property(x=>x.ClientOrganizationId).HasConversion(x=>x.Value,x=>new OrganizationId(x)).IsRequired();
        b.Property(x=>x.Currency).HasMaxLength(3).IsRequired();
        b.Property(x=>x.Status).HasConversion<string>().HasMaxLength(32).IsRequired();
        b.Property(x=>x.ReviewedByUserId).HasConversion(x=>x==null?(Guid?)null:x.Value.Value,x=>x==null?null:new UserId(x.Value));
        b.Property(x=>x.RejectionReason).HasMaxLength(512);
        var comparer=new ValueComparer<ServiceStatementLine[]>(
            (a,c)=>JsonSerializer.Serialize(a,(JsonSerializerOptions?)null)==JsonSerializer.Serialize(c,(JsonSerializerOptions?)null),
            v=>JsonSerializer.Serialize(v,(JsonSerializerOptions?)null).GetHashCode(),
            v=>JsonSerializer.Deserialize<ServiceStatementLine[]>(JsonSerializer.Serialize(v,(JsonSerializerOptions?)null),(JsonSerializerOptions?)null)??Array.Empty<ServiceStatementLine>());
        b.Property(x=>x.Lines).HasConversion(
            v=>JsonSerializer.Serialize(v,(JsonSerializerOptions?)null),
            v=>JsonSerializer.Deserialize<ServiceStatementLine[]>(v,(JsonSerializerOptions?)null)??Array.Empty<ServiceStatementLine>())
            .HasColumnType("jsonb").Metadata.SetValueComparer(comparer);
        b.HasIndex(x=>new{x.EngagementId,x.PeriodStart,x.PeriodEnd}).IsUnique();
        b.HasIndex(x=>new{x.ClientOrganizationId,x.Status,x.PeriodEnd});
        b.HasIndex(x=>new{x.ProfessionalProfileId,x.Status,x.PeriodEnd});
        b.Ignore(x=>x.TotalAmount);b.Ignore(x=>x.ApprovedAmount);b.Ignore(x=>x.DisputedAmount);b.Ignore(x=>x.DomainEvents);
    }
}
