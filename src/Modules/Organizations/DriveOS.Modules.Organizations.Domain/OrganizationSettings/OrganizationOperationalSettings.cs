using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Organizations.Domain.OrganizationSettings;

public sealed record OrganizationOperationalSettings
{
    public const int MinimumSessionDurationMinutes = 15;
    public const int MaximumSessionDurationMinutes = 480;
    public const int MaximumBookingLeadTimeMinutes = 525600;
    public const int MaximumCancellationDelayHours = 8760;

    private OrganizationOperationalSettings(
        int defaultSessionDurationMinutes,
        int defaultBookingLeadTimeMinutes,
        int defaultCancellationDelayHours,
        bool allowStudentSelfBooking,
        bool requireBranchForOperations,
        BranchId? defaultBranchId
    )
    {
        DefaultSessionDurationMinutes = defaultSessionDurationMinutes;
        DefaultBookingLeadTimeMinutes = defaultBookingLeadTimeMinutes;
        DefaultCancellationDelayHours = defaultCancellationDelayHours;
        AllowStudentSelfBooking = allowStudentSelfBooking;
        RequireBranchForOperations = requireBranchForOperations;
        DefaultBranchId = defaultBranchId;
    }

    public int DefaultSessionDurationMinutes { get; }
    public int DefaultBookingLeadTimeMinutes { get; }
    public int DefaultCancellationDelayHours { get; }
    public bool AllowStudentSelfBooking { get; }
    public bool RequireBranchForOperations { get; }
    public BranchId? DefaultBranchId { get; }

    public static Result<OrganizationOperationalSettings> Create(
        int defaultSessionDurationMinutes,
        int defaultBookingLeadTimeMinutes,
        int defaultCancellationDelayHours,
        bool allowStudentSelfBooking,
        bool requireBranchForOperations,
        BranchId? defaultBranchId
    )
    {
        if (
            defaultSessionDurationMinutes
            is < MinimumSessionDurationMinutes
                or > MaximumSessionDurationMinutes
        )
        {
            return Result.Failure<OrganizationOperationalSettings>(
                OrganizationSettingsErrors.InvalidSessionDuration
            );
        }

        if (defaultBookingLeadTimeMinutes is < 0 or > MaximumBookingLeadTimeMinutes)
        {
            return Result.Failure<OrganizationOperationalSettings>(
                OrganizationSettingsErrors.InvalidBookingLeadTime
            );
        }

        if (defaultCancellationDelayHours is < 0 or > MaximumCancellationDelayHours)
        {
            return Result.Failure<OrganizationOperationalSettings>(
                OrganizationSettingsErrors.InvalidCancellationDelay
            );
        }

        if (defaultBranchId is { IsEmpty: true })
        {
            return Result.Failure<OrganizationOperationalSettings>(
                OrganizationSettingsErrors.InvalidDefaultBranch
            );
        }

        if (requireBranchForOperations && defaultBranchId is null)
        {
            return Result.Failure<OrganizationOperationalSettings>(
                OrganizationSettingsErrors.DefaultBranchRequired
            );
        }

        return Result.Success(
            new OrganizationOperationalSettings(
                defaultSessionDurationMinutes,
                defaultBookingLeadTimeMinutes,
                defaultCancellationDelayHours,
                allowStudentSelfBooking,
                requireBranchForOperations,
                defaultBranchId
            )
        );
    }
}
