using DriveOS.Application.Abstractions.Time;
using DriveOS.Modules.Students.Application.RegulatoryIdentities;
using DriveOS.Modules.Students.Domain.RegulatoryIdentities;
using DriveOS.Modules.Students.Infrastructure.Persistence;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;
using Microsoft.EntityFrameworkCore;

namespace DriveOS.Modules.Students.Infrastructure.RegulatoryIdentities;

internal sealed class StudentRegulatoryIdentityService(StudentsDbContext db, IClock clock)
    : IStudentRegulatoryIdentityService, IStudentRegulatoryIdentityReadService
{
    public async Task<IReadOnlyList<StudentRegulatoryIdentityResponse>> GetAsync(
        OrganizationId organizationId,
        PersonId studentId,
        CancellationToken cancellationToken = default)
    {
        return await db.StudentRegulatoryIdentities.AsNoTracking()
            .Where(x => x.OrganizationId == organizationId && x.StudentId == studentId)
            .OrderByDescending(x => x.DeclaredAtUtc)
            .Select(x => new StudentRegulatoryIdentityResponse(
                x.Id.Value, x.StudentId.Value, x.CountryCode, x.IdentifierType, x.Value,
                x.Source, x.Status, x.DeclaredAtUtc, x.VerifiedAtUtc, x.VerificationMethod,
                x.DecisionReason, x.SupersededAtUtc,
                x.SupersededById.HasValue ? x.SupersededById.Value.Value : null))
            .ToListAsync(cancellationToken);
    }

    public async Task<StudentRegulatoryIdentifierSnapshot?> ResolveCurrentAsync(
        OrganizationId organizationId,
        PersonId studentId,
        string countryCode,
        string identifierType,
        CancellationToken cancellationToken = default)
    {
        string country = StudentRegulatoryIdentity.NormalizeCountry(countryCode);
        string type = StudentRegulatoryIdentity.NormalizeToken(identifierType);
        return await db.StudentRegulatoryIdentities.AsNoTracking()
            .Where(x => x.OrganizationId == organizationId && x.StudentId == studentId
                && x.CountryCode == country && x.IdentifierType == type
                && (x.Status == StudentRegulatoryIdentityStatus.Declared || x.Status == StudentRegulatoryIdentityStatus.Verified))
            .Select(x => new StudentRegulatoryIdentifierSnapshot(
                x.CountryCode, x.IdentifierType, x.Value,
                x.Status == StudentRegulatoryIdentityStatus.Verified,
                x.DeclaredAtUtc, x.VerifiedAtUtc))
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<Result<StudentRegulatoryIdentityResponse>> DeclareAsync(
        DeclareStudentRegulatoryIdentityCommand command,
        CancellationToken cancellationToken = default)
    {
        bool studentExists = await db.Students.AsNoTracking().AnyAsync(
            x => x.OrganizationId == command.OrganizationId && x.Id == command.StudentId,
            cancellationToken);
        if (!studentExists)
            return Result.Failure<StudentRegulatoryIdentityResponse>(StudentRegulatoryIdentityErrors.StudentNotFound);

        string country = StudentRegulatoryIdentity.NormalizeCountry(command.CountryCode);
        string type = StudentRegulatoryIdentity.NormalizeToken(command.IdentifierType);
        string value = StudentRegulatoryIdentity.NormalizeValue(command.Value);

        StudentRegulatoryIdentity? current = await db.StudentRegulatoryIdentities
            .SingleOrDefaultAsync(x => x.OrganizationId == command.OrganizationId
                && x.StudentId == command.StudentId
                && x.CountryCode == country
                && x.IdentifierType == type
                && (x.Status == StudentRegulatoryIdentityStatus.Declared || x.Status == StudentRegulatoryIdentityStatus.Verified),
                cancellationToken);

        if (current is not null && current.Value == value)
            return Result.Success(Map(current));

        Result<StudentRegulatoryIdentity> create = StudentRegulatoryIdentity.Declare(
            command.OrganizationId, command.StudentId, country, type, value,
            command.Source, command.ActorUserId, clock.UtcNow);
        if (create.IsFailure)
            return Result.Failure<StudentRegulatoryIdentityResponse>(create.Error);

        StudentRegulatoryIdentity replacement = create.Value;
        if (current is not null)
        {
            Result supersede = current.Supersede(replacement.Id, command.ActorUserId, clock.UtcNow);
            if (supersede.IsFailure)
                return Result.Failure<StudentRegulatoryIdentityResponse>(supersede.Error);
        }

        db.StudentRegulatoryIdentities.Add(replacement);
        await db.SaveChangesAsync(cancellationToken);
        return Result.Success(Map(replacement));
    }

    public async Task<Result<StudentRegulatoryIdentityResponse>> VerifyAsync(
        VerifyStudentRegulatoryIdentityCommand command,
        CancellationToken cancellationToken = default)
    {
        StudentRegulatoryIdentity? identity = await FindTracked(command.OrganizationId, command.StudentId, command.IdentityId, cancellationToken);
        if (identity is null)
            return Result.Failure<StudentRegulatoryIdentityResponse>(StudentRegulatoryIdentityErrors.NotFound);
        Result result = identity.Verify(command.VerificationMethod, command.Reason, command.ActorUserId, clock.UtcNow);
        if (result.IsFailure)
            return Result.Failure<StudentRegulatoryIdentityResponse>(result.Error);
        await db.SaveChangesAsync(cancellationToken);
        return Result.Success(Map(identity));
    }

    public async Task<Result<StudentRegulatoryIdentityResponse>> RejectAsync(
        RejectStudentRegulatoryIdentityCommand command,
        CancellationToken cancellationToken = default)
    {
        StudentRegulatoryIdentity? identity = await FindTracked(command.OrganizationId, command.StudentId, command.IdentityId, cancellationToken);
        if (identity is null)
            return Result.Failure<StudentRegulatoryIdentityResponse>(StudentRegulatoryIdentityErrors.NotFound);
        Result result = identity.Reject(command.Reason, command.ActorUserId, clock.UtcNow);
        if (result.IsFailure)
            return Result.Failure<StudentRegulatoryIdentityResponse>(result.Error);
        await db.SaveChangesAsync(cancellationToken);
        return Result.Success(Map(identity));
    }

    private Task<StudentRegulatoryIdentity?> FindTracked(
        OrganizationId organizationId, PersonId studentId, StudentRegulatoryIdentityId id,
        CancellationToken cancellationToken) => db.StudentRegulatoryIdentities.SingleOrDefaultAsync(
            x => x.OrganizationId == organizationId && x.StudentId == studentId && x.Id == id,
            cancellationToken);

    private static StudentRegulatoryIdentityResponse Map(StudentRegulatoryIdentity x) => new(
        x.Id.Value, x.StudentId.Value, x.CountryCode, x.IdentifierType, x.Value, x.Source, x.Status,
        x.DeclaredAtUtc, x.VerifiedAtUtc, x.VerificationMethod, x.DecisionReason, x.SupersededAtUtc,
        x.SupersededById.HasValue ? x.SupersededById.Value.Value : null);
}
