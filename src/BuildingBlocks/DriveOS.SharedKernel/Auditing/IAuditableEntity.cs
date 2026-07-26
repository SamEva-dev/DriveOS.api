using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.SharedKernel.Auditing;

public interface IAuditableEntity
{
    DateTimeOffset CreatedAtUtc { get; }

    UserId? CreatedByUserId { get; }

    DateTimeOffset? LastModifiedAtUtc { get; }

    UserId? LastModifiedByUserId { get; }

    void SetCreatedAudit(
        DateTimeOffset createdAtUtc,
        UserId? createdByUserId);

    void SetModifiedAudit(
        DateTimeOffset modifiedAtUtc,
        UserId? modifiedByUserId);
}