using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.TrainingDelivery.Domain.Sessions.Events;

public sealed record TrainingCreditConsumptionRequestedDomainEvent(
    TrainingSessionId SessionId,
    OrganizationId OrganizationId,
    PersonId StudentId,
    TrainingCreditAccountId TrainingCreditAccountId,
    decimal CreditQuantity,
    string CreditReservationReference,
    DateTimeOffset RequestedAtUtc) : DomainEvent;
