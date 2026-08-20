using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.SchedulingCapacity.Application.SlotSearch;

public sealed record SlotSearchRequest(
    Guid StudentId,
    int BookingType,
    int DurationMinutes,
    DateTimeOffset FromUtc,
    DateTimeOffset ToUtc,
    Guid? BranchId,
    Guid? PreferredInstructorId,
    Guid? PreferredVehicleId,
    bool RequireVehicle,
    bool RequireRoom,
    int StepMinutes = 30,
    int MaxSuggestions = 10,
    string? TrainingCategory = null,
    bool PreferContinuity = true);

public sealed record SlotSearchInstructorContext(
    bool QualificationVerified,
    bool IsEligible,
    bool HasStudentContinuity,
    IReadOnlyCollection<string> Warnings);

public sealed record SlotSearchSuggestion(
    DateTimeOffset StartAtUtc,
    DateTimeOffset EndAtUtc,
    Guid? BranchId,
    Guid? InstructorId,
    Guid? InstructorCalendarResourceId,
    string? InstructorDisplayName,
    Guid? VehicleId,
    Guid? VehicleCalendarResourceId,
    string? VehicleDisplayName,
    Guid? RoomCalendarResourceId,
    string? RoomDisplayName,
    bool QualificationVerified,
    bool HasStudentContinuity,
    int InstructorScheduledMinutes,
    int VehicleScheduledMinutes,
    int Score,
    string Compatibility,
    IReadOnlyCollection<string> Reasons,
    IReadOnlyCollection<string> ExternalReviews);

public sealed record SlotSearchResponse(
    DateTimeOffset SearchedFromUtc,
    DateTimeOffset SearchedToUtc,
    int DurationMinutes,
    int EvaluatedCandidates,
    IReadOnlyCollection<SlotSearchSuggestion> Suggestions,
    IReadOnlyCollection<string> Warnings);

public interface ISlotSearchInstructorContextGateway
{
    Task<SlotSearchInstructorContext> EvaluateAsync(
        OrganizationId organizationId,
        PersonId studentId,
        UserId instructorId,
        BranchId? branchId,
        string trainingCategory,
        CancellationToken cancellationToken = default);
}

public interface ISlotSearchService
{
    Task<SlotSearchResponse> SearchAsync(OrganizationId organizationId, SlotSearchRequest request, CancellationToken cancellationToken = default);
}
