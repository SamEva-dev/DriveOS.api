using DriveOS.Application.Abstractions.Messaging;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.Organizations.Application.OrganizationSettings.UpdateOperationalSettings;

public sealed record UpdateOrganizationOperationalSettingsCommand(
    OrganizationId OrganizationId,
    int DefaultSessionDurationMinutes,
    int DefaultBookingLeadTimeMinutes,
    int DefaultCancellationDelayHours,
    bool AllowStudentSelfBooking,
    bool RequireBranchForOperations,
    BranchId? DefaultBranchId,
    int ExpectedVersion)
    : ICommand;
