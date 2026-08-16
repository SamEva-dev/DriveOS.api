using DriveOS.Application.Abstractions.Time;
using DriveOS.Modules.Students.Application.Guardians;
using DriveOS.Modules.Students.Domain.Guardians;
using DriveOS.Modules.Students.Infrastructure.Persistence;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;
using Microsoft.EntityFrameworkCore;

namespace DriveOS.Modules.Students.Infrastructure.Guardians;

internal sealed class GuardianService(StudentsDbContext db, IClock clock) : IGuardianService
{
    public async Task<GuardianListResponse?> GetAsync(
        OrganizationId org,
        PersonId studentId,
        CancellationToken ct = default
    )
    {
        var student = await db
            .Students.AsNoTracking()
            .SingleOrDefaultAsync(x => x.OrganizationId == org && x.Id == studentId, ct);
        if (student is null)
            return null;
        var birthDate = student.BirthDate;
        DateOnly today = DateOnly.FromDateTime(clock.UtcNow.UtcDateTime);
        var rows = await db
            .GuardianRelationships.AsNoTracking()
            .Where(x => x.OrganizationId == org && x.StudentId == studentId)
            .OrderBy(x => x.GuardianLastName)
            .ThenBy(x => x.GuardianFirstName)
            .ToListAsync(ct);
        bool review =
            birthDate.HasValue
            && birthDate.Value.AddYears(18) <= today
            && rows.Any(x => x.Status != GuardianRelationshipStatus.Revoked);
        return new GuardianListResponse(
            studentId.Value,
            review,
            rows.Select(x => Map(x, today)).ToArray()
        );
    }

    public async Task<Result<Guid>> CreateAsync(
        CreateGuardianCommand x,
        CancellationToken ct = default
    )
    {
        if (!await StudentExists(x.OrganizationId, x.StudentId, ct))
            return Result.Failure<Guid>(GuardianApplicationErrors.StudentNotFound);
        bool duplicate = await db
            .GuardianRelationships.AsNoTracking()
            .AnyAsync(
                g =>
                    g.OrganizationId == x.OrganizationId
                    && g.StudentId == x.StudentId
                    && g.GuardianPersonId == x.GuardianPersonId
                    && g.Status != GuardianRelationshipStatus.Revoked,
                ct
            );
        if (duplicate)
            return Result.Failure<Guid>(GuardianApplicationErrors.Duplicate);
        var result = GuardianRelationship.Create(
            x.OrganizationId,
            x.StudentId,
            x.GuardianPersonId,
            x.FirstName,
            x.LastName,
            x.Email,
            x.Phone,
            x.RelationshipType,
            x.LegalBasis,
            x.ParentalAuthorityStatus,
            x.Permissions,
            x.EffectiveFrom,
            x.EffectiveTo,
            x.FinancialRights,
            x.SignatureRights,
            x.NotificationPreferences,
            x.ActorUserId,
            clock.UtcNow
        );
        if (result.IsFailure)
            return Result.Failure<Guid>(result.Error);
        db.GuardianRelationships.Add(result.Value);
        await db.SaveChangesAsync(ct);
        return Result.Success(result.Value.Id.Value);
    }

    public async Task<Result> UpdateAsync(UpdateGuardianCommand x, CancellationToken ct = default)
    {
        var entity = await FindTracked(x.OrganizationId, x.StudentId, x.RelationshipId, ct);
        if (entity is null)
            return Result.Failure(GuardianErrors.NotFound);
        var result = entity.Update(
            x.RelationshipType,
            x.LegalBasis,
            x.ParentalAuthorityStatus,
            x.Permissions,
            x.EffectiveFrom,
            x.EffectiveTo,
            x.FinancialRights,
            x.SignatureRights,
            x.NotificationPreferences,
            x.ActorUserId,
            clock.UtcNow
        );
        if (result.IsSuccess)
            await db.SaveChangesAsync(ct);
        return result;
    }

    public async Task<Result> RevokeAsync(RevokeGuardianCommand x, CancellationToken ct = default)
    {
        var entity = await FindTracked(x.OrganizationId, x.StudentId, x.RelationshipId, ct);
        if (entity is null)
            return Result.Failure(GuardianErrors.NotFound);
        var result = entity.Revoke(x.Reason, x.ActorUserId, clock.UtcNow);
        if (result.IsSuccess)
            await db.SaveChangesAsync(ct);
        return result;
    }

    public async Task<Result> InviteAsync(InviteGuardianCommand x, CancellationToken ct = default)
    {
        var entity = await FindTracked(x.OrganizationId, x.StudentId, x.RelationshipId, ct);
        if (entity is null)
            return Result.Failure(GuardianErrors.NotFound);
        var result = entity.Invite(x.ActorUserId, clock.UtcNow);
        if (result.IsSuccess)
            await db.SaveChangesAsync(ct);
        return result;
    }

    private Task<bool> StudentExists(
        OrganizationId org,
        PersonId studentId,
        CancellationToken ct
    ) => db.Students.AsNoTracking().AnyAsync(x => x.OrganizationId == org && x.Id == studentId, ct);

    private Task<GuardianRelationship?> FindTracked(
        OrganizationId org,
        PersonId studentId,
        Guid id,
        CancellationToken ct
    ) =>
        db.GuardianRelationships.SingleOrDefaultAsync(
            x => x.OrganizationId == org && x.StudentId == studentId && x.Id == new GuardianRelationshipId(id),
            ct
        );

    private static GuardianItem Map(GuardianRelationship x, DateOnly today) =>
        new(
            x.Id,
            x.GuardianPersonId.Value,
            x.GuardianFirstName,
            x.GuardianLastName,
            x.GuardianEmail,
            x.GuardianPhone,
            x.RelationshipType,
            x.LegalBasis,
            x.ParentalAuthorityStatus,
            x.Permissions,
            x.EffectiveFrom,
            x.EffectiveTo,
            x.FinancialRights,
            x.SignatureRights,
            x.NotificationPreferences,
            x.Status == GuardianRelationshipStatus.Active
            && x.EffectiveTo.HasValue
            && x.EffectiveTo < today
                ? GuardianRelationshipStatus.Expired
                : x.Status,
            x.InvitedAtUtc
        );
}
