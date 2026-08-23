using DriveOS.Application.Abstractions.Messaging;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Organizations.Application.InstructorRegulatoryCredentials;

public sealed class GetInstructorRegulatoryCredentialsQueryHandler(IInstructorRegulatoryCredentialService service)
    : IQueryHandler<GetInstructorRegulatoryCredentialsQuery, IReadOnlyList<InstructorRegulatoryCredentialResponse>>
{
    public async Task<Result<IReadOnlyList<InstructorRegulatoryCredentialResponse>>> Handle(GetInstructorRegulatoryCredentialsQuery q, CancellationToken ct)
        => Result.Success(await service.GetAsync(q.OrganizationId, q.InstructorUserId, ct));
}
public sealed class DeclareInstructorRegulatoryCredentialCommandHandler(IInstructorRegulatoryCredentialService service)
    : ICommandHandler<DeclareInstructorRegulatoryCredentialCommand, InstructorRegulatoryCredentialResponse>
{
    public Task<Result<InstructorRegulatoryCredentialResponse>> Handle(DeclareInstructorRegulatoryCredentialCommand c, CancellationToken ct) => service.DeclareAsync(c, ct);
}
public sealed class VerifyInstructorRegulatoryCredentialCommandHandler(IInstructorRegulatoryCredentialService service)
    : ICommandHandler<VerifyInstructorRegulatoryCredentialCommand, InstructorRegulatoryCredentialResponse>
{
    public Task<Result<InstructorRegulatoryCredentialResponse>> Handle(VerifyInstructorRegulatoryCredentialCommand c, CancellationToken ct) => service.VerifyAsync(c, ct);
}
public sealed class RejectInstructorRegulatoryCredentialCommandHandler(IInstructorRegulatoryCredentialService service)
    : ICommandHandler<RejectInstructorRegulatoryCredentialCommand, InstructorRegulatoryCredentialResponse>
{
    public Task<Result<InstructorRegulatoryCredentialResponse>> Handle(RejectInstructorRegulatoryCredentialCommand c, CancellationToken ct) => service.RejectAsync(c, ct);
}
