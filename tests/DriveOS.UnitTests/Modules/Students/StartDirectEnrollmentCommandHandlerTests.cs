using DriveOS.Modules.Students.Application.Enrollments.StartDirectEnrollment;
using DriveOS.Modules.Students.Domain.Enrollments;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;
using FluentAssertions;

namespace DriveOS.UnitTests.Modules.Students;

public sealed class StartDirectEnrollmentCommandHandlerTests
{
    [Fact]
    public async Task Handle_ShouldDelegateTheValidatedCommand()
    {
        var service = new StubService();
        var handler = new StartDirectEnrollmentCommandHandler(service);
        var command = new StartDirectEnrollmentCommand(
            OrganizationId.New(),
            "request-12345",
            null,
            BranchId.New(),
            "Ada",
            "Lovelace",
            "ada@example.test",
            null,
            "PERMIS-B",
            EnrollmentSource.DirectBranch,
            "FR",
            "fr",
            true
        );

        Result<StartDirectEnrollmentResponse> result = await handler.Handle(command, default);

        result.IsSuccess.Should().BeTrue();
        service.LastCommand.Should().Be(command);
    }

    private sealed class StubService : IDirectEnrollmentService
    {
        public StartDirectEnrollmentCommand? LastCommand { get; private set; }

        public Task<Result<StartDirectEnrollmentResponse>> StartAsync(
            StartDirectEnrollmentCommand command,
            CancellationToken cancellationToken = default
        )
        {
            LastCommand = command;
            return Task.FromResult(
                Result.Success(
                    new StartDirectEnrollmentResponse(Guid.NewGuid(), Guid.NewGuid(), false, false)
                )
            );
        }
    }
}
