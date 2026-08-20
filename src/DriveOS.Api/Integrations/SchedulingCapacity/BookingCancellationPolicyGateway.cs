using DriveOS.Modules.Organizations.Application.OrganizationSettings;
using DriveOS.Modules.SchedulingCapacity.Application.Bookings;
using DriveOS.Modules.SchedulingCapacity.Domain.Bookings;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Api.Integrations.SchedulingCapacity;

internal sealed class BookingCancellationPolicyGateway(IOrganizationSettingsReadService settingsReadService)
    : IBookingCancellationPolicyGateway
{
    public async Task<BookingCancellationPolicyResolution> ResolveAsync(
        OrganizationId organizationId,
        Booking booking,
        CancellationInitiator initiator,
        CancellationReasonCode reasonCode,
        DateTimeOffset cancelledAtUtc,
        CancellationToken cancellationToken = default)
    {
        var settings = await settingsReadService.GetByOrganizationIdAsync(organizationId, cancellationToken);
        int thresholdHours = settings?.DefaultCancellationDelayHours ?? 48;
        int noticeMinutes = (int)Math.Floor((booking.StartAtUtc - cancelledAtUtc.ToUniversalTime()).TotalMinutes);

        bool organizationCaused = initiator is CancellationInitiator.Organization or CancellationInitiator.Instructor or CancellationInitiator.System;
        bool forceMajeure = initiator == CancellationInitiator.ForceMajeure || reasonCode == CancellationReasonCode.ForceMajeure;

        if (organizationCaused || forceMajeure)
        {
            return new BookingCancellationPolicyResolution(
                "scheduling.default-cancellation",
                settings?.Version ?? 1,
                "scheduling.cancellation.policy.noCharge.organizationOrForceMajeure",
                BookingCreditDecision.Released,
                BookingFeeDecision.NoCharge,
                initiator == CancellationInitiator.Instructor);
        }

        if (noticeMinutes >= thresholdHours * 60)
        {
            return new BookingCancellationPolicyResolution(
                "scheduling.default-cancellation",
                settings?.Version ?? 1,
                "scheduling.cancellation.policy.noCharge.beforeDeadline",
                BookingCreditDecision.Released,
                BookingFeeDecision.NoCharge,
                false);
        }

        return new BookingCancellationPolicyResolution(
            "scheduling.default-cancellation",
            settings?.Version ?? 1,
            "scheduling.cancellation.policy.manualReview.afterDeadline",
            BookingCreditDecision.PendingExternalReview,
            BookingFeeDecision.PendingExternalReview,
            false);
    }
}
