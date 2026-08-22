using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Application.Abstractions.Time;
using DriveOS.Modules.ExamsCertification.Application.Persistence;
using DriveOS.Modules.ExamsCertification.Domain.Providers.Connections;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.ExamsCertification.Application.Providers.Connections;

public sealed class CreateExamProviderConnectionCommandHandler(
    IExamProviderConnectionRepository repository,
    IExamsCertificationUnitOfWork unitOfWork,
    IClock clock) : ICommandHandler<CreateExamProviderConnectionCommand, ExamProviderConnectionId>
{
    public async Task<Result<ExamProviderConnectionId>> Handle(CreateExamProviderConnectionCommand command, CancellationToken cancellationToken)
    {
        ExamProviderConnection? existing = await repository.FindByProviderCodeAsync(command.OrganizationId, command.ProviderCode, cancellationToken);
        if (existing is not null) return Result.Success(existing.Id);

        ExamProviderConnectionId id = ExamProviderConnectionId.New();
        Result<ExamProviderConnection> creation = ExamProviderConnection.Create(id, command.OrganizationId, command.ProviderCode,
            command.DisplayName, command.CountryCode, command.Kind, command.AuthenticationMode, command.BaseUrl,
            command.CredentialReference, command.RequestsPerMinute, clock.UtcNow);
        if (creation.IsFailure) return Result.Failure<ExamProviderConnectionId>(creation.Error);

        creation.Value.SetCreatedAudit(clock.UtcNow, command.ActorUserId);
        repository.Add(creation.Value);
        await unitOfWork.CommitAsync(cancellationToken);
        return Result.Success(id);
    }
}

public sealed class GetExamProviderConnectionsQueryHandler(IExamProviderConnectionRepository repository)
    : IQueryHandler<GetExamProviderConnectionsQuery, IReadOnlyList<ExamProviderConnectionResponse>>
{
    public async Task<Result<IReadOnlyList<ExamProviderConnectionResponse>>> Handle(GetExamProviderConnectionsQuery query, CancellationToken cancellationToken)
    {
        IReadOnlyList<ExamProviderConnection> items = await repository.ListAsync(query.OrganizationId, cancellationToken);
        return Result.Success<IReadOnlyList<ExamProviderConnectionResponse>>(items.Select(Map).ToArray());
    }

    private static ExamProviderConnectionResponse Map(ExamProviderConnection x) => new(x.Id.Value, x.ProviderCode, x.DisplayName,
        x.CountryCode, x.Kind.ToString(), x.AuthenticationMode.ToString(), x.BaseUrl, !string.IsNullOrWhiteSpace(x.CredentialReference),
        x.RequestsPerMinute, x.Status.ToString(), x.LastTestedAtUtc, x.LastSuccessfulAtUtc, x.LastErrorCode,
        x.ConsecutiveFailureCount);
}

public sealed class GetExamProviderCatalogQueryHandler(
    IExamPlaceProviderResolver resolver,
    IExamProviderConnectionRepository repository) : IQueryHandler<GetExamProviderCatalogQuery, IReadOnlyList<ExamProviderCatalogResponse>>
{
    public async Task<Result<IReadOnlyList<ExamProviderCatalogResponse>>> Handle(GetExamProviderCatalogQuery query, CancellationToken cancellationToken)
    {
        IReadOnlyList<ExamProviderConnection> connections = await repository.ListAsync(query.OrganizationId, cancellationToken);
        Dictionary<string, ExamProviderConnection> byCode = connections.ToDictionary(x => x.ProviderCode, StringComparer.OrdinalIgnoreCase);
        var result = resolver.GetAvailableProviders().Select(x =>
        {
            byCode.TryGetValue(x.Code, out ExamProviderConnection? connection);
            return new ExamProviderCatalogResponse(x.Code, x.CountryCode, x.Kind.ToString(), (long)x.Capabilities,
                x.IsEnabled, connection is not null, connection?.Status.ToString());
        }).ToList();

        foreach (ExamProviderConnection connection in connections.Where(c => result.All(r => !string.Equals(r.Code, c.ProviderCode, StringComparison.OrdinalIgnoreCase))))
            result.Add(new ExamProviderCatalogResponse(connection.ProviderCode, connection.CountryCode, connection.Kind.ToString(), 0, false, true, connection.Status.ToString()));

        return Result.Success<IReadOnlyList<ExamProviderCatalogResponse>>(result.OrderBy(x => x.Code).ToArray());
    }
}

public sealed class TestExamProviderConnectionCommandHandler(
    IExamProviderConnectionRepository repository,
    IExamProviderConnectionTester tester,
    IExamsCertificationUnitOfWork unitOfWork,
    IClock clock) : ICommandHandler<TestExamProviderConnectionCommand, ExamProviderConnectionTestResponse>
{
    public async Task<Result<ExamProviderConnectionTestResponse>> Handle(TestExamProviderConnectionCommand command, CancellationToken cancellationToken)
    {
        ExamProviderConnection? connection = await repository.GetByIdForUpdateAsync(command.OrganizationId, command.ConnectionId, cancellationToken);
        if (connection is null) return Result.Failure<ExamProviderConnectionTestResponse>(ExamProviderConnectionErrors.NotFound);

        ExamProviderConnectionTestResult test = await tester.TestAsync(command.OrganizationId, connection.ProviderCode, cancellationToken);
        if (test.Success) connection.RecordConnectionSuccess(command.ActorUserId, clock.UtcNow);
        else connection.RecordConnectionFailure(test.ErrorCode ?? "Exams.ProviderConnection.AdapterUnavailable", command.ActorUserId, clock.UtcNow);
        await unitOfWork.CommitAsync(cancellationToken);

        return Result.Success(new ExamProviderConnectionTestResponse(test.Success, connection.ProviderCode, test.ErrorCode,
            clock.UtcNow, test.Capabilities));
    }
}

public sealed class SuspendExamProviderConnectionCommandHandler(IExamProviderConnectionRepository repository,
    IExamsCertificationUnitOfWork unitOfWork, IClock clock) : ICommandHandler<SuspendExamProviderConnectionCommand>
{
    public async Task<Result> Handle(SuspendExamProviderConnectionCommand command, CancellationToken cancellationToken)
    {
        ExamProviderConnection? connection = await repository.GetByIdForUpdateAsync(command.OrganizationId, command.ConnectionId, cancellationToken);
        if (connection is null) return Result.Failure(ExamProviderConnectionErrors.NotFound);
        Result result = connection.Suspend(command.ActorUserId, clock.UtcNow);
        if (result.IsFailure) return result;
        await unitOfWork.CommitAsync(cancellationToken);
        return Result.Success();
    }
}

public sealed class RevokeExamProviderConnectionCommandHandler(IExamProviderConnectionRepository repository,
    IExamsCertificationUnitOfWork unitOfWork, IClock clock) : ICommandHandler<RevokeExamProviderConnectionCommand>
{
    public async Task<Result> Handle(RevokeExamProviderConnectionCommand command, CancellationToken cancellationToken)
    {
        ExamProviderConnection? connection = await repository.GetByIdForUpdateAsync(command.OrganizationId, command.ConnectionId, cancellationToken);
        if (connection is null) return Result.Failure(ExamProviderConnectionErrors.NotFound);
        connection.Revoke(command.ActorUserId, clock.UtcNow);
        await unitOfWork.CommitAsync(cancellationToken);
        return Result.Success();
    }
}
