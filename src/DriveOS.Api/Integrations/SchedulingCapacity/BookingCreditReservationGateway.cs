using DomainRelay.Abstractions;
using DriveOS.Modules.FundingBilling.Application.TrainingCredits.Manage;
using DriveOS.Modules.SchedulingCapacity.Application.Bookings;
using DriveOS.Modules.SchedulingCapacity.Domain.Bookings;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Api.Integrations.SchedulingCapacity;

internal sealed class BookingCreditReservationGateway(IMediator mediator) : IBookingCreditReservationGateway
{
    public async Task<Result<BookingCreditReservationResult>> ReserveAsync(
        OrganizationId organizationId,
        Guid trainingCreditAccountId,
        decimal quantity,
        BookingId bookingId,
        UserId actorUserId,
        CancellationToken cancellationToken = default)
    {
        string reference = $"scheduling-booking:{bookingId.Value:N}:credit-reservation";
        Result<TrainingCreditMovementId> result = await mediator.Send(
            new RecordTrainingCreditMovementCommand(
                organizationId,
                new TrainingCreditAccountId(trainingCreditAccountId),
                TrainingCreditOperation.Reserve,
                quantity,
                reference,
                "Scheduling booking confirmation",
                actorUserId),
            cancellationToken);

        if (result.IsSuccess)
            return Result.Success(new BookingCreditReservationResult(reference));

        return result.Error.Code.Contains("Available.Insufficient", StringComparison.OrdinalIgnoreCase)
            ? Result.Failure<BookingCreditReservationResult>(BookingErrors.CreditInsufficient)
            : Result.Failure<BookingCreditReservationResult>(BookingErrors.CreditReservationFailed);
    }
}
