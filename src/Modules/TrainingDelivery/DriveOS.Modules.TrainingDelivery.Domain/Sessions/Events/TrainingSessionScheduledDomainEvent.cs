using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.TrainingDelivery.Domain.Sessions.Events;

public sealed record TrainingSessionScheduledDomainEvent(TrainingSessionId SessionId, OrganizationId OrganizationId, BookingId SourceBookingId, PersonId StudentId) : DomainEvent;
