using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Modules.Students.Domain.Suspensions;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Students.Application.Reactivations;

public sealed record EnrollmentReactivationResponse(
    Guid ReactivationId,
    Guid SuspensionId,
    Guid EnrollmentId,
    EnrollmentReactivationMode Mode,
    DateOnly ResumeDate,
    string Conditions,
    bool PedagogyReviewRequested,
    EnrollmentReactivationStatus Status,
    DateTimeOffset? AppliedAtUtc,
    IReadOnlyList<EnrollmentReactivationCheckItem> Checks
);

public sealed record EnrollmentReactivationCheckItem(
    ReactivationCheckType Type,
    ReactivationCheckStatus Status,
    string Detail
);

public sealed record GetEnrollmentReactivationsQuery(
    OrganizationId OrganizationId,
    PersonId StudentId
) : IQuery<IReadOnlyList<EnrollmentReactivationResponse>>;

public sealed record CreateEnrollmentReactivationCommand(
    OrganizationId OrganizationId,
    PersonId StudentId,
    Guid SuspensionId,
    EnrollmentReactivationMode Mode,
    DateOnly ResumeDate,
    string Conditions,
    bool PedagogyReviewRequested,
    IReadOnlyList<EnrollmentReactivationCheckSeed> Checks,
    UserId ActorUserId
) : ICommand<Guid>;

public sealed record ReviewEnrollmentReactivationCheckCommand(
    OrganizationId OrganizationId,
    PersonId StudentId,
    Guid ReactivationId,
    ReactivationCheckType Type,
    ReactivationCheckStatus Status,
    string Detail,
    UserId ActorUserId
) : ICommand;

public sealed record ApplyEnrollmentReactivationCommand(
    OrganizationId OrganizationId,
    PersonId StudentId,
    Guid ReactivationId,
    UserId ActorUserId
) : ICommand;

public interface IEnrollmentReactivationService
{
    Task<IReadOnlyList<EnrollmentReactivationResponse>> GetAsync(
        GetEnrollmentReactivationsQuery query,
        CancellationToken ct = default
    );
    Task<Result<Guid>> CreateAsync(
        CreateEnrollmentReactivationCommand command,
        CancellationToken ct = default
    );
    Task<Result> ReviewCheckAsync(
        ReviewEnrollmentReactivationCheckCommand command,
        CancellationToken ct = default
    );
    Task<Result> ApplyAsync(
        ApplyEnrollmentReactivationCommand command,
        CancellationToken ct = default
    );
    Task ApplyDueAsync(CancellationToken ct = default);
}
