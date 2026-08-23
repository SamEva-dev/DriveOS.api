using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.SchedulingCapacity.Application.Bookings;

public sealed record InstructorWorkforceAvailabilityResult(bool IsUnavailable, string? Reason);

public interface IInstructorWorkforceAvailabilityGateway
{
    Task<InstructorWorkforceAvailabilityResult> CheckAsync(
        OrganizationId organizationId,
        UserId instructorUserId,
        DateTimeOffset startAtUtc,
        DateTimeOffset endAtUtc,
        BranchId branchId,
        string timeZoneId,
        CancellationToken cancellationToken = default);
}
