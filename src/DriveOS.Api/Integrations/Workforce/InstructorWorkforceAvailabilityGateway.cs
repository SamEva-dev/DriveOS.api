using DriveOS.Modules.SchedulingCapacity.Application.Bookings;
using DriveOS.Modules.Workforce.Application.Availability;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Api.Integrations.Workforce;

internal sealed class InstructorWorkforceAvailabilityGateway(IWorkforceAvailabilityReadService workforce) : IInstructorWorkforceAvailabilityGateway
{
    public async Task<InstructorWorkforceAvailabilityResult> CheckAsync(
        OrganizationId organizationId,
        UserId instructorUserId,
        DateTimeOffset startAtUtc,
        DateTimeOffset endAtUtc,
        BranchId? branchId,
        string timeZoneId,
        CancellationToken cancellationToken = default)
    {
        TimeZoneInfo zone;
        try { zone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId); }
        catch (TimeZoneNotFoundException) { return new(false, null); }
        catch (InvalidTimeZoneException) { return new(false, null); }

        DateTimeOffset localStart = TimeZoneInfo.ConvertTime(startAtUtc, zone);
        DateTimeOffset localEnd = TimeZoneInfo.ConvertTime(endAtUtc, zone);
        DateOnly from = DateOnly.FromDateTime(localStart.DateTime);
        DateOnly to = DateOnly.FromDateTime(localEnd.DateTime);

        WorkforceEmploymentAvailabilitySnapshot professional = await workforce.CheckTeachingAvailabilityAsync(
            organizationId, instructorUserId, from, branchId, null, cancellationToken);
        if (!professional.IsProfessionallyAvailable)
            return new(true, professional.RestrictionId.HasValue
                ? $"{professional.ReasonCode}:{professional.RestrictionId.Value}"
                : professional.ReasonCode);

        IReadOnlyCollection<WorkforceAbsenceSnapshot> absences = await workforce.ListApprovedAbsencesAsync(
            organizationId, instructorUserId, from, to, cancellationToken);

        foreach (WorkforceAbsenceSnapshot absence in absences)
        {
            if (Overlaps(absence, localStart, localEnd))
                return new(true, $"workforce.leave.approved:{absence.LeaveRequestId}");
        }

        return new(false, null);
    }

    private static bool Overlaps(WorkforceAbsenceSnapshot absence, DateTimeOffset localStart, DateTimeOffset localEnd)
    {
        DateTime absenceStart = absence.StartDate.ToDateTime(
            absence.StartPortion == 2 ? new TimeOnly(12, 0) : TimeOnly.MinValue);

        DateTime absenceEnd = absence.EndPortion == 1
            ? absence.EndDate.ToDateTime(new TimeOnly(12, 0))
            : absence.EndDate.AddDays(1).ToDateTime(TimeOnly.MinValue);

        DateTime bookingStart = localStart.DateTime;
        DateTime bookingEnd = localEnd.DateTime;
        return bookingStart < absenceEnd && absenceStart < bookingEnd;
    }
}
