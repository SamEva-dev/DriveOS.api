using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Modules.Students.Domain.Branches;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Students.Application.Branches;

public sealed record StudentBranchesResponse(
    Guid StudentId,
    Guid? PrimaryBranchId,
    IReadOnlyList<StudentBranchAssignmentItem> Assignments
);

public sealed record StudentBranchAssignmentItem(
    Guid Id,
    Guid BranchId,
    StudentBranchAssignmentType Type,
    StudentBranchService ServicesAllowed,
    DateOnly EffectiveFrom,
    DateOnly? EffectiveTo,
    string Reason,
    StudentBranchAssignmentStatus Status
);

public sealed record BranchVerificationItem(
    string Code,
    BranchVerificationStatus Status,
    string MessageKey
);

public sealed record BranchChangeImpactItem(
    BranchImpactType Type,
    int AffectedCount,
    string MessageKey,
    bool RequiresAction
);

public sealed record PrimaryBranchChangeAnalysisResponse(
    Guid AnalysisId,
    Guid? CurrentBranchId,
    Guid TargetBranchId,
    DateTimeOffset ExpiresAtUtc,
    IReadOnlyList<BranchVerificationItem> Verifications,
    IReadOnlyList<BranchChangeImpactItem> Impacts
);

public sealed record GetStudentBranchesQuery(OrganizationId OrganizationId, PersonId StudentId)
    : IQuery<StudentBranchesResponse>;

public sealed record AssignStudentBranchCommand(
    OrganizationId OrganizationId,
    PersonId StudentId,
    BranchId BranchId,
    StudentBranchAssignmentType Type,
    StudentBranchService ServicesAllowed,
    DateOnly EffectiveFrom,
    DateOnly? EffectiveTo,
    string Reason,
    UserId ActorUserId
) : ICommand<Guid>;

public sealed record AnalyzePrimaryBranchChangeCommand(
    OrganizationId OrganizationId,
    PersonId StudentId,
    BranchId TargetBranchId,
    UserId ActorUserId
) : ICommand<PrimaryBranchChangeAnalysisResponse>;

public sealed record ChangePrimaryStudentBranchCommand(
    OrganizationId OrganizationId,
    PersonId StudentId,
    Guid AnalysisId,
    string Reason,
    UserId ActorUserId
) : ICommand;

public sealed record EndStudentBranchAssignmentCommand(
    OrganizationId OrganizationId,
    PersonId StudentId,
    Guid AssignmentId,
    string Reason,
    UserId ActorUserId
) : ICommand;

public interface IStudentBranchService
{
    Task<StudentBranchesResponse?> GetAsync(
        GetStudentBranchesQuery q,
        CancellationToken ct = default
    );
    Task<Result<Guid>> AssignAsync(AssignStudentBranchCommand c, CancellationToken ct = default);
    Task<Result<PrimaryBranchChangeAnalysisResponse>> AnalyzePrimaryChangeAsync(
        AnalyzePrimaryBranchChangeCommand c,
        CancellationToken ct = default
    );
    Task<Result> ChangePrimaryAsync(
        ChangePrimaryStudentBranchCommand c,
        CancellationToken ct = default
    );
    Task<Result> EndAsync(EndStudentBranchAssignmentCommand c, CancellationToken ct = default);
}

public interface IStudentBranchVerifier
{
    Task<IReadOnlyList<BranchVerificationItem>> VerifyAsync(
        OrganizationId org,
        BranchId branch,
        CancellationToken ct = default
    );
}

public interface IStudentBranchImpactAnalyzer
{
    Task<IReadOnlyList<BranchChangeImpactItem>> AnalyzeAsync(
        OrganizationId org,
        PersonId student,
        BranchId? current,
        BranchId target,
        CancellationToken ct = default
    );
}

public static class StudentBranchApplicationErrors
{
    public static readonly Error StudentNotFound = Error.NotFound(
        "Students.Branches.Student.NotFound",
        "errors.students.branches.student.notFound"
    );
}
