
using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;
namespace DriveOS.Modules.TrainingDelivery.Domain.Incidents.Events;
public sealed record TrainingIncidentResolvedDomainEvent(TrainingIncidentId IncidentId, OrganizationId OrganizationId, TrainingSessionId SessionId, UserId ResolvedByUserId) : DomainEvent;
