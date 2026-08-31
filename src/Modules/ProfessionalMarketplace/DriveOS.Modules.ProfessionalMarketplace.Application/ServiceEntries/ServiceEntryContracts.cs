using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Modules.ProfessionalMarketplace.Domain.ServiceEntries;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.ProfessionalMarketplace.Application.ServiceEntries;

public sealed record RecordServiceEntryCommand(
    ServiceEntryId Id,OrganizationId OrganizationId,ProfessionalEngagementId EngagementId,
    ProfessionalMissionId? MissionId,ServiceEntrySourceType SourceType,Guid SourceId,DateOnly ServiceDate,
    string ServiceCode,int QuantityMinutes,decimal UnitRate,decimal ExpensesAmount,decimal IndemnitiesAmount,
    decimal DiscountAmount,string Currency,string Description,UserId ActorUserId):ICommand<ServiceEntryId>;

public sealed record SubmitServiceEntryCommand(ServiceEntryId Id,ProfessionalProfileId ProfileId,UserId ActorUserId):ICommand;
public sealed record ApproveServiceEntryCommand(ServiceEntryId Id,OrganizationId OrganizationId,UserId ActorUserId):ICommand;
public sealed record RejectServiceEntryCommand(ServiceEntryId Id,OrganizationId OrganizationId,string Reason,UserId ActorUserId):ICommand;
public sealed record DisputeServiceEntryCommand(ServiceEntryId Id,OrganizationId OrganizationId,string Reason,UserId ActorUserId):ICommand;


public sealed record ListCurrentProfessionalServiceEntriesQuery(UserId UserId,ProfessionalMissionId? MissionId=null)
    :IQuery<IReadOnlyList<ServiceEntryResponse>>;
public sealed record GetCurrentProfessionalServiceEntryQuery(UserId UserId,ServiceEntryId Id)
    :IQuery<ServiceEntryResponse>;
public sealed record SubmitCurrentProfessionalServiceEntryCommand(UserId UserId,ServiceEntryId Id):ICommand;

public sealed record ServiceEntryResponse(
    Guid Id,Guid EngagementId,Guid? MissionId,Guid ProfessionalProfileId,Guid OrganizationId,Guid? BranchId,
    string SourceType,Guid SourceId,DateOnly ServiceDate,string ServiceCode,int QuantityMinutes,decimal UnitRate,
    decimal BaseAmount,decimal ExpensesAmount,decimal IndemnitiesAmount,decimal DiscountAmount,decimal TotalAmount,
    string Currency,string Description,string Status,DateTimeOffset? SubmittedAtUtc,DateTimeOffset? ReviewedAtUtc,
    Guid? ReviewedByUserId,string? ReviewReason,DateTimeOffset CreatedAtUtc);
