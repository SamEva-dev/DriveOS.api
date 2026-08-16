using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Modules.Students.Domain.Transfers;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Students.Application.Transfers;

public sealed record InternalTransferResponse(
    Guid TransferId,
    Guid StudentId,
    Guid SourceBranchId,
    Guid TargetBranchId,
    InternalTransferMode Mode,
    InternalTransferElement Elements,
    DateOnly EffectiveOn,
    DateOnly? TemporaryUntil,
    string Reason,
    InternalTransferStatus Status,
    DateTimeOffset AnalysisExpiresAtUtc,
    IReadOnlyList<InternalTransferImpactItem> Impacts
);

public sealed record InternalTransferImpactItem(
    InternalTransferImpactType Type,
    int AffectedCount,
    InternalTransferImpactStatus Status,
    string MessageKey,
    bool RequiresAction
);

public sealed record GetInternalTransfersQuery(OrganizationId OrganizationId, PersonId StudentId)
    : IQuery<IReadOnlyList<InternalTransferResponse>>;

public sealed record AnalyzeInternalTransferCommand(
    OrganizationId OrganizationId,
    PersonId StudentId,
    BranchId TargetBranchId,
    InternalTransferMode Mode,
    InternalTransferElement Elements,
    DateOnly? EffectiveOn,
    DateOnly? TemporaryUntil,
    string Reason,
    UserId ActorUserId
) : ICommand<InternalTransferResponse>;

public sealed record ValidateInternalTransferCommand(
    OrganizationId OrganizationId,
    PersonId StudentId,
    Guid TransferId,
    UserId ActorUserId
) : ICommand<InternalTransferResponse>;

public interface IInternalTransferService
{
    Task<IReadOnlyList<InternalTransferResponse>> GetAsync(
        GetInternalTransfersQuery query,
        CancellationToken ct = default
    );
    Task<Result<InternalTransferResponse>> AnalyzeAsync(
        AnalyzeInternalTransferCommand command,
        CancellationToken ct = default
    );
    Task<Result<InternalTransferResponse>> ValidateAsync(
        ValidateInternalTransferCommand command,
        CancellationToken ct = default
    );
}

public interface IInternalTransferImpactAnalyzer
{
    Task<IReadOnlyList<InternalTransferImpactSeed>> AnalyzeAsync(
        OrganizationId organizationId,
        PersonId studentId,
        BranchId sourceBranchId,
        BranchId targetBranchId,
        InternalTransferElement elements,
        CancellationToken ct = default
    );
}

public static class InternalTransferApplicationErrors
{
    public static readonly Error StudentNotFound = Error.NotFound(
        "Students.InternalTransfer.Student.NotFound",
        "errors.students.internalTransfer.student.notFound"
    );
    public static readonly Error EnrollmentNotFound = Error.NotFound(
        "Students.InternalTransfer.Enrollment.NotFound",
        "errors.students.internalTransfer.enrollment.notFound"
    );
    public static readonly Error SourceBranchNotFound = Error.NotFound(
        "Students.InternalTransfer.SourceBranch.NotFound",
        "errors.students.internalTransfer.sourceBranch.notFound"
    );
    public static readonly Error TargetBranchNotEligible = Error.Conflict(
        "Students.InternalTransfer.TargetBranch.NotEligible",
        "errors.students.internalTransfer.targetBranch.notEligible"
    );
}
