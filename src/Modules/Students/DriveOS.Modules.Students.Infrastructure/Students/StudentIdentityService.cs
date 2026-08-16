using DriveOS.Application.Abstractions.Time;
using DriveOS.Modules.Students.Application.Students.Identity;
using DriveOS.Modules.Students.Domain.Students;
using DriveOS.Modules.Students.Infrastructure.Persistence;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;
using Microsoft.EntityFrameworkCore;

namespace DriveOS.Modules.Students.Infrastructure.Students;

internal sealed class StudentIdentityService(StudentsDbContext db, IClock clock)
    : IStudentIdentityService
{
    public async Task<StudentIdentityResponse?> GetAsync(
        OrganizationId organizationId,
        PersonId studentId,
        CancellationToken cancellationToken = default
    )
    {
        Student? student = await db
            .Students.AsNoTracking()
            .SingleOrDefaultAsync(
                x => x.OrganizationId == organizationId && x.Id == studentId,
                cancellationToken
            );
        return student is null ? null : Map(student);
    }

    public async Task<Result<UpdateStudentIdentityResponse>> UpdateAsync(
        UpdateStudentIdentityCommand command,
        CancellationToken cancellationToken = default
    )
    {
        Student? student = await FindTracked(
            command.OrganizationId,
            command.StudentId,
            cancellationToken
        );
        if (student is null)
            return Result.Failure<UpdateStudentIdentityResponse>(StudentIdentityErrors.NotFound);
        Result update = student.UpdateIdentity(
            command.Identity,
            command.Justification,
            command.ActorUserId,
            clock.UtcNow
        );
        if (update.IsFailure)
            return Result.Failure<UpdateStudentIdentityResponse>(update.Error);
        await db.SaveChangesAsync(cancellationToken);

        string? email = Normalize(student.Email)?.ToLowerInvariant();
        string? phone = Normalize(student.Phone);
        bool duplicate = await db
            .Students.AsNoTracking()
            .AnyAsync(
                x =>
                    x.OrganizationId == command.OrganizationId
                    && x.Id != command.StudentId
                    && (
                        (email != null && x.Email != null && x.Email.ToLower() == email)
                        || (phone != null && x.Phone == phone)
                    ),
                cancellationToken
            );
        return Result.Success(new UpdateStudentIdentityResponse(Map(student), duplicate));
    }

    public async Task<Result<StudentIdentityResponse>> VerifyAsync(
        VerifyStudentIdentityCommand command,
        CancellationToken cancellationToken = default
    )
    {
        Student? student = await FindTracked(
            command.OrganizationId,
            command.StudentId,
            cancellationToken
        );
        if (student is null)
            return Result.Failure<StudentIdentityResponse>(StudentIdentityErrors.NotFound);
        Result verification = student.VerifyIdentity(
            command.Status,
            command.Justification,
            command.ActorUserId,
            clock.UtcNow
        );
        if (verification.IsFailure)
            return Result.Failure<StudentIdentityResponse>(verification.Error);
        await db.SaveChangesAsync(cancellationToken);
        return Result.Success(Map(student));
    }

    public async Task<Result<UpdateStudentIdentityResponse>> UpdateOwnContactAsync(
        UpdateOwnStudentContactCommand command,
        CancellationToken cancellationToken = default
    )
    {
        Student? student = await FindTracked(
            command.OrganizationId,
            command.StudentId,
            cancellationToken
        );
        if (student is null)
            return Result.Failure<UpdateStudentIdentityResponse>(StudentIdentityErrors.NotFound);
        Result update = student.UpdateSelfServiceContact(
            command.Email,
            command.Phone,
            command.AddressLine1,
            command.AddressLine2,
            command.PostalCode,
            command.City,
            command.CountryCode,
            command.PreferredLanguage,
            command.TimeZone,
            command.AllowEmail,
            command.AllowSms,
            command.AllowPhone,
            command.ActorUserId,
            clock.UtcNow
        );
        if (update.IsFailure)
            return Result.Failure<UpdateStudentIdentityResponse>(update.Error);
        await db.SaveChangesAsync(cancellationToken);
        bool duplicate = await HasDuplicateAsync(
            student,
            command.OrganizationId,
            cancellationToken
        );
        return Result.Success(new UpdateStudentIdentityResponse(Map(student), duplicate));
    }

    private Task<bool> HasDuplicateAsync(
        Student student,
        OrganizationId organizationId,
        CancellationToken cancellationToken
    )
    {
        string? email = Normalize(student.Email)?.ToLowerInvariant();
        string? phone = Normalize(student.Phone);
        return db
            .Students.AsNoTracking()
            .AnyAsync(
                x =>
                    x.OrganizationId == organizationId
                    && x.Id != student.Id
                    && (
                        (email != null && x.Email != null && x.Email.ToLower() == email)
                        || (phone != null && x.Phone == phone)
                    ),
                cancellationToken
            );
    }

    private Task<Student?> FindTracked(
        OrganizationId organizationId,
        PersonId studentId,
        CancellationToken cancellationToken
    ) =>
        db
            .Students.Include(x => x.IdentityAuditEntries)
            .SingleOrDefaultAsync(
                x => x.OrganizationId == organizationId && x.Id == studentId,
                cancellationToken
            );

    private static StudentIdentityResponse Map(Student x) =>
        new(
            x.Id.Value,
            x.FirstName,
            x.LastName,
            x.PreferredName,
            x.BirthDate,
            x.BirthPlace,
            x.Nationality,
            x.Email,
            x.Phone,
            x.AddressLine1,
            x.AddressLine2,
            x.PostalCode,
            x.City,
            x.CountryCode,
            x.PreferredLanguage,
            x.TimeZone,
            x.AllowEmail,
            x.AllowSms,
            x.AllowPhone,
            x.IdentityVerificationStatus,
            x.IdentityVerifiedAtUtc
        );

    private static string? Normalize(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
