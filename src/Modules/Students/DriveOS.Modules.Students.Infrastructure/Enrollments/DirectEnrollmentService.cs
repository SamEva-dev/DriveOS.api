using DriveOS.Modules.Students.Application.Enrollments.StartDirectEnrollment;
using DriveOS.Modules.Students.Domain.Enrollments;
using DriveOS.Modules.Students.Domain.Students;
using DriveOS.Modules.Students.Infrastructure.Persistence;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;
using Microsoft.EntityFrameworkCore;

namespace DriveOS.Modules.Students.Infrastructure.Enrollments;

internal sealed class DirectEnrollmentService(StudentsDbContext db) : IDirectEnrollmentService
{
    public async Task<Result<StartDirectEnrollmentResponse>> StartAsync(
        StartDirectEnrollmentCommand command,
        CancellationToken cancellationToken = default
    )
    {
        string key = command.IdempotencyKey.Trim();
        Enrollment? replay = await db
            .Enrollments.AsNoTracking()
            .SingleOrDefaultAsync(
                x => x.OrganizationId == command.OrganizationId && x.IdempotencyKey == key,
                cancellationToken
            );
        if (replay is not null)
            return Success(replay, true, true);

        Student? student;
        bool reused;
        if (command.ExistingStudentId.HasValue)
        {
            student = await db.Students.SingleOrDefaultAsync(
                x =>
                    x.OrganizationId == command.OrganizationId
                    && x.Id == command.ExistingStudentId.Value,
                cancellationToken
            );
            if (student is null)
                return Result.Failure<StartDirectEnrollmentResponse>(
                    DirectEnrollmentErrors.StudentNotFound
                );
            reused = true;
        }
        else
        {
            string? email = Normalize(command.Email)?.ToLowerInvariant();
            string? phone = Normalize(command.Phone);
            bool duplicateExists = await db
                .Students.AsNoTracking()
                .AnyAsync(
                    x =>
                        x.OrganizationId == command.OrganizationId
                        && (
                            (email != null && x.Email != null && x.Email.ToLower() == email)
                            || (phone != null && x.Phone == phone)
                        ),
                    cancellationToken
                );
            if (duplicateExists)
                return Result.Failure<StartDirectEnrollmentResponse>(
                    DirectEnrollmentErrors.PossibleDuplicate
                );

            Result<Student> studentResult = Student.Create(
                PersonId.New(),
                command.OrganizationId,
                command.FirstName,
                command.LastName,
                command.Email,
                command.Phone
            );
            if (studentResult.IsFailure)
                return Result.Failure<StartDirectEnrollmentResponse>(studentResult.Error);
            student = studentResult.Value;
            db.Students.Add(student);
            reused = false;
        }

        Result<Enrollment> enrollmentResult = Enrollment.CreateDirectDraft(
            DraftEnrollmentId.New(),
            command.OrganizationId,
            student.Id,
            command.BranchId,
            command.TrainingCode,
            command.Source,
            key,
            command.RegulatoryCountryCode,
            command.PreferredLanguageCode,
            command.RequiredConsentsAccepted
        );
        if (enrollmentResult.IsFailure)
            return Result.Failure<StartDirectEnrollmentResponse>(enrollmentResult.Error);

        db.Enrollments.Add(enrollmentResult.Value);
        try
        {
            await db.SaveChangesAsync(cancellationToken);
            return Success(enrollmentResult.Value, reused, false);
        }
        catch (DbUpdateException)
        {
            db.ChangeTracker.Clear();
            replay = await db
                .Enrollments.AsNoTracking()
                .SingleOrDefaultAsync(
                    x => x.OrganizationId == command.OrganizationId && x.IdempotencyKey == key,
                    cancellationToken
                );
            if (replay is not null)
                return Success(replay, true, true);
            throw;
        }
    }

    private static Result<StartDirectEnrollmentResponse> Success(
        Enrollment enrollment,
        bool reused,
        bool replay
    ) =>
        Result.Success(
            new StartDirectEnrollmentResponse(
                enrollment.StudentId.Value,
                enrollment.Id.Value,
                reused,
                replay
            )
        );

    private static string? Normalize(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
