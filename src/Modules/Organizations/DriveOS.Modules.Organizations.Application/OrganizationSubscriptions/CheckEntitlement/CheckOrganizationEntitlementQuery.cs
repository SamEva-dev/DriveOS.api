using DriveOS.Application.Abstractions.Messaging;
using DriveOS.SharedKernel.Identifiers;
namespace DriveOS.Modules.Organizations.Application.OrganizationSubscriptions.CheckEntitlement;
public sealed record CheckOrganizationEntitlementQuery(OrganizationId OrganizationId, string EntitlementCode) : IQuery<bool>;
