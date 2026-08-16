using DriveOS.Application.Abstractions.Messaging;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Students.Application.Administration;

public sealed class GetAdministrationQueryHandler(IAdministrationService service)
    : IQueryHandler<GetAdministrationQuery, AdministrationResponse>
{
    public async Task<Result<AdministrationResponse>> Handle(
        GetAdministrationQuery q,
        CancellationToken ct
    )
    {
        var value = await service.GetAsync(q.OrganizationId, q.StudentId, ct);
        return value is null
            ? Result.Failure<AdministrationResponse>(
                AdministrationApplicationErrors.StudentNotFound
            )
            : Result.Success(value);
    }
}

public sealed class ConfigureRequirementCommandHandler(IAdministrationService s)
    : ICommandHandler<ConfigureRequirementCommand, Guid>
{
    public Task<Result<Guid>> Handle(ConfigureRequirementCommand c, CancellationToken ct) =>
        s.ConfigureAsync(c, ct);
}

public sealed class DecideRequirementCommandHandler(IAdministrationService s)
    : ICommandHandler<DecideRequirementCommand>
{
    public Task<Result> Handle(DecideRequirementCommand c, CancellationToken ct) =>
        s.DecideRequirementAsync(c, ct);
}

public sealed class AddAdministrativeBlockCommandHandler(IAdministrationService s)
    : ICommandHandler<AddAdministrativeBlockCommand, Guid>
{
    public Task<Result<Guid>> Handle(AddAdministrativeBlockCommand c, CancellationToken ct) =>
        s.AddBlockAsync(c, ct);
}

public sealed class ReleaseAdministrativeBlockCommandHandler(IAdministrationService s)
    : ICommandHandler<ReleaseAdministrativeBlockCommand>
{
    public Task<Result> Handle(ReleaseAdministrativeBlockCommand c, CancellationToken ct) =>
        s.ReleaseBlockAsync(c, ct);
}

public sealed class RequestComplianceExceptionCommandHandler(IAdministrationService s)
    : ICommandHandler<RequestComplianceExceptionCommand, Guid>
{
    public Task<Result<Guid>> Handle(RequestComplianceExceptionCommand c, CancellationToken ct) =>
        s.RequestExceptionAsync(c, ct);
}

public sealed class DecideComplianceExceptionCommandHandler(IAdministrationService s)
    : ICommandHandler<DecideComplianceExceptionCommand>
{
    public Task<Result> Handle(DecideComplianceExceptionCommand c, CancellationToken ct) =>
        s.DecideExceptionAsync(c, ct);
}

public sealed class SynchronizeAdministrativeRequirementsCommandHandler(IAdministrationService s)
    : ICommandHandler<SynchronizeAdministrativeRequirementsCommand, int>
{
    public Task<Result<int>> Handle(
        SynchronizeAdministrativeRequirementsCommand c,
        CancellationToken ct
    ) => s.SynchronizeRequirementsAsync(c, ct);
}
