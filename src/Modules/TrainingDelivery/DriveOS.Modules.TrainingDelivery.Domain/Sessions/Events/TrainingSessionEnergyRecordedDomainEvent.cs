using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.TrainingDelivery.Domain.Sessions.Events;

public sealed record TrainingSessionEnergyRecordedDomainEvent(
    TrainingSessionId SessionId, OrganizationId OrganizationId, Guid VehicleId, TrainingSessionEnergyEntryId EntryId,
    TrainingSessionEnergyEntryType Type, decimal? EnergyLevelPercent, decimal? Quantity, DateTimeOffset ObservedAtUtc) : DomainEvent;
