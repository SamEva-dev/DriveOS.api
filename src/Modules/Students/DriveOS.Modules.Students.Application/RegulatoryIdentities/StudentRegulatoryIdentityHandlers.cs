using DriveOS.Application.Abstractions.Messaging;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Students.Application.RegulatoryIdentities;

public sealed class GetStudentRegulatoryIdentitiesQueryHandler(IStudentRegulatoryIdentityService service)
    : IQueryHandler<GetStudentRegulatoryIdentitiesQuery, IReadOnlyList<StudentRegulatoryIdentityResponse>>
{
    public async Task<Result<IReadOnlyList<StudentRegulatoryIdentityResponse>>> Handle(
        GetStudentRegulatoryIdentitiesQuery query,
        CancellationToken cancellationToken) =>
        Result.Success(await service.GetAsync(query.OrganizationId, query.StudentId, cancellationToken));
}

public sealed class DeclareStudentRegulatoryIdentityCommandHandler(IStudentRegulatoryIdentityService service)
    : ICommandHandler<DeclareStudentRegulatoryIdentityCommand, StudentRegulatoryIdentityResponse>
{
    public Task<Result<StudentRegulatoryIdentityResponse>> Handle(
        DeclareStudentRegulatoryIdentityCommand command,
        CancellationToken cancellationToken) => service.DeclareAsync(command, cancellationToken);
}

public sealed class VerifyStudentRegulatoryIdentityCommandHandler(IStudentRegulatoryIdentityService service)
    : ICommandHandler<VerifyStudentRegulatoryIdentityCommand, StudentRegulatoryIdentityResponse>
{
    public Task<Result<StudentRegulatoryIdentityResponse>> Handle(
        VerifyStudentRegulatoryIdentityCommand command,
        CancellationToken cancellationToken) => service.VerifyAsync(command, cancellationToken);
}

public sealed class RejectStudentRegulatoryIdentityCommandHandler(IStudentRegulatoryIdentityService service)
    : ICommandHandler<RejectStudentRegulatoryIdentityCommand, StudentRegulatoryIdentityResponse>
{
    public Task<Result<StudentRegulatoryIdentityResponse>> Handle(
        RejectStudentRegulatoryIdentityCommand command,
        CancellationToken cancellationToken) => service.RejectAsync(command, cancellationToken);
}
