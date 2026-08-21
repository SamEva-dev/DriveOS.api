
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Domain;
namespace DriveOS.Modules.TrainingDelivery.Domain.Incidents.Events;
public sealed record TrainingIncidentEscalatedDomainEvent(TrainingIncidentId IncidentId, OrganizationId OrganizationId, TrainingSessionId SessionId, UserId EscalatedByUserId) : DomainEvent;
