using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Modules.Students.Domain.ExternalTransfers;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Students.Application.ExternalTransfers;

public sealed record ExternalTransferResponse(
    Guid TransferId,
    Guid StudentId,
    Guid SourceOrganizationId,
    Guid TargetOrganizationId,
    ExternalTransferType Type,
    ExternalTransferDataScope DataScope,
    DateOnly EffectiveOn,
    DateOnly? TemporaryUntil,
    string CountryCode,
    string Reason,
    string Responsibilities,
    ExternalTransferStatus Status,
    TransferConsentStatus ConsentStatus,
    TransferFinancialStatus FinancialStatus,
    TargetRelationshipStatus RelationshipStatus,
    IReadOnlyList<StudentDataGrantItem> DataGrants,
    IReadOnlyList<ExternalTransferAuditItem> Audit
);

public sealed record StudentDataGrantItem(
    Guid Id,
    Guid GranteeOrganizationId,
    ExternalTransferDataScope Scope,
    DateTimeOffset GrantedAtUtc,
    DateOnly? ExpiresOn,
    bool IsActive
);

public sealed record ExternalTransferAuditItem(
    string Action,
    string Detail,
    Guid ActorUserId,
    DateTimeOffset OccurredAtUtc
);

public sealed record ExternalTransferPreconditions(
    TargetRelationshipStatus RelationshipStatus,
    bool TargetOrganizationActive,
    bool CountryRuleSatisfied,
    string SourceCountryCode,
    string TargetCountryCode,
    IReadOnlyList<string> Warnings
);

public sealed record GetExternalTransfersQuery(OrganizationId OrganizationId, PersonId StudentId)
    : IQuery<IReadOnlyList<ExternalTransferResponse>>;

public sealed record CreateExternalTransferCommand(
    OrganizationId OrganizationId,
    PersonId StudentId,
    OrganizationId TargetOrganizationId,
    ExternalTransferType Type,
    ExternalTransferDataScope DataScope,
    DateOnly EffectiveOn,
    DateOnly? TemporaryUntil,
    string CountryCode,
    string Reason,
    string Responsibilities,
    UserId ActorUserId
) : ICommand<Guid>;

public sealed record VerifyExternalTransferConsentCommand(
    OrganizationId OrganizationId,
    PersonId StudentId,
    Guid TransferId,
    string EvidenceReference,
    UserId ActorUserId
) : ICommand;

public sealed record ReviewExternalTransferFinanceCommand(
    OrganizationId OrganizationId,
    PersonId StudentId,
    Guid TransferId,
    TransferFinancialStatus Status,
    string? Resolution,
    UserId ActorUserId
) : ICommand;

public sealed record SubmitExternalTransferCommand(
    OrganizationId OrganizationId,
    PersonId StudentId,
    Guid TransferId,
    bool RequestInvitationIfMissing,
    UserId ActorUserId
) : ICommand<ExternalTransferPreconditions>;

public sealed record DecideExternalTransferCommand(
    OrganizationId OrganizationId,
    PersonId StudentId,
    Guid TransferId,
    bool Accept,
    string Reason,
    UserId ActorUserId
) : ICommand;

public sealed record CompleteExternalTransferCommand(
    OrganizationId OrganizationId,
    PersonId StudentId,
    Guid TransferId,
    UserId ActorUserId
) : ICommand;

public interface IExternalTransferService
{
    Task<IReadOnlyList<ExternalTransferResponse>> GetAsync(
        GetExternalTransfersQuery query,
        CancellationToken ct = default
    );
    Task<Result<Guid>> CreateAsync(
        CreateExternalTransferCommand command,
        CancellationToken ct = default
    );
    Task<Result> VerifyConsentAsync(
        VerifyExternalTransferConsentCommand command,
        CancellationToken ct = default
    );
    Task<Result> ReviewFinanceAsync(
        ReviewExternalTransferFinanceCommand command,
        CancellationToken ct = default
    );
    Task<Result<ExternalTransferPreconditions>> SubmitAsync(
        SubmitExternalTransferCommand command,
        CancellationToken ct = default
    );
    Task<Result> DecideAsync(DecideExternalTransferCommand command, CancellationToken ct = default);
    Task<Result> CompleteAsync(
        CompleteExternalTransferCommand command,
        CancellationToken ct = default
    );
}

public interface IExternalTransferPreconditionGateway
{
    Task<ExternalTransferPreconditions> VerifyAsync(
        OrganizationId source,
        OrganizationId target,
        string requestedCountry,
        bool requestInvitation,
        CancellationToken ct = default
    );
}

public static class ExternalTransferApplicationErrors
{
    public static readonly Error StudentNotFound = Error.NotFound(
        "Students.ExternalTransfer.Student.NotFound",
        "errors.students.externalTransfer.student.notFound"
    );
    public static readonly Error TargetOrganizationNotFound = Error.NotFound(
        "Students.ExternalTransfer.TargetOrganization.NotFound",
        "errors.students.externalTransfer.targetOrganization.notFound"
    );
}
