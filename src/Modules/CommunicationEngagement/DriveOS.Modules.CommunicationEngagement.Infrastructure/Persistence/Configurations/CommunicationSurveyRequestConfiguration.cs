using DriveOS.Modules.CommunicationEngagement.Domain.Surveys;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DriveOS.Modules.CommunicationEngagement.Infrastructure.Persistence.Configurations;

internal sealed class CommunicationSurveyRequestConfiguration:IEntityTypeConfiguration<CommunicationSurveyRequest>
{
    public void Configure(EntityTypeBuilder<CommunicationSurveyRequest>b)
    {
        b.ToTable("survey_requests");
        b.HasKey(x=>x.Id);
        b.Property(x=>x.Id).HasConversion(x=>x.Value,x=>new CommunicationSurveyRequestId(x)).ValueGeneratedNever();
        b.Property(x=>x.RecipientUserId).HasConversion(x=>x.Value,x=>new UserId(x)).IsRequired();
        b.Property(x=>x.OrganizationId).HasConversion(x=>x.Value,x=>new OrganizationId(x)).IsRequired();
        b.Property(x=>x.SurveyType).HasMaxLength(80).IsRequired();
        b.Property(x=>x.DeduplicationKey).HasMaxLength(180).IsRequired();
        b.Property(x=>x.RelatedEntityType).HasMaxLength(80).IsRequired();
        b.Property(x=>x.PayloadJson).HasColumnType("jsonb").IsRequired();
        b.Property(x=>x.Status).HasConversion<string>().HasMaxLength(24).IsRequired();
        b.HasIndex(x=>x.DeduplicationKey).IsUnique();
        b.HasIndex(x=>new{x.RecipientUserId,x.Status,x.CreatedAtUtc});
        b.Ignore(x=>x.DomainEvents);
    }
}
