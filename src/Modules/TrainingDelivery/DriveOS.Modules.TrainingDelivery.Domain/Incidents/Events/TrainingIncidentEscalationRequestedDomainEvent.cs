using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Domain;


namespace DriveOS.Modules.TrainingDelivery.Domain.Incidents.Events;

public sealed record TrainingIncidentEscalationRequestedDomainEvent(TrainingIncidentId IncidentId, OrganizationId OrganizationId, TrainingSessionId SessionId, TrainingIncidentSeverity Severity, bool RequiresFleetFollowUp, bool RequiresComplianceFollowUp) : DomainEvent;
