using DriveOS.Application.Abstractions.Time;
using DriveOS.Modules.Organizations.Application.InstructorRegulatoryCredentials;
using DriveOS.Modules.Organizations.Domain.BranchAssignments;
using DriveOS.Modules.Organizations.Domain.InstructorRegulatoryCredentials;
using DriveOS.Modules.Organizations.Infrastructure.Persistence;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;
using Microsoft.EntityFrameworkCore;

namespace DriveOS.Modules.Organizations.Infrastructure.InstructorRegulatoryCredentials;

internal sealed class InstructorRegulatoryCredentialService(OrganizationsDbContext db, IClock clock)
    : IInstructorRegulatoryCredentialService, IInstructorRegulatoryCredentialReadService
{
    public async Task<IReadOnlyList<InstructorRegulatoryCredentialResponse>> GetAsync(OrganizationId org, UserId instructor, CancellationToken ct = default)
        => await db.InstructorRegulatoryCredentials.AsNoTracking().Where(x => x.OrganizationId == org && x.InstructorUserId == instructor)
            .OrderByDescending(x => x.DeclaredAtUtc)
            .Select(x => new InstructorRegulatoryCredentialResponse(x.Id.Value, x.InstructorUserId.Value, x.CountryCode, x.CredentialType,
                x.Identifier, x.IssuingAuthority, x.JurisdictionCode, x.IssuedOn, x.ExpiresOn, x.Source, x.Status, x.DeclaredAtUtc, x.VerifiedAtUtc,
                x.VerificationMethod, x.DecisionReason, x.SupersededAtUtc, x.SupersededById.HasValue ? x.SupersededById.Value.Value : null))
            .ToListAsync(ct);

    public async Task<InstructorRegulatoryCredentialSnapshot?> ResolveCurrentAsync(OrganizationId org, UserId instructor, string countryCode, string credentialType, CancellationToken ct = default)
    {
        string country = InstructorRegulatoryCredential.NormalizeToken(countryCode);
        string type = InstructorRegulatoryCredential.NormalizeToken(credentialType);
        return await db.InstructorRegulatoryCredentials.AsNoTracking()
            .Where(x => x.OrganizationId == org && x.InstructorUserId == instructor && x.CountryCode == country && x.CredentialType == type
                && (x.Status == InstructorRegulatoryCredentialStatus.Declared || x.Status == InstructorRegulatoryCredentialStatus.Verified))
            .Select(x => new InstructorRegulatoryCredentialSnapshot(x.CountryCode, x.CredentialType, x.Identifier, x.IssuingAuthority,
                x.JurisdictionCode, x.IssuedOn, x.ExpiresOn, x.Status == InstructorRegulatoryCredentialStatus.Verified))
            .SingleOrDefaultAsync(ct);
    }

    public async Task<Result<InstructorRegulatoryCredentialResponse>> DeclareAsync(DeclareInstructorRegulatoryCredentialCommand c, CancellationToken ct = default)
    {
        bool assigned = await db.BranchUserAssignments.AsNoTracking().AnyAsync(x => x.OrganizationId == c.OrganizationId && x.UserId == c.InstructorUserId
            && x.Role == BranchAssignmentRole.Instructor && x.Status != BranchUserAssignmentStatus.Ended, ct);
        if (!assigned) return Result.Failure<InstructorRegulatoryCredentialResponse>(InstructorRegulatoryCredentialErrors.InstructorNotAssigned);

        string country = InstructorRegulatoryCredential.NormalizeToken(c.CountryCode);
        string type = InstructorRegulatoryCredential.NormalizeToken(c.CredentialType);
        string identifier = InstructorRegulatoryCredential.NormalizeIdentifier(c.Identifier);
        InstructorRegulatoryCredential? current = await db.InstructorRegulatoryCredentials.SingleOrDefaultAsync(x => x.OrganizationId == c.OrganizationId
            && x.InstructorUserId == c.InstructorUserId && x.CountryCode == country && x.CredentialType == type
            && (x.Status == InstructorRegulatoryCredentialStatus.Declared || x.Status == InstructorRegulatoryCredentialStatus.Verified), ct);
        if (current is not null && current.Identifier == identifier) return Result.Success(Map(current));

        Result<InstructorRegulatoryCredential> create = InstructorRegulatoryCredential.Declare(c.OrganizationId, c.InstructorUserId, country, type,
            identifier, c.IssuingAuthority, c.JurisdictionCode, c.IssuedOn, c.ExpiresOn, c.Source, c.ActorUserId, clock.UtcNow);
        if (create.IsFailure) return Result.Failure<InstructorRegulatoryCredentialResponse>(create.Error);
        if (current is not null) { Result supersede = current.Supersede(create.Value.Id, clock.UtcNow); if (supersede.IsFailure) return Result.Failure<InstructorRegulatoryCredentialResponse>(supersede.Error); }
        db.InstructorRegulatoryCredentials.Add(create.Value);
        await db.SaveChangesAsync(ct);
        return Result.Success(Map(create.Value));
    }

    public async Task<Result<InstructorRegulatoryCredentialResponse>> VerifyAsync(VerifyInstructorRegulatoryCredentialCommand c, CancellationToken ct = default)
    {
        var x = await Find(c.OrganizationId, c.InstructorUserId, c.CredentialId, ct); if (x is null) return Result.Failure<InstructorRegulatoryCredentialResponse>(InstructorRegulatoryCredentialErrors.NotFound);
        Result r = x.Verify(c.VerificationMethod, c.Reason, c.ActorUserId, clock.UtcNow); if (r.IsFailure) return Result.Failure<InstructorRegulatoryCredentialResponse>(r.Error);
        await db.SaveChangesAsync(ct); return Result.Success(Map(x));
    }
    public async Task<Result<InstructorRegulatoryCredentialResponse>> RejectAsync(RejectInstructorRegulatoryCredentialCommand c, CancellationToken ct = default)
    {
        var x = await Find(c.OrganizationId, c.InstructorUserId, c.CredentialId, ct); if (x is null) return Result.Failure<InstructorRegulatoryCredentialResponse>(InstructorRegulatoryCredentialErrors.NotFound);
        Result r = x.Reject(c.Reason, c.ActorUserId, clock.UtcNow); if (r.IsFailure) return Result.Failure<InstructorRegulatoryCredentialResponse>(r.Error);
        await db.SaveChangesAsync(ct); return Result.Success(Map(x));
    }
    private Task<InstructorRegulatoryCredential?> Find(OrganizationId org, UserId user, InstructorRegulatoryCredentialId id, CancellationToken ct)
        => db.InstructorRegulatoryCredentials.SingleOrDefaultAsync(x => x.OrganizationId == org && x.InstructorUserId == user && x.Id == id, ct);
    private static InstructorRegulatoryCredentialResponse Map(InstructorRegulatoryCredential x) => new(x.Id.Value, x.InstructorUserId.Value, x.CountryCode, x.CredentialType,
        x.Identifier, x.IssuingAuthority, x.JurisdictionCode, x.IssuedOn, x.ExpiresOn, x.Source, x.Status, x.DeclaredAtUtc, x.VerifiedAtUtc,
        x.VerificationMethod, x.DecisionReason, x.SupersededAtUtc, x.SupersededById.HasValue ? x.SupersededById.Value.Value : null);
}
