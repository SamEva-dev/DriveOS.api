using DriveOS.Application.Abstractions.Messaging;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.CRM.Application.Tasks.CloseTask;

public sealed record CloseCrmTaskCommand(OrganizationId OrganizationId, Guid TaskId, bool Cancel) : ICommand;
