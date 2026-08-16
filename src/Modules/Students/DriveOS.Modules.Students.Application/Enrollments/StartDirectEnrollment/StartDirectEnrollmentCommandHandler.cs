using DriveOS.Application.Abstractions.Messaging;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Students.Application.Enrollments.StartDirectEnrollment;

public sealed class StartDirectEnrollmentCommandHandler(IDirectEnrollmentService service)
    : ICommandHandler<StartDirectEnrollmentCommand, StartDirectEnrollmentResponse>
{
    public Task<Result<StartDirectEnrollmentResponse>> Handle(
        StartDirectEnrollmentCommand command,
        CancellationToken cancellationToken
    ) => service.StartAsync(command, cancellationToken);
}
