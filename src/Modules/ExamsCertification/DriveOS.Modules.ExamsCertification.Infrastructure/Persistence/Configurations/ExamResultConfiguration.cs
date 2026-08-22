using DriveOS.Modules.ExamsCertification.Domain.Results;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DriveOS.Modules.ExamsCertification.Infrastructure.Persistence.Configurations;

internal sealed class ExamResultConfiguration : IEntityTypeConfiguration<ExamResult>
{
    public void Configure(EntityTypeBuilder<ExamResult> b)
    {
        b.ToTable("exam_results"); b.HasKey(x=>x.Id);
        b.Property(x=>x.Id).HasConversion(x=>x.Value,x=>new ExamResultId(x)); b.Property(x=>x.OrganizationId).HasConversion(x=>x.Value,x=>new OrganizationId(x));
        b.Property(x=>x.AttemptId).HasConversion(x=>x.Value,x=>new ExamAttemptId(x)); b.Property(x=>x.RegistrationId).HasConversion(x=>x.Value,x=>new ExamRegistrationId(x));
        b.Property(x=>x.StudentId).HasConversion(x=>x.Value,x=>new PersonId(x)); b.Property(x=>x.EvidenceDocumentId).HasConversion(x=>x.HasValue?x.Value.Value:(Guid?)null,x=>x.HasValue?new DocumentId(x.Value):null);
        b.Property(x=>x.VerifiedByUserId).HasConversion(x=>x.HasValue?x.Value.Value:(Guid?)null,x=>x.HasValue?new UserId(x.Value):null); b.Property(x=>x.FinalizedByUserId).HasConversion(x=>x.HasValue?x.Value.Value:(Guid?)null,x=>x.HasValue?new UserId(x.Value):null);
        b.Property(x=>x.CreatedByUserId).HasConversion(x=>x.HasValue?x.Value.Value:(Guid?)null,x=>x.HasValue?new UserId(x.Value):null); b.Property(x=>x.LastModifiedByUserId).HasConversion(x=>x.HasValue?x.Value.Value:(Guid?)null,x=>x.HasValue?new UserId(x.Value):null);
        b.Property(x=>x.Outcome).HasConversion<int>(); b.Property(x=>x.SourceKind).HasConversion<int>(); b.Property(x=>x.Status).HasConversion<int>(); b.Property(x=>x.Score).HasPrecision(10,2);
        b.Property(x=>x.FailureReasonCode).HasMaxLength(160); b.Property(x=>x.Comments).HasMaxLength(4000); b.Property(x=>x.ProviderCode).HasMaxLength(120).IsRequired(); b.Property(x=>x.ExternalResultId).HasMaxLength(256); b.Property(x=>x.VerificationReference).HasMaxLength(512);
        b.HasIndex(x=>new{x.OrganizationId,x.AttemptId}).IsUnique(); b.HasIndex(x=>new{x.OrganizationId,x.StudentId,x.Status}); b.HasIndex(x=>new{x.OrganizationId,x.ExternalResultId});
        b.HasMany(x=>x.Revisions).WithOne().HasForeignKey(x=>x.ResultId).OnDelete(DeleteBehavior.Cascade); b.Navigation(x=>x.Revisions).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

internal sealed class ExamResultRevisionConfiguration : IEntityTypeConfiguration<ExamResultRevision>
{
    public void Configure(EntityTypeBuilder<ExamResultRevision> b)
    {
        b.ToTable("exam_result_revisions"); b.HasKey(x=>x.Id); b.Property(x=>x.Id).HasConversion(x=>x.Value,x=>new ExamResultRevisionId(x));
        b.Property(x=>x.ResultId).HasConversion(x=>x.Value,x=>new ExamResultId(x)); b.Property(x=>x.OrganizationId).HasConversion(x=>x.Value,x=>new OrganizationId(x)); b.Property(x=>x.ActorUserId).HasConversion(x=>x.Value,x=>new UserId(x));
        b.Property(x=>x.EvidenceDocumentId).HasConversion(x=>x.HasValue?x.Value.Value:(Guid?)null,x=>x.HasValue?new DocumentId(x.Value):null); b.Property(x=>x.Outcome).HasConversion<int>(); b.Property(x=>x.SourceKind).HasConversion<int>(); b.Property(x=>x.Score).HasPrecision(10,2);
        b.Property(x=>x.FailureReasonCode).HasMaxLength(160); b.Property(x=>x.Comments).HasMaxLength(4000); b.Property(x=>x.ProviderCode).HasMaxLength(120).IsRequired(); b.Property(x=>x.ExternalResultId).HasMaxLength(256); b.Property(x=>x.CorrectionReason).HasMaxLength(1000); b.Property(x=>x.RequestFingerprint).HasMaxLength(128).IsRequired();
        b.HasIndex(x=>new{x.ResultId,x.RevisionNumber}).IsUnique(); b.HasIndex(x=>new{x.OrganizationId,x.OperationId}).IsUnique();
    }
}
