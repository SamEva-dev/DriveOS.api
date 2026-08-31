using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Modules.ProfessionalMarketplace.Domain.Disputes;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.ProfessionalMarketplace.Application.Disputes;

public sealed record ServiceDisputeEvidenceInput(Guid DocumentReferenceId,string Label,string? Note);

public sealed record OpenServiceDisputeCommand(
    ServiceDisputeId Id,
    ServiceEntryId ServiceEntryId,
    OrganizationId ClientOrganizationId,
    ServiceDisputeParty RaisedByParty,
    Guid RaisedByOrganizationId,
    ProfessionalProfileId? RaisedByProfessionalProfileId,
    ServiceDisputeReason Reason,
    string Description,
    ServiceDisputeEvidenceInput[] Evidence,
    UserId ActorUserId):ICommand<ServiceDisputeId>;

public sealed record AddServiceDisputeMessageCommand(
    ServiceDisputeId Id,OrganizationId ClientOrganizationId,ServiceDisputeParty Party,string Message,UserId ActorUserId):ICommand;

public sealed record AddServiceDisputeEvidenceCommand(
    ServiceDisputeId Id,OrganizationId ClientOrganizationId,ServiceDisputeEvidenceInput Evidence,UserId ActorUserId):ICommand;

public sealed record WaitServiceDisputeForCommand(
    ServiceDisputeId Id,OrganizationId ClientOrganizationId,ServiceDisputeParty WaitingFor,UserId ActorUserId):ICommand;

public sealed record ResolveServiceDisputeCommand(
    ServiceDisputeId Id,OrganizationId ClientOrganizationId,ServiceDisputeResolutionOutcome Outcome,string Resolution,UserId ActorUserId):ICommand;

public sealed record EscalateServiceDisputeCommand(
    ServiceDisputeId Id,OrganizationId ClientOrganizationId,string Reason,UserId ActorUserId):ICommand;

public sealed record GetServiceDisputeQuery(ServiceDisputeId Id,OrganizationId? OrganizationId,ProfessionalProfileId? ProfessionalProfileId):IQuery<ServiceDisputeResponse>;

public sealed record ServiceDisputeResponse(
    Guid Id,Guid ServiceEntryId,Guid EngagementId,Guid ProfessionalProfileId,Guid ClientOrganizationId,
    Guid RaisedByOrganizationId,string Reason,string Description,string Status,string? ResolutionOutcome,string? Resolution,
    ServiceDisputeEvidence[] Evidence,ServiceDisputeMessage[] Discussion,DateTimeOffset CreatedAtUtc,DateTimeOffset? ResolvedAtUtc,
    DateTimeOffset? EscalatedAtUtc,Guid? EscalatedByUserId,string? EscalationReason);

public sealed record ListOrganizationServiceDisputesQuery(OrganizationId OrganizationId):IQuery<IReadOnlyList<ServiceDisputeResponse>>;
public sealed record ListProfessionalServiceDisputesQuery(ProfessionalProfileId ProfessionalProfileId):IQuery<IReadOnlyList<ServiceDisputeResponse>>;
