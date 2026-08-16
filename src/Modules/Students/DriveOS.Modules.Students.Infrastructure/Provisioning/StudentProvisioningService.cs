using DriveOS.Modules.Students.Application.Provisioning;
using DriveOS.Modules.Students.Domain.Enrollments;
using DriveOS.Modules.Students.Domain.Students;
using DriveOS.Modules.Students.Infrastructure.Persistence;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;
using Microsoft.EntityFrameworkCore;

namespace DriveOS.Modules.Students.Infrastructure.Provisioning;

internal sealed class StudentProvisioningService(StudentsDbContext db) : IStudentProvisioningService
{
    public async Task<Result<ProvisionStudentResult>> ProvisionAsync(
        ProvisionStudentRequest request,
        CancellationToken cancellationToken = default
    )
    {
        Enrollment? existingEnrollment = await db
            .Enrollments.AsNoTracking()
            .SingleOrDefaultAsync(
                x =>
                    x.OrganizationId == request.OrganizationId
                    && x.SourceLeadId == request.SourceLeadId,
                cancellationToken
            );
        if (existingEnrollment is not null)
            return Result.Success(
                new ProvisionStudentResult(
                    existingEnrollment.StudentId,
                    existingEnrollment.Id,
                    true
                )
            );

        Result<Student> studentResult = Student.Create(
            PersonId.New(),
            request.OrganizationId,
            request.FirstName,
            request.LastName,
            request.Email,
            request.Phone
        );
        if (studentResult.IsFailure)
            return Result.Failure<ProvisionStudentResult>(studentResult.Error);
        Result<Enrollment> enrollmentResult = Enrollment.CreateDraft(
            DraftEnrollmentId.New(),
            request.OrganizationId,
            studentResult.Value.Id,
            request.BranchId,
            request.SourceLeadId,
            request.TrainingCode
        );
        if (enrollmentResult.IsFailure)
            return Result.Failure<ProvisionStudentResult>(enrollmentResult.Error);

        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            db.Students.Add(studentResult.Value);
            db.Enrollments.Add(enrollmentResult.Value);
            await db.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return Result.Success(
                new ProvisionStudentResult(studentResult.Value.Id, enrollmentResult.Value.Id, false)
            );
        }
        catch (DbUpdateException)
        {
            await transaction.RollbackAsync(cancellationToken);
            db.ChangeTracker.Clear();
            existingEnrollment = await db
                .Enrollments.AsNoTracking()
                .SingleOrDefaultAsync(
                    x =>
                        x.OrganizationId == request.OrganizationId
                        && x.SourceLeadId == request.SourceLeadId,
                    cancellationToken
                );
            if (existingEnrollment is not null)
                return Result.Success(
                    new ProvisionStudentResult(
                        existingEnrollment.StudentId,
                        existingEnrollment.Id,
                        true
                    )
                );
            throw;
        }
    }
}
