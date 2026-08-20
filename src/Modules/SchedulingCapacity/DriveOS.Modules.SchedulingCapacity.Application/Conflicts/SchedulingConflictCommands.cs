using DriveOS.Application.Abstractions.Messaging;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.SchedulingCapacity.Application.Conflicts;

public sealed record RefreshSchedulingConflictsCommand(OrganizationId OrganizationId, BookingId BookingId) : ICommand<SchedulingConflictScanResponse>;
public sealed record ResolveSchedulingConflictCommand(OrganizationId OrganizationId, SchedulingConflictId ConflictId, int Resolution, string Reason) : ICommand;
public sealed record OverrideSchedulingConflictCommand(OrganizationId OrganizationId, SchedulingConflictId ConflictId, string Reason, string Risk, DateTimeOffset ExpiresAtUtc) : ICommand;
