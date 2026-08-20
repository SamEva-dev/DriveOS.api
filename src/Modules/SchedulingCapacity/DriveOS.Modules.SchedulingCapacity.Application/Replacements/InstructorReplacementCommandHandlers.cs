using DriveOS.Application.Abstractions.Messaging;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.SchedulingCapacity.Application.Replacements;

public sealed class ApplyInstructorReplacementCommandHandler(IInstructorReplacementService service)
    : ICommandHandler<ApplyInstructorReplacementCommand, InstructorReplacementApplyResponse>
{
    public Task<Result<InstructorReplacementApplyResponse>> Handle(ApplyInstructorReplacementCommand command, CancellationToken cancellationToken) =>
        service.ApplyAsync(command.OrganizationId, command.OperationId, command.PreviousInstructorId, command.ReplacementInstructorId,
            command.Mode, command.BookingIds, command.TrainingCategory, command.Reason, command.AccessExpiresAtUtc, cancellationToken);
}
