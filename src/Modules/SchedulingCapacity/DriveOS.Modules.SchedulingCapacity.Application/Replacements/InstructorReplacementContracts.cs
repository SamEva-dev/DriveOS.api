using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.SchedulingCapacity.Application.Replacements;

public sealed record InstructorReplacementSuggestionResponse(
    Guid InstructorId,
    Guid CalendarResourceId,
    string DisplayName,
    Guid? BranchId,
    bool QualificationVerified,
    bool IsAvailableForAllBookings,
    bool HasStudentContinuity,
    decimal? LoadPercentage,
    int CompatibleBookingCount,
    int TargetBookingCount,
    int Score,
    IReadOnlyCollection<string> Factors,
    IReadOnlyCollection<string> ExternalReviews);

public sealed record InstructorReplacementPreviewResponse(
    Guid OperationId,
    Guid PreviousInstructorId,
    Guid ReplacementInstructorId,
    int Mode,
    IReadOnlyCollection<Guid> BookingIds,
    IReadOnlyCollection<Guid> StudentIds,
    bool CanConfirm,
    IReadOnlyCollection<string> BlockingReasons,
    IReadOnlyCollection<string> ExternalReviews);

public sealed record InstructorReplacementApplyResponse(
    Guid OperationId,
    int ReplacedBookingCount,
    IReadOnlyCollection<Guid> BookingIds,
    IReadOnlyCollection<Guid> StudentIds);

public sealed record InstructorReplacementEligibility(
    bool IsEligible,
    bool QualificationVerified,
    bool HasStudentContinuity,
    IReadOnlyCollection<string> Warnings);

public interface IInstructorReplacementEligibilityGateway
{
    Task<InstructorReplacementEligibility> EvaluateAsync(
        OrganizationId organizationId,
        PersonId? studentId,
        UserId instructorId,
        BranchId? branchId,
        string trainingCategory,
        CancellationToken cancellationToken = default);
}

public interface IInstructorReplacementService
{
    Task<IReadOnlyCollection<InstructorReplacementSuggestionResponse>> SuggestAsync(
        OrganizationId organizationId,
        UserId previousInstructorId,
        IReadOnlyCollection<BookingId> bookingIds,
        string trainingCategory,
        CancellationToken cancellationToken = default);

    Task<InstructorReplacementPreviewResponse?> PreviewAsync(
        OrganizationId organizationId,
        Guid operationId,
        UserId previousInstructorId,
        UserId replacementInstructorId,
        int mode,
        IReadOnlyCollection<BookingId> bookingIds,
        string trainingCategory,
        DateTimeOffset? accessExpiresAtUtc,
        CancellationToken cancellationToken = default);

    Task<DriveOS.SharedKernel.Results.Result<InstructorReplacementApplyResponse>> ApplyAsync(
        OrganizationId organizationId,
        Guid operationId,
        UserId previousInstructorId,
        UserId replacementInstructorId,
        int mode,
        IReadOnlyCollection<BookingId> bookingIds,
        string trainingCategory,
        string reason,
        DateTimeOffset? accessExpiresAtUtc,
        CancellationToken cancellationToken = default);
}
