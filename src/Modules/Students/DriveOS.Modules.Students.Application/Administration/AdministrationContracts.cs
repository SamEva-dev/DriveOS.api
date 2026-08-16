using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Modules.Students.Domain.Administration;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Students.Application.Administration;

public sealed record AdministrationResponse(
    Guid StudentId,
    AdministrativeStatus Status,
    int ValidatedRequirements,
    int TotalRequirements,
    IReadOnlyList<RequirementItem> Requirements,
    IReadOnlyList<BlockItem> ActiveBlocks,
    IReadOnlyList<ExceptionItem> Exceptions,
    IReadOnlyList<AdministrationHistoryItem> History
);

public sealed record RequirementItem(
    Guid Id,
    string Code,
    string LabelKey,
    bool IsBlocking,
    AdministrativeRequirementStatus Status,
    DateTimeOffset? DueAtUtc,
    string PolicySource,
    string? DecisionReason
);

public sealed record BlockItem(Guid Id, string Code, string Reason, DateTimeOffset AppliedAtUtc);

public sealed record ExceptionItem(
    Guid Id,
    Guid RequirementId,
    string RequestReason,
    ComplianceExceptionStatus Status,
    string? DecisionReason,
    DateTimeOffset RequestedAtUtc
);

public sealed record AdministrationHistoryItem(
    string Action,
    Guid SubjectId,
    DateTimeOffset OccurredAtUtc
);

public sealed record GetAdministrationQuery(OrganizationId OrganizationId, PersonId StudentId)
    : IQuery<AdministrationResponse>;

public sealed record ConfigureRequirementCommand(
    OrganizationId OrganizationId,
    PersonId StudentId,
    Guid? RequirementId,
    string Code,
    string LabelKey,
    bool IsBlocking,
    DateTimeOffset? DueAtUtc,
    string PolicySource,
    UserId ActorUserId
) : ICommand<Guid>;

public sealed record DecideRequirementCommand(
    OrganizationId OrganizationId,
    PersonId StudentId,
    Guid RequirementId,
    AdministrativeRequirementStatus Status,
    string Reason,
    UserId ActorUserId
) : ICommand;

public sealed record AddAdministrativeBlockCommand(
    OrganizationId OrganizationId,
    PersonId StudentId,
    string Code,
    string Reason,
    UserId ActorUserId
) : ICommand<Guid>;

public sealed record ReleaseAdministrativeBlockCommand(
    OrganizationId OrganizationId,
    PersonId StudentId,
    Guid BlockId,
    string Reason,
    UserId ActorUserId
) : ICommand;

public sealed record RequestComplianceExceptionCommand(
    OrganizationId OrganizationId,
    PersonId StudentId,
    Guid RequirementId,
    string Reason,
    UserId ActorUserId
) : ICommand<Guid>;

public sealed record DecideComplianceExceptionCommand(
    OrganizationId OrganizationId,
    PersonId StudentId,
    Guid ExceptionId,
    bool Approve,
    string Reason,
    UserId ActorUserId
) : ICommand;

public sealed record SynchronizeAdministrativeRequirementsCommand(
    OrganizationId OrganizationId,
    PersonId StudentId,
    UserId ActorUserId
) : ICommand<int>;

public interface IAdministrationService
{
    Task<AdministrationResponse?> GetAsync(
        OrganizationId organizationId,
        PersonId studentId,
        CancellationToken ct = default
    );
    Task<Result<Guid>> ConfigureAsync(
        ConfigureRequirementCommand command,
        CancellationToken ct = default
    );
    Task<Result> DecideRequirementAsync(
        DecideRequirementCommand command,
        CancellationToken ct = default
    );
    Task<Result<Guid>> AddBlockAsync(
        AddAdministrativeBlockCommand command,
        CancellationToken ct = default
    );
    Task<Result> ReleaseBlockAsync(
        ReleaseAdministrativeBlockCommand command,
        CancellationToken ct = default
    );
    Task<Result<Guid>> RequestExceptionAsync(
        RequestComplianceExceptionCommand command,
        CancellationToken ct = default
    );
    Task<Result> DecideExceptionAsync(
        DecideComplianceExceptionCommand command,
        CancellationToken ct = default
    );
    Task<Result<int>> SynchronizeRequirementsAsync(
        SynchronizeAdministrativeRequirementsCommand command,
        CancellationToken ct = default
    );
}

public static class AdministrationApplicationErrors
{
    public static readonly Error StudentNotFound = Error.NotFound(
        "Students.Administration.Student.NotFound",
        "errors.students.administration.student.notFound"
    );
}
