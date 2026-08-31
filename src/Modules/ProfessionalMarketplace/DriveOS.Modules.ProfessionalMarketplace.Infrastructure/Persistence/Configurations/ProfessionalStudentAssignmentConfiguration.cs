using DriveOS.Modules.ProfessionalMarketplace.Domain.StudentAssignments;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DriveOS.Modules.ProfessionalMarketplace.Infrastructure.Persistence.Configurations;

internal sealed class ProfessionalStudentAssignmentConfiguration
    :IEntityTypeConfiguration<ProfessionalStudentAssignment>
{
    public void Configure(EntityTypeBuilder<ProfessionalStudentAssignment>b)
    {
        b.ToTable("professional_student_assignments");
        b.HasKey(x=>x.Id);
        b.Property(x=>x.Id).HasConversion(x=>x.Value,x=>new ProfessionalStudentAssignmentId(x)).ValueGeneratedNever();
        b.Property(x=>x.MissionId).HasConversion(x=>x.Value,x=>new ProfessionalMissionId(x)).IsRequired();
        b.Property(x=>x.EngagementId).HasConversion(x=>x.Value,x=>new ProfessionalEngagementId(x)).IsRequired();
        b.Property(x=>x.ProfessionalProfileId).HasConversion(x=>x.Value,x=>new ProfessionalProfileId(x)).IsRequired();
        b.Property(x=>x.OrganizationId).HasConversion(x=>x.Value,x=>new OrganizationId(x)).IsRequired();
        b.Property(x=>x.StudentId).HasConversion(x=>x.Value,x=>new PersonId(x)).IsRequired();
        b.Property(x=>x.ScopeCode).HasMaxLength(80).IsRequired();
        b.Property(x=>x.ResponsibleUserId).HasConversion(x=>x.Value,x=>new UserId(x)).IsRequired();
        b.Property(x=>x.AssignmentReason).HasMaxLength(512).IsRequired();
        b.Property(x=>x.Status).HasConversion<string>().HasMaxLength(24).IsRequired();
        b.Property(x=>x.RevokedByUserId).HasConversion(
            x=>x==null?(Guid?)null:x.Value.Value,
            x=>x==null?null:new UserId(x.Value));
        b.Property(x=>x.RevocationReason).HasMaxLength(512);
        b.HasIndex(x=>new{x.MissionId,x.StudentId,x.Status});
        b.HasIndex(x=>new{x.ProfessionalProfileId,x.Status,x.EndsOn});
        b.Ignore(x=>x.DomainEvents);
    }
}
