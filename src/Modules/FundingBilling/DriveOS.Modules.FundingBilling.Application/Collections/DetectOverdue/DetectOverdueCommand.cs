using DriveOS.Application.Abstractions.Messaging;
using DriveOS.SharedKernel.Identifiers;
namespace DriveOS.Modules.FundingBilling.Application.Collections.DetectOverdue;
public sealed record DetectOverdueCommand(OrganizationId OrganizationId, DateOnly BusinessDate, UserId ActorUserId) : ICommand<DetectOverdueResponse>;
public sealed record DetectOverdueResponse(int InvoicesMarkedOverdue, int InstallmentsMarkedOverdue);
