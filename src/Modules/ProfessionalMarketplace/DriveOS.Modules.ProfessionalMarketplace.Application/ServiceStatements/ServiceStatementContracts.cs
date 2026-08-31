using DriveOS.Application.Abstractions.Messaging;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.ProfessionalMarketplace.Application.ServiceStatements;

public sealed record CreateServiceStatementCommand(
    ServiceStatementId Id,OrganizationId OrganizationId,ProfessionalEngagementId EngagementId,
    DateOnly PeriodStart,DateOnly PeriodEnd,UserId ActorUserId):ICommand<ServiceStatementId>;

public sealed record SubmitServiceStatementCommand(ServiceStatementId Id,ProfessionalProfileId ProfileId,UserId ActorUserId):ICommand;
public sealed record StartServiceStatementReviewCommand(ServiceStatementId Id,OrganizationId OrganizationId,UserId ActorUserId):ICommand;
public sealed record RefreshServiceStatementCommand(ServiceStatementId Id,OrganizationId OrganizationId,UserId ActorUserId):ICommand;
public sealed record ApproveServiceStatementCommand(ServiceStatementId Id,OrganizationId OrganizationId,UserId ActorUserId):ICommand;
public sealed record RejectServiceStatementCommand(ServiceStatementId Id,OrganizationId OrganizationId,string Reason,UserId ActorUserId):ICommand;



public sealed record ListOrganizationServiceStatementsQuery(OrganizationId OrganizationId,ProfessionalEngagementId EngagementId)
    :IQuery<IReadOnlyList<ServiceStatementResponse>>;
public sealed record GetOrganizationServiceStatementQuery(OrganizationId OrganizationId,ServiceStatementId Id)
    :IQuery<ServiceStatementResponse>;

public sealed record ListCurrentProfessionalServiceStatementsQuery(UserId UserId,ProfessionalEngagementId? EngagementId=null)
    :IQuery<IReadOnlyList<ServiceStatementResponse>>;
public sealed record GetCurrentProfessionalServiceStatementQuery(UserId UserId,ServiceStatementId Id):IQuery<ServiceStatementResponse>;
public sealed record CreateCurrentProfessionalServiceStatementCommand(UserId UserId,ProfessionalEngagementId EngagementId,DateOnly PeriodStart,DateOnly PeriodEnd):ICommand<ServiceStatementId>;
public sealed record SubmitCurrentProfessionalServiceStatementCommand(UserId UserId,ServiceStatementId Id):ICommand;

public sealed record ServiceStatementResponse(
    Guid Id,Guid EngagementId,Guid ProfessionalProfileId,Guid ClientOrganizationId,Guid ProviderOrganizationId,
    DateOnly PeriodStart,DateOnly PeriodEnd,string Currency,decimal TotalAmount,decimal ApprovedAmount,decimal DisputedAmount,
    string Status,DateTimeOffset? SubmittedAtUtc,DateTimeOffset? ReviewedAtUtc,Guid? ReviewedByUserId,string? RejectionReason,
    DateTimeOffset CreatedAtUtc,IReadOnlyList<ServiceStatementLineResponse> Lines);
public sealed record ServiceStatementLineResponse(Guid ServiceEntryId,DateOnly ServiceDate,string ServiceCode,int QuantityMinutes,
    decimal UnitRate,string Currency,decimal TotalAmount,string Description,string EntryStatus);
