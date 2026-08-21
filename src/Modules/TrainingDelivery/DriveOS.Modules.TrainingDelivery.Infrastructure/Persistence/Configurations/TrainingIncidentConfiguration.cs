using DriveOS.Modules.TrainingDelivery.Domain.Incidents;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DriveOS.Modules.TrainingDelivery.Infrastructure.Persistence.Configurations;

internal sealed class TrainingIncidentConfiguration : IEntityTypeConfiguration<TrainingIncident>
{
    public void Configure(EntityTypeBuilder<TrainingIncident> b)
    {
        b.ToTable("training_incidents"); b.HasKey(x=>x.Id);
        b.Property(x=>x.Id).HasConversion(x=>x.Value,x=>new TrainingIncidentId(x));
        b.Property(x=>x.OrganizationId).HasConversion(x=>x.Value,x=>new OrganizationId(x)).IsRequired();
        b.Property(x=>x.TrainingSessionId).HasConversion(x=>x.Value,x=>new TrainingSessionId(x)).IsRequired();
        b.Property(x=>x.StudentId).HasConversion(x=>x.Value,x=>new PersonId(x)).IsRequired();
        b.Property(x=>x.InstructorId).HasConversion(x=>x.Value,x=>new UserId(x)).IsRequired();
        b.Property(x=>x.BranchId).HasConversion(x=>x.HasValue?x.Value.Value:(Guid?)null,x=>x.HasValue?new BranchId(x.Value):null);
        b.Property(x=>x.PerformingOrganizationId).HasConversion(x=>x.Value,x=>new OrganizationId(x)).IsRequired();
        b.Property(x=>x.Description).HasMaxLength(5000).IsRequired(); b.Property(x=>x.ImmediateActions).HasMaxLength(3000).IsRequired();
        b.Property(x=>x.Resolution).HasMaxLength(4000); b.Property(x=>x.ReportRequestFingerprint).HasMaxLength(64).IsRequired();
        b.Property(x=>x.EscalatedByUserId).HasConversion(x=>x.HasValue?x.Value.Value:(Guid?)null,x=>x.HasValue?new UserId(x.Value):null);
        b.Property(x=>x.ResolvedByUserId).HasConversion(x=>x.HasValue?x.Value.Value:(Guid?)null,x=>x.HasValue?new UserId(x.Value):null);
        b.Property(x=>x.ClosedByUserId).HasConversion(x=>x.HasValue?x.Value.Value:(Guid?)null,x=>x.HasValue?new UserId(x.Value):null);
        b.Property(x=>x.CreatedByUserId).HasConversion(x=>x.HasValue?x.Value.Value:(Guid?)null,x=>x.HasValue?new UserId(x.Value):null);
        b.Property(x=>x.LastModifiedByUserId).HasConversion(x=>x.HasValue?x.Value.Value:(Guid?)null,x=>x.HasValue?new UserId(x.Value):null);
        b.HasMany(x=>x.Participants).WithOne().HasForeignKey(x=>x.TrainingIncidentId).OnDelete(DeleteBehavior.Cascade);
        b.HasMany(x=>x.Evidence).WithOne().HasForeignKey(x=>x.TrainingIncidentId).OnDelete(DeleteBehavior.Cascade);
        b.HasMany(x=>x.History).WithOne().HasForeignKey(x=>x.TrainingIncidentId).OnDelete(DeleteBehavior.Cascade);
        b.HasIndex(x=>new{x.OrganizationId,x.TrainingSessionId,x.ReportOperationId}).IsUnique();
        b.HasIndex(x=>new{x.OrganizationId,x.Status,x.Severity});
        b.HasIndex(x=>new{x.OrganizationId,x.OccurredAtUtc});
    }
}
