using DriveOS.Application.Abstractions.Messaging;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.TrainingDelivery.Application.Sessions;

public sealed record TrainingSessionReportRevisionResponse(Guid Id, int Scenario, int Status, string FieldCode, string CurrentValue, string ProposedValue, string Reason, bool HasFinancialImpact, Guid RequestedByUserId, DateTimeOffset RequestedAtUtc, Guid? DecidedByUserId, DateTimeOffset? DecidedAtUtc, string? DecisionReason, int? AppliedReportVersion);
public sealed record RequestTrainingSessionReportRevisionCommand(OrganizationId OrganizationId, TrainingSessionId SessionId, Guid OperationId, int ExpectedVersion, int Scenario, string FieldCode, string CurrentValue, string ProposedValue, string Reason, bool HasFinancialImpact, bool ApprovalRequired, UserId ActorUserId) : ICommand<TrainingSessionReportRevisionResponse>;
public sealed record DecideTrainingSessionReportRevisionCommand(OrganizationId OrganizationId, TrainingSessionId SessionId, TrainingSessionReportRevisionId RevisionId, bool Approve, string? DecisionReason, UserId ActorUserId) : ICommand<TrainingSessionReportRevisionResponse>;
public sealed record GetTrainingSessionReportRevisionsQuery(OrganizationId OrganizationId, TrainingSessionId SessionId) : IQuery<IReadOnlyCollection<TrainingSessionReportRevisionResponse>>;
