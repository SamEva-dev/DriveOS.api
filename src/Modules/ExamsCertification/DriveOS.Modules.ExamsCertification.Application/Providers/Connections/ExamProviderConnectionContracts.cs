using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Modules.ExamsCertification.Domain.Providers;
using DriveOS.Modules.ExamsCertification.Domain.Providers.Connections;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.ExamsCertification.Application.Providers.Connections;

public sealed record CreateExamProviderConnectionCommand(
    OrganizationId OrganizationId,
    string ProviderCode,
    string DisplayName,
    string CountryCode,
    ExamPlaceProviderKind Kind,
    ExamProviderAuthenticationMode AuthenticationMode,
    string? BaseUrl,
    string? CredentialReference,
    int RequestsPerMinute,
    UserId ActorUserId) : ICommand<ExamProviderConnectionId>;

public sealed record GetExamProviderConnectionsQuery(OrganizationId OrganizationId) : IQuery<IReadOnlyList<ExamProviderConnectionResponse>>;
public sealed record GetExamProviderCatalogQuery(OrganizationId OrganizationId) : IQuery<IReadOnlyList<ExamProviderCatalogResponse>>;
public sealed record TestExamProviderConnectionCommand(OrganizationId OrganizationId, ExamProviderConnectionId ConnectionId, UserId ActorUserId)
    : ICommand<ExamProviderConnectionTestResponse>;
public sealed record SuspendExamProviderConnectionCommand(OrganizationId OrganizationId, ExamProviderConnectionId ConnectionId, UserId ActorUserId) : ICommand;
public sealed record RevokeExamProviderConnectionCommand(OrganizationId OrganizationId, ExamProviderConnectionId ConnectionId, UserId ActorUserId) : ICommand;

public sealed record ExamProviderConnectionResponse(Guid Id, string ProviderCode, string DisplayName, string CountryCode,
    string Kind, string AuthenticationMode, string? BaseUrl, bool HasCredentialReference, int RequestsPerMinute,
    string Status, DateTimeOffset? LastTestedAtUtc, DateTimeOffset? LastSuccessfulAtUtc, string? LastErrorCode,
    int ConsecutiveFailureCount);

public sealed record ExamProviderCatalogResponse(string Code, string CountryCode, string Kind, long Capabilities,
    bool AdapterEnabled, bool TenantConnected, string? ConnectionStatus);

public sealed record ExamProviderConnectionTestResponse(bool Success, string ProviderCode, string? ErrorCode,
    DateTimeOffset TestedAtUtc, IReadOnlyCollection<string> Capabilities);
