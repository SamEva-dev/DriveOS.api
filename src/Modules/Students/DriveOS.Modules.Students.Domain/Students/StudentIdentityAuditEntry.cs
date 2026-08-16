using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.Students.Domain.Students;

public sealed class StudentIdentityAuditEntry
{
    private StudentIdentityAuditEntry() { }

    internal StudentIdentityAuditEntry(
        Guid id,
        OrganizationId organizationId,
        PersonId studentId,
        string action,
        string justification,
        UserId actorUserId,
        DateTimeOffset occurredAtUtc
    )
    {
        Id = id;
        OrganizationId = organizationId;
        StudentId = studentId;
        Action = action;
        Justification = justification;
        ActorUserId = actorUserId;
        OccurredAtUtc = occurredAtUtc;
    }

    public Guid Id { get; private set; }
    public OrganizationId OrganizationId { get; private set; }
    public PersonId StudentId { get; private set; }
    public string Action { get; private set; } = string.Empty;
    public string Justification { get; private set; } = string.Empty;
    public UserId ActorUserId { get; private set; }
    public DateTimeOffset OccurredAtUtc { get; private set; }
}
