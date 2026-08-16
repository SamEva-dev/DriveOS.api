using DriveOS.Application.Abstractions.Messaging;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Students.Application.Students.Identity;

public sealed class GetStudentIdentityQueryHandler(IStudentIdentityService service)
    : IQueryHandler<GetStudentIdentityQuery, StudentIdentityResponse>
{
    public async Task<Result<StudentIdentityResponse>> Handle(
        GetStudentIdentityQuery query,
        CancellationToken cancellationToken
    )
    {
        var value = await service.GetAsync(
            query.OrganizationId,
            query.StudentId,
            cancellationToken
        );
        return value is null
            ? Result.Failure<StudentIdentityResponse>(StudentIdentityErrors.NotFound)
            : Result.Success(value);
    }
}

public sealed class UpdateStudentIdentityCommandHandler(IStudentIdentityService service)
    : ICommandHandler<UpdateStudentIdentityCommand, UpdateStudentIdentityResponse>
{
    public Task<Result<UpdateStudentIdentityResponse>> Handle(
        UpdateStudentIdentityCommand command,
        CancellationToken cancellationToken
    ) => service.UpdateAsync(command, cancellationToken);
}

public sealed class VerifyStudentIdentityCommandHandler(IStudentIdentityService service)
    : ICommandHandler<VerifyStudentIdentityCommand, StudentIdentityResponse>
{
    public Task<Result<StudentIdentityResponse>> Handle(
        VerifyStudentIdentityCommand command,
        CancellationToken cancellationToken
    ) => service.VerifyAsync(command, cancellationToken);
}

public sealed class UpdateOwnStudentContactCommandHandler(IStudentIdentityService service)
    : ICommandHandler<UpdateOwnStudentContactCommand, UpdateStudentIdentityResponse>
{
    public Task<Result<UpdateStudentIdentityResponse>> Handle(
        UpdateOwnStudentContactCommand command,
        CancellationToken cancellationToken
    ) => service.UpdateOwnContactAsync(command, cancellationToken);
}
