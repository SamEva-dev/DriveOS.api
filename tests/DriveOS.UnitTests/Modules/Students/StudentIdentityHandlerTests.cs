using DriveOS.Modules.Students.Application.Students.Identity;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;
using FluentAssertions;

namespace DriveOS.UnitTests.Modules.Students;

public sealed class StudentIdentityHandlerTests
{
    [Fact]
    public async Task Get_ShouldReturnStableNotFoundError()
    {
        var handler = new GetStudentIdentityQueryHandler(new MissingIdentityService());
        var result = await handler.Handle(
            new GetStudentIdentityQuery(OrganizationId.New(), PersonId.New()),
            default
        );
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(StudentIdentityErrors.NotFound);
    }

    private sealed class MissingIdentityService : IStudentIdentityService
    {
        public Task<StudentIdentityResponse?> GetAsync(
            OrganizationId organizationId,
            PersonId studentId,
            CancellationToken cancellationToken = default
        ) => Task.FromResult<StudentIdentityResponse?>(null);

        public Task<Result<UpdateStudentIdentityResponse>> UpdateAsync(
            UpdateStudentIdentityCommand command,
            CancellationToken cancellationToken = default
        ) => throw new NotSupportedException();

        public Task<Result<StudentIdentityResponse>> VerifyAsync(
            VerifyStudentIdentityCommand command,
            CancellationToken cancellationToken = default
        ) => throw new NotSupportedException();

        public Task<Result<UpdateStudentIdentityResponse>> UpdateOwnContactAsync(
            UpdateOwnStudentContactCommand command,
            CancellationToken cancellationToken = default
        ) => throw new NotSupportedException();
    }
}
