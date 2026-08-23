using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.Workforce.Application.Availability;

public sealed record WorkforceAbsenceSnapshot(
    Guid LeaveRequestId,
    Guid EmployeeId,
    DateOnly StartDate,
    DateOnly EndDate,
    int StartPortion,
    int EndPortion,
    string PolicyCode,
    string? Reason);

public sealed record WorkforceEmploymentAvailabilitySnapshot(bool IsProfessionallyAvailable, string? ReasonCode, Guid? RestrictionId);

public interface IWorkforceAvailabilityReadService
{
    Task<IReadOnlyCollection<WorkforceAbsenceSnapshot>> ListApprovedAbsencesAsync(
        OrganizationId organizationId,
        UserId userId,
        DateOnly from,
        DateOnly to,
        CancellationToken cancellationToken = default);

    Task<WorkforceEmploymentAvailabilitySnapshot> CheckTeachingAvailabilityAsync(
        OrganizationId organizationId, UserId userId, DateOnly date, BranchId? branchId = null, string? licenseCategoryCode = null, CancellationToken cancellationToken = default);
}
