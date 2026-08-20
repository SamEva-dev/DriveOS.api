using DriveOS.Application.Abstractions.Messaging;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.SchedulingCapacity.Application.Replacements;

public sealed record ApplyInstructorReplacementCommand(
    OrganizationId OrganizationId,
    Guid OperationId,
    UserId PreviousInstructorId,
    UserId ReplacementInstructorId,
    int Mode,
    IReadOnlyCollection<BookingId> BookingIds,
    string TrainingCategory,
    string Reason,
    DateTimeOffset? AccessExpiresAtUtc) : ICommand<InstructorReplacementApplyResponse>;
