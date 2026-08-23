using DriveOS.Modules.Workforce.Domain.Events;
using DriveOS.SharedKernel.Auditing;
using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;
namespace DriveOS.Modules.Workforce.Domain.JobPositions;

/// <summary>
/// Organization-defined job position. The label/code describe employment organization; ProfessionalFunction provides a stable
/// business classification. A JobPosition never grants application permissions and never proves a regulatory qualification.
/// </summary>
public sealed class JobPosition : AggregateRoot<JobPositionId>, IAuditableEntity
{
    private JobPosition() { }
    private JobPosition(JobPositionId id, OrganizationId organizationId, string code, string name, string? description, ProfessionalFunction professionalFunction, DateTimeOffset nowUtc) : base(id)
    {
        OrganizationId = organizationId;
        Code = NormalizeCode(code);
        Name = name.Trim();
        Description = NormalizeOptional(description);
        ProfessionalFunction = professionalFunction;
        Status = JobPositionStatus.Active;
        RaiseDomainEvent(new JobPositionCreatedDomainEvent(Guid.NewGuid(), nowUtc.ToUniversalTime(), Id, OrganizationId, Code, ProfessionalFunction));
    }

    public OrganizationId OrganizationId { get; private set; }
    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public ProfessionalFunction ProfessionalFunction { get; private set; }
    public JobPositionStatus Status { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public UserId? CreatedByUserId { get; private set; }
    public DateTimeOffset? LastModifiedAtUtc { get; private set; }
    public UserId? LastModifiedByUserId { get; private set; }

    public static Result<JobPosition> Create(JobPositionId id, OrganizationId organizationId, string code, string name, string? description, ProfessionalFunction professionalFunction, DateTimeOffset nowUtc)
    {
        if (id.IsEmpty) return Result.Failure<JobPosition>(JobPositionErrors.InvalidIdentifier);
        if (organizationId.IsEmpty) return Result.Failure<JobPosition>(JobPositionErrors.InvalidOrganization);
        Error? validation = Validate(code, name);
        return validation is not null ? Result.Failure<JobPosition>(validation) : Result.Success(new JobPosition(id, organizationId, code, name, description, professionalFunction, nowUtc));
    }

    public Result Update(string code, string name, string? description, ProfessionalFunction professionalFunction, DateTimeOffset nowUtc, UserId actorUserId)
    {
        Error? validation = Validate(code, name);
        if (validation is not null) return Result.Failure(validation);
        Code = NormalizeCode(code); Name = name.Trim(); Description = NormalizeOptional(description); ProfessionalFunction = professionalFunction;
        SetModifiedAudit(nowUtc, actorUserId);
        RaiseDomainEvent(new JobPositionUpdatedDomainEvent(Guid.NewGuid(), nowUtc.ToUniversalTime(), Id, OrganizationId, Code, ProfessionalFunction, actorUserId));
        return Result.Success();
    }

    public Result Deactivate(DateTimeOffset nowUtc, UserId actorUserId)
    {
        if (Status == JobPositionStatus.Inactive) return Result.Failure(JobPositionErrors.AlreadyInactive);
        Status = JobPositionStatus.Inactive; SetModifiedAudit(nowUtc, actorUserId);
        RaiseDomainEvent(new JobPositionDeactivatedDomainEvent(Guid.NewGuid(), nowUtc.ToUniversalTime(), Id, OrganizationId, actorUserId));
        return Result.Success();
    }

    public Result Reactivate(DateTimeOffset nowUtc, UserId actorUserId)
    {
        if (Status == JobPositionStatus.Active) return Result.Failure(JobPositionErrors.AlreadyActive);
        Status = JobPositionStatus.Active; SetModifiedAudit(nowUtc, actorUserId);
        RaiseDomainEvent(new JobPositionReactivatedDomainEvent(Guid.NewGuid(), nowUtc.ToUniversalTime(), Id, OrganizationId, actorUserId));
        return Result.Success();
    }

    private static Error? Validate(string code, string name)
    {
        if (string.IsNullOrWhiteSpace(code)) return JobPositionErrors.CodeRequired;
        if (code.Trim().Length > 64) return JobPositionErrors.CodeTooLong;
        if (string.IsNullOrWhiteSpace(name)) return JobPositionErrors.NameRequired;
        if (name.Trim().Length > 160) return JobPositionErrors.NameTooLong;
        return null;
    }
    public void SetCreatedAudit(DateTimeOffset createdAtUtc, UserId? createdByUserId) { if (CreatedAtUtc != default) return; CreatedAtUtc = createdAtUtc.ToUniversalTime(); CreatedByUserId = createdByUserId; }
    public void SetModifiedAudit(DateTimeOffset modifiedAtUtc, UserId? modifiedByUserId) { LastModifiedAtUtc = modifiedAtUtc.ToUniversalTime(); LastModifiedByUserId = modifiedByUserId; }

    private static string NormalizeCode(string value) => value.Trim().ToUpperInvariant();
    private static string? NormalizeOptional(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
