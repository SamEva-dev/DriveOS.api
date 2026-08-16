using System.Data;
using System.Data.Common;
using DriveOS.Application.Abstractions.Time;
using DriveOS.Modules.Students.Application.ExternalTransfers;
using DriveOS.Modules.Students.Domain.ExternalTransfers;
using DriveOS.Modules.Students.Infrastructure.Persistence;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;
using Microsoft.EntityFrameworkCore;

namespace DriveOS.Modules.Students.Infrastructure.ExternalTransfers;

internal sealed class ExternalTransferService(
    StudentsDbContext db,
    IClock clock,
    IExternalTransferPreconditionGateway gateway
) : IExternalTransferService
{
    public async Task<IReadOnlyList<ExternalTransferResponse>> GetAsync(
        GetExternalTransfersQuery q,
        CancellationToken ct = default
    )
    {
        var rows = await db
            .ExternalTransferCases.AsNoTracking()
            .Where(x => x.SourceOrganizationId == q.OrganizationId && x.StudentId == q.StudentId)
            .Include(x => x.DataGrants)
            .Include(x => x.Audit)
            .OrderByDescending(x => x.CreatedAtUtc)
            .ToListAsync(ct);
        var today = DateOnly.FromDateTime(clock.UtcNow.UtcDateTime);
        return rows.Select(x => Map(x, today)).ToArray();
    }

    public async Task<Result<Guid>> CreateAsync(
        CreateExternalTransferCommand c,
        CancellationToken ct = default
    )
    {
        if (
            !await db
                .Students.AsNoTracking()
                .AnyAsync(x => x.OrganizationId == c.OrganizationId && x.Id == c.StudentId, ct)
        )
            return Result.Failure<Guid>(ExternalTransferApplicationErrors.StudentNotFound);
        if (
            await db
                .ExternalTransferCases.AsNoTracking()
                .AnyAsync(
                    x =>
                        x.SourceOrganizationId == c.OrganizationId
                        && x.StudentId == c.StudentId
                        && (
                            x.Status == ExternalTransferStatus.ConsentPending
                            || x.Status == ExternalTransferStatus.TargetReview
                            || x.Status == ExternalTransferStatus.Scheduled
                            || x.Status == ExternalTransferStatus.InProgress
                        ),
                    ct
                )
        )
            return Result.Failure<Guid>(ExternalTransferErrors.ActiveTransferExists);
        var p = await gateway.VerifyAsync(
            c.OrganizationId,
            c.TargetOrganizationId,
            c.CountryCode,
            false,
            ct
        );
        if (!p.TargetOrganizationActive)
            return Result.Failure<Guid>(
                ExternalTransferApplicationErrors.TargetOrganizationNotFound
            );
        if (!p.CountryRuleSatisfied)
            return Result.Failure<Guid>(ExternalTransferErrors.CountryRuleViolation);
        var r = ExternalTransferCase.Create(
            c.OrganizationId,
            c.TargetOrganizationId,
            c.StudentId,
            c.Type,
            c.DataScope,
            c.EffectiveOn,
            c.TemporaryUntil,
            c.CountryCode,
            c.Reason,
            c.Responsibilities,
            c.ActorUserId,
            clock.UtcNow
        );
        if (r.IsFailure)
            return Result.Failure<Guid>(r.Error);
        db.ExternalTransferCases.Add(r.Value);
        await db.SaveChangesAsync(ct);
        return Result.Success(r.Value.Id.Value);
    }

    public Task<Result> VerifyConsentAsync(
        VerifyExternalTransferConsentCommand c,
        CancellationToken ct = default
    ) =>
        Mutate(
            c.OrganizationId,
            c.StudentId,
            c.TransferId,
            x => x.VerifyConsent(c.EvidenceReference, c.ActorUserId, clock.UtcNow),
            ct
        );

    public Task<Result> ReviewFinanceAsync(
        ReviewExternalTransferFinanceCommand c,
        CancellationToken ct = default
    ) =>
        Mutate(
            c.OrganizationId,
            c.StudentId,
            c.TransferId,
            x => x.ReviewFinance(c.Status, c.Resolution, c.ActorUserId, clock.UtcNow),
            ct
        );

    public async Task<Result<ExternalTransferPreconditions>> SubmitAsync(
        SubmitExternalTransferCommand c,
        CancellationToken ct = default
    )
    {
        var x = await Find(c.OrganizationId, c.StudentId, c.TransferId, ct);
        if (x is null)
            return Result.Failure<ExternalTransferPreconditions>(
                ExternalTransferErrors.TransferNotFound
            );
        var p = await gateway.VerifyAsync(
            x.SourceOrganizationId,
            x.TargetOrganizationId,
            x.CountryCode,
            c.RequestInvitationIfMissing,
            ct
        );
        if (!p.TargetOrganizationActive)
            return Result.Failure<ExternalTransferPreconditions>(
                ExternalTransferApplicationErrors.TargetOrganizationNotFound
            );
        if (!p.CountryRuleSatisfied)
            return Result.Failure<ExternalTransferPreconditions>(
                ExternalTransferErrors.CountryRuleViolation
            );
        var r = x.Submit(p.RelationshipStatus, c.ActorUserId, clock.UtcNow);
        if (r.IsFailure)
            return Result.Failure<ExternalTransferPreconditions>(r.Error);
        await db.SaveChangesAsync(ct);
        return Result.Success(p);
    }

    public Task<Result> DecideAsync(
        DecideExternalTransferCommand c,
        CancellationToken ct = default
    ) =>
        Mutate(
            c.OrganizationId,
            c.StudentId,
            c.TransferId,
            x => x.Decide(c.Accept, c.Reason, c.ActorUserId, clock.UtcNow),
            ct
        );

    public Task<Result> CompleteAsync(
        CompleteExternalTransferCommand c,
        CancellationToken ct = default
    ) =>
        Mutate(
            c.OrganizationId,
            c.StudentId,
            c.TransferId,
            x => x.Complete(c.ActorUserId, clock.UtcNow),
            ct
        );

    private async Task<Result> Mutate(
        OrganizationId org,
        PersonId student,
        Guid id,
        Func<ExternalTransferCase, Result> action,
        CancellationToken ct
    )
    {
        var x = await Find(org, student, id, ct);
        if (x is null)
            return Result.Failure(ExternalTransferErrors.TransferNotFound);
        var r = action(x);
        if (r.IsFailure)
            return r;
        await db.SaveChangesAsync(ct);
        return Result.Success();
    }

    private Task<ExternalTransferCase?> Find(
        OrganizationId org,
        PersonId student,
        Guid id,
        CancellationToken ct
    ) =>
        db
            .ExternalTransferCases.Include(x => x.DataGrants)
            .Include(x => x.Audit)
            .SingleOrDefaultAsync(
                x =>
                    x.Id == new ExternalTransferCaseId(id)
                    && x.SourceOrganizationId == org
                    && x.StudentId == student,
                ct
            );

    private static ExternalTransferResponse Map(ExternalTransferCase x, DateOnly today) =>
        new(
            x.Id,
            x.StudentId.Value,
            x.SourceOrganizationId.Value,
            x.TargetOrganizationId.Value,
            x.Type,
            x.DataScope,
            x.EffectiveOn,
            x.TemporaryUntil,
            x.CountryCode,
            x.Reason,
            x.Responsibilities,
            x.Status,
            x.ConsentStatus,
            x.FinancialStatus,
            x.RelationshipStatus,
            x.DataGrants.Select(g => new StudentDataGrantItem(
                    g.Id,
                    g.GranteeOrganizationId.Value,
                    g.Scope,
                    g.GrantedAtUtc,
                    g.ExpiresOn,
                    g.IsActive(today)
                ))
                .ToArray(),
            x.Audit.OrderBy(a => a.OccurredAtUtc)
                .Select(a => new ExternalTransferAuditItem(
                    a.Action,
                    a.Detail,
                    a.ActorUserId.Value,
                    a.OccurredAtUtc
                ))
                .ToArray()
        );
}

