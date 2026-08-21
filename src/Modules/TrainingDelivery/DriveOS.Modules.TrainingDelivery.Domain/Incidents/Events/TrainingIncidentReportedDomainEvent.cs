
using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;
namespace DriveOS.Modules.TrainingDelivery.Domain.Incidents.Events;
public sealed record TrainingIncidentReportedDomainEvent(TrainingIncidentId IncidentId, OrganizationId OrganizationId, TrainingSessionId SessionId, TrainingIncidentType IncidentType, TrainingIncidentSeverity Severity) : DomainEvent;
