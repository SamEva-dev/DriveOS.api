using DriveOS.Application.Abstractions.Messaging;
using DriveOS.SharedKernel.Identifiers;
namespace DriveOS.Modules.Organizations.Application.OrganizationSubscriptions.CheckLimit;
public sealed record CheckOrganizationLimitQuery(OrganizationId OrganizationId, string LimitCode, long CurrentUsage, long RequestedIncrease) : IQuery<OrganizationLimitCheckResponse>;
