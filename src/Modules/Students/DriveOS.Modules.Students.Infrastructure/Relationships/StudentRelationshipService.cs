using DriveOS.Application.Abstractions.Time;
using DriveOS.Modules.Students.Application.Relationships;
using DriveOS.Modules.Students.Domain.Relationships;
using DriveOS.Modules.Students.Infrastructure.Persistence;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;
using Microsoft.EntityFrameworkCore;

namespace DriveOS.Modules.Students.Infrastructure.Relationships;

internal sealed class StudentRelationshipService(StudentsDbContext db, IClock clock)
    : IStudentRelationshipService
{
    public async Task<StudentRelationshipListResponse?> GetAsync(
        OrganizationId org,
        PersonId studentId,
        CancellationToken ct = default
    )
    {
        if (!await StudentExists(org, studentId, ct))
            return null;
        DateOnly today = DateOnly.FromDateTime(clock.UtcNow.UtcDateTime);
        var rows = await db
            .StudentRelationships.AsNoTracking()
            .Where(x => x.OrganizationId == org && x.StudentId == studentId)
            .OrderByDescending(x => x.IsPrimaryPayer)
            .ThenBy(x => x.DisplayName)
            .ToListAsync(ct);
        return new(studentId.Value, rows.Select(x => Map(x, today)).ToArray());
    }

    public async Task<Result<Guid>> CreateAsync(
        CreateStudentRelationshipCommand x,
        CancellationToken ct = default
    )
    {
        if (!await StudentExists(x.OrganizationId, x.StudentId, ct))
            return Result.Failure<Guid>(StudentRelationshipApplicationErrors.StudentNotFound);
        if (x.IsPrimaryPayer && !x.CanManagePayers)
            return Result.Failure<Guid>(
                StudentRelationshipApplicationErrors.PayerManagementForbidden
            );
        if (
            await db
                .StudentRelationships.AsNoTracking()
                .AnyAsync(
                    r =>
                        r.OrganizationId == x.OrganizationId
                        && r.StudentId == x.StudentId
                        && r.PersonOrOrganizationId == x.PartyId
                        && r.RelationshipType == x.RelationshipType
                        && r.Status != StudentRelationshipStatus.Revoked,
                    ct
                )
        )
            return Result.Failure<Guid>(StudentRelationshipApplicationErrors.Duplicate);
        var result = StudentRelationship.Create(
            x.OrganizationId,
            x.StudentId,
            x.PartyId,
            x.PartyKind,
            x.DisplayName,
            x.Email,
            x.Phone,
            x.RelationshipType,
            x.Permissions,
            x.FinancialScope,
            x.CommunicationScope,
            x.EffectiveFrom,
            x.EffectiveTo,
            x.IsPrimaryPayer,
            x.ActorUserId,
            clock.UtcNow
        );
        if (result.IsFailure)
            return Result.Failure<Guid>(result.Error);
        if (
            x.IsPrimaryPayer
            && await ClearPrimary(x.OrganizationId, x.StudentId, null, x.ActorUserId, ct)
        )
            await db.SaveChangesAsync(ct);
        db.StudentRelationships.Add(result.Value);
        await db.SaveChangesAsync(ct);
        return Result.Success(result.Value.Id.Value);
    }

    public async Task<Result> UpdateAsync(
        UpdateStudentRelationshipCommand x,
        CancellationToken ct = default
    )
    {
        var entity = await Find(x.OrganizationId, x.StudentId, x.RelationshipId, ct);
        if (entity is null)
            return Result.Failure(StudentRelationshipErrors.NotFound);
        if (entity.IsPrimaryPayer != x.IsPrimaryPayer && !x.CanManagePayers)
            return Result.Failure(StudentRelationshipApplicationErrors.PayerManagementForbidden);
        if (
            x.IsPrimaryPayer
            && !entity.IsPrimaryPayer
            && await ClearPrimary(
                x.OrganizationId,
                x.StudentId,
                x.RelationshipId,
                x.ActorUserId,
                ct
            )
        )
            await db.SaveChangesAsync(ct);
        var result = entity.Update(
            x.RelationshipType,
            x.Permissions,
            x.FinancialScope,
            x.CommunicationScope,
            x.EffectiveFrom,
            x.EffectiveTo,
            x.IsPrimaryPayer,
            x.ActorUserId,
            clock.UtcNow
        );
        if (result.IsFailure)
            return result;
        await db.SaveChangesAsync(ct);
        return result;
    }

    public async Task<Result> SuspendAsync(
        SuspendStudentRelationshipCommand x,
        CancellationToken ct = default
    ) =>
        await Change(
            x.OrganizationId,
            x.StudentId,
            x.RelationshipId,
            e => e.Suspend(x.Reason, x.ActorUserId, clock.UtcNow),
            ct
        );

    public async Task<Result> RevokeAsync(
        RevokeStudentRelationshipCommand x,
        CancellationToken ct = default
    ) =>
        await Change(
            x.OrganizationId,
            x.StudentId,
            x.RelationshipId,
            e => e.Revoke(x.Reason, x.ActorUserId, clock.UtcNow),
            ct
        );

    public async Task<Result> InviteAsync(
        InviteStudentRelationshipCommand x,
        CancellationToken ct = default
    ) =>
        await Change(
            x.OrganizationId,
            x.StudentId,
            x.RelationshipId,
            e => e.Invite(x.ActorUserId, clock.UtcNow),
            ct
        );

    private async Task<Result> Change(
        OrganizationId org,
        PersonId studentId,
        Guid id,
        Func<StudentRelationship, Result> action,
        CancellationToken ct
    )
    {
        var e = await Find(org, studentId, id, ct);
        if (e is null)
            return Result.Failure(StudentRelationshipErrors.NotFound);
        var r = action(e);
        if (r.IsSuccess)
            await db.SaveChangesAsync(ct);
        return r;
    }

    private Task<StudentRelationship?> Find(
        OrganizationId org,
        PersonId studentId,
        Guid id,
        CancellationToken ct
    ) =>
        db.StudentRelationships.SingleOrDefaultAsync(
            x => x.OrganizationId == org && x.StudentId == studentId && x.Id == new StudentRelationshipId(id),
            ct
        );

    private Task<bool> StudentExists(
        OrganizationId org,
        PersonId studentId,
        CancellationToken ct
    ) => db.Students.AsNoTracking().AnyAsync(x => x.OrganizationId == org && x.Id == studentId, ct);

    private async Task<bool> ClearPrimary(
        OrganizationId org,
        PersonId studentId,
        Guid? except,
        UserId actor,
        CancellationToken ct
    )
    {
        var items = await db
            .StudentRelationships.Where(x =>
                x.OrganizationId == org
                && x.StudentId == studentId
                && x.IsPrimaryPayer
                && (!except.HasValue || x.Id != new StudentRelationshipId(except.Value))
            )
            .ToListAsync(ct);
        foreach (var item in items)
            item.ClearPrimaryPayer(actor, clock.UtcNow);
        return items.Count > 0;
    }

    private static StudentRelationshipItem Map(StudentRelationship x, DateOnly today) =>
        new(
            x.Id,
            x.PersonOrOrganizationId,
            x.PartyKind,
            x.DisplayName,
            x.Email,
            x.Phone,
            x.RelationshipType,
            x.Permissions,
            x.FinancialScope,
            x.CommunicationScope,
            x.EffectiveFrom,
            x.EffectiveTo,
            x.IsPrimaryPayer,
            x.Status == StudentRelationshipStatus.Active
            && x.EffectiveTo.HasValue
            && x.EffectiveTo < today
                ? StudentRelationshipStatus.Expired
                : x.Status,
            x.InvitedAtUtc,
            x.StatusReason
        );
}
