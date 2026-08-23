using DriveOS.Modules.Workforce.Domain.LeavePolicies;
using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;
namespace DriveOS.Modules.Workforce.Domain.Events;
public sealed record LeavePolicyCreatedDomainEvent(Guid EventId,DateTimeOffset OccurredAtUtc,LeavePolicyId LeavePolicyId,OrganizationId OrganizationId,string CountryCode,string Code,LeaveCategory Category):IDomainEvent;
public sealed record LeavePolicyUpdatedDomainEvent(Guid EventId,DateTimeOffset OccurredAtUtc,LeavePolicyId LeavePolicyId,OrganizationId OrganizationId,string CountryCode,string Code,LeaveCategory Category,UserId ActorUserId):IDomainEvent;
public sealed record LeavePolicyDeactivatedDomainEvent(Guid EventId,DateTimeOffset OccurredAtUtc,LeavePolicyId LeavePolicyId,OrganizationId OrganizationId,UserId ActorUserId):IDomainEvent;
public sealed record LeavePolicyReactivatedDomainEvent(Guid EventId,DateTimeOffset OccurredAtUtc,LeavePolicyId LeavePolicyId,OrganizationId OrganizationId,UserId ActorUserId):IDomainEvent;
