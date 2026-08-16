using DriveOS.Application.Abstractions.Messaging;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.CRM.Application.Tasks.CloseTask;

public sealed record CloseCrmTaskCommand(
    OrganizationId OrganizationId,
    CrmTaskId TaskId,
    bool Cancel
) : ICommand;