internal sealed class ExternalTransferPreconditionGateway(StudentsDbContext db)
    : IExternalTransferPreconditionGateway
{
    public async Task<ExternalTransferPreconditions> VerifyAsync(
        OrganizationId source,
        OrganizationId target,
        string requestedCountry,
        bool requestInvitation,
        CancellationToken ct = default
    )
    {
        var connection = db.Database.GetDbConnection();
        if (connection.State != ConnectionState.Open)
            await connection.OpenAsync(ct);
        var sourceInfo = await Organization(connection, source.Value, ct);
        var targetInfo = await Organization(connection, target.Value, ct);
        bool targetActive =
            targetInfo is not null
            && string.Equals(targetInfo.Value.Status, "Active", StringComparison.OrdinalIgnoreCase);
        bool related = targetActive && await Related(connection, source.Value, target.Value, ct);
        var relationship =
            related ? TargetRelationshipStatus.Active
            : requestInvitation ? TargetRelationshipStatus.InvitationRequested
            : TargetRelationshipStatus.Missing;
        bool country =
            targetInfo is not null
            && string.Equals(
                targetInfo.Value.Country,
                requestedCountry.Trim(),
                StringComparison.OrdinalIgnoreCase
            );
        var warnings = new List<string>();
        if (!related)
            warnings.Add("students.externalTransfer.warnings.relationshipMissing");
        if (!country)
            warnings.Add("students.externalTransfer.warnings.countryRuleMismatch");
        return new(
            relationship,
            targetActive,
            country,
            sourceInfo?.Country ?? string.Empty,
            targetInfo?.Country ?? string.Empty,
            warnings
        );
    }

    private static async Task<(string Country, string Status)?> Organization(
        DbConnection c,
        Guid id,
        CancellationToken ct
    )
    {
        await using var cmd = c.CreateCommand();
        cmd.CommandText =
            "SELECT country_code, status FROM organization.organizations WHERE id = @id";
        var p = cmd.CreateParameter();
        p.ParameterName = "id";
        p.DbType = DbType.Guid;
        p.Value = id;
        cmd.Parameters.Add(p);
        await using var r = await cmd.ExecuteReaderAsync(ct);
        return await r.ReadAsync(ct) ? (r.GetString(0), r.GetString(1)) : null;
    }

    private static async Task<bool> Related(
        DbConnection c,
        Guid source,
        Guid target,
        CancellationToken ct
    )
    {
        await using var cmd = c.CreateCommand();
        cmd.CommandText =
            "WITH source_networks AS (SELECT @source::uuid AS id UNION SELECT network_organization_id FROM organization.network_organization_memberships WHERE member_organization_id=@source AND ended_at_utc IS NULL), target_networks AS (SELECT @target::uuid AS id UNION SELECT network_organization_id FROM organization.network_organization_memberships WHERE member_organization_id=@target AND ended_at_utc IS NULL) SELECT EXISTS (SELECT 1 FROM source_networks s JOIN target_networks t ON t.id=s.id)";
        foreach (var value in new[] { ("source", source), ("target", target) })
        {
            var p = cmd.CreateParameter();
            p.ParameterName = value.Item1;
            p.DbType = DbType.Guid;
            p.Value = value.Item2;
            cmd.Parameters.Add(p);
        }
        return (bool)(await cmd.ExecuteScalarAsync(ct) ?? false);
    }
}
