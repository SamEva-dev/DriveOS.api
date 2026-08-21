using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.TrainingDelivery.Application.Sessions;

public sealed record ConfirmedBookingSessionSource(OrganizationId OrganizationId, OrganizationId StudentOwnerOrganizationId, OrganizationId PerformingOrganizationId, BookingId BookingId, PersonId StudentId, TrainingPathId TrainingPathId, UserId InstructorId, BranchId? BranchId, Guid? VehicleId, DateTimeOffset PlannedStartAtUtc, DateTimeOffset PlannedEndAtUtc, string? TrainingCategory, string? Objectives, string? MeetingPoint, string? PricingReference, TrainingCreditAccountId? TrainingCreditAccountId, decimal? CreditQuantity, string? CreditReservationReference);

public interface IConfirmedBookingSessionSourceGateway
{
    Task<Result<ConfirmedBookingSessionSource>> GetAsync(OrganizationId organizationId, BookingId bookingId, CancellationToken cancellationToken = default);
}

public interface ITrainingSessionMaterializationLock
{
    Task AcquireAsync(OrganizationId organizationId, BookingId bookingId, CancellationToken cancellationToken = default);
}
