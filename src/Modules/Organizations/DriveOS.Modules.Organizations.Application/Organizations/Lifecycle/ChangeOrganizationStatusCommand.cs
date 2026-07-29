using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Modules.Organizations.Domain.Organizations;

namespace DriveOS.Modules.Organizations.Application
    .Organizations.Lifecycle;

public sealed record ChangeOrganizationStatusCommand(
    Guid OrganizationId,
    OrganizationStatus TargetStatus,
    string Reason)
    : ICommand;