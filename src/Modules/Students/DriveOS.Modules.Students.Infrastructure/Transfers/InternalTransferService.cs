using DriveOS.Application.Abstractions.Time;
using DriveOS.Modules.Students.Application.Branches;
using DriveOS.Modules.Students.Application.Transfers;
using DriveOS.Modules.Students.Domain.Branches;
using DriveOS.Modules.Students.Domain.Transfers;
using DriveOS.Modules.Students.Infrastructure.Persistence;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;
using Microsoft.EntityFrameworkCore;

namespace DriveOS.Modules.Students.Infrastructure.Transfers;

internal sealed class InternalTransferService(
    StudentsDbContext db,
    IClock clock,
    IStudentBranchVerifier branchVerifier,
    IInternalTransferImpactAnalyzer impactAnalyzer
) : IInternalTransferService
{
    public async Task<IReadOnlyList<InternalTransferResponse>> GetAsync(
        GetInternalTransfersQuery q,
        CancellationToken ct = default
    )
    {
        var transfers = await db
            .InternalTransferCases.AsNoTracking()
            .Where(x => x.OrganizationId == q.OrganizationId && x.StudentId == q.StudentId)
            .Include(x => x.Impacts)
            .OrderByDescending(x => x.AnalyzedAtUtc)
            .ToListAsync(ct);
        return transfers.Select(Map).ToArray();
    }

    public async Task<Result<InternalTransferResponse>> AnalyzeAsync(
        AnalyzeInternalTransferCommand c,
        CancellationToken ct = default
    )
    {
        if (
            !await db
                .Students.AsNoTracking()
                .AnyAsync(x => x.OrganizationId == c.OrganizationId && x.Id == c.StudentId, ct)
        )
            return Result.Failure<InternalTransferResponse>(
                InternalTransferApplicationErrors.StudentNotFound
            );
        var now = clock.UtcNow;
        if (
            await db
                .InternalTransferCases.AsNoTracking()
                .AnyAsync(
                    x =>
                        x.OrganizationId == c.OrganizationId
                        && x.StudentId == c.StudentId
                        && (
                            x.Status == InternalTransferStatus.Scheduled
                            || x.Status == InternalTransferStatus.Analyzed
                                && x.AnalysisExpiresAtUtc >= now
                        ),
                    ct
                )
        )
            return Result.Failure<InternalTransferResponse>(
                InternalTransferErrors.ActiveTransferExists
            );
        var source = await ResolveSourceBranch(c.OrganizationId, c.StudentId, ct);
        if (source is null)
            return Result.Failure<InternalTransferResponse>(
                InternalTransferApplicationErrors.SourceBranchNotFound
            );
        var checks = await branchVerifier.VerifyAsync(c.OrganizationId, c.TargetBranchId, ct);
        if (checks.Any(x => x.Status == BranchVerificationStatus.Failed))
            return Result.Failure<InternalTransferResponse>(
                InternalTransferApplicationErrors.TargetBranchNotEligible
            );
        var impacts = await impactAnalyzer.AnalyzeAsync(
            c.OrganizationId,
            c.StudentId,
            source.Value,
            c.TargetBranchId,
            c.Elements,
            ct
        );
        var created = InternalTransferCase.Create(
            c.OrganizationId,
            c.StudentId,
            source.Value,
            c.TargetBranchId,
            c.Mode,
            c.Elements,
            c.EffectiveOn,
            c.TemporaryUntil,
            c.Reason,
            impacts,
            c.ActorUserId,
            now
        );
        if (created.IsFailure)
            return Result.Failure<InternalTransferResponse>(created.Error);
        db.InternalTransferCases.Add(created.Value);
        await db.SaveChangesAsync(ct);
        return Result.Success(Map(created.Value));
    }

    public async Task<Result<InternalTransferResponse>> ValidateAsync(
        ValidateInternalTransferCommand c,
        CancellationToken ct = default
    )
    {
        var transfer = await db
            .InternalTransferCases.Include(x => x.Impacts)
            .SingleOrDefaultAsync(
                x =>
                    x.Id == new InternalTransferCaseId(c.TransferId)
                    && x.OrganizationId == c.OrganizationId
                    && x.StudentId == c.StudentId,
                ct
            );
        if (transfer is null)
            return Result.Failure<InternalTransferResponse>(
                InternalTransferErrors.AnalysisNotFound
            );
        var validated = transfer.Validate(c.ActorUserId, clock.UtcNow);
        if (validated.IsFailure)
            return Result.Failure<InternalTransferResponse>(validated.Error);
        var enrollment = await db
            .Enrollments.Where(x =>
                x.OrganizationId == c.OrganizationId && x.StudentId == c.StudentId
            )
            .OrderByDescending(x => x.CreatedAtUtc)
            .FirstOrDefaultAsync(ct);
        if (enrollment is null)
            return Result.Failure<InternalTransferResponse>(
                InternalTransferApplicationErrors.EnrollmentNotFound
            );
        var portfolio = await db
            .StudentBranchPortfolios.Include(x => x.Assignments)
            .SingleOrDefaultAsync(
                x => x.OrganizationId == c.OrganizationId && x.StudentId == c.StudentId,
                ct
            );
        if (portfolio is null)
        {
            portfolio = StudentBranchPortfolio.Create(c.OrganizationId, c.StudentId).Value;
            db.StudentBranchPortfolios.Add(portfolio);
            portfolio.Assign(
                transfer.SourceBranchId,
                StudentBranchAssignmentType.Primary,
                StudentBranchService.Administration,
                DateOnly.FromDateTime(enrollment.CreatedAtUtc.UtcDateTime),
                null,
                "Enrollment origin",
                c.ActorUserId,
                clock.UtcNow
            );
        }
        var branchChange = portfolio.TransferPrimary(
            transfer.TargetBranchId,
            transfer.EffectiveOn,
            transfer.TemporaryUntil,
            transfer.Reason,
            c.ActorUserId,
            clock.UtcNow
        );
        if (branchChange.IsFailure)
            return Result.Failure<InternalTransferResponse>(branchChange.Error);
        if (transfer.Status == InternalTransferStatus.Applied)
        {
            var moved = enrollment.TransferToBranch(
                transfer.TargetBranchId,
                c.ActorUserId,
                clock.UtcNow
            );
            if (moved.IsFailure)
                return Result.Failure<InternalTransferResponse>(moved.Error);
        }
        await db.SaveChangesAsync(ct);
        return Result.Success(Map(transfer));
    }

    private async Task<BranchId?> ResolveSourceBranch(
        OrganizationId org,
        PersonId student,
        CancellationToken ct
    )
    {
        var portfolio = await db
            .StudentBranchPortfolios.AsNoTracking()
            .Include(x => x.Assignments)
            .SingleOrDefaultAsync(x => x.OrganizationId == org && x.StudentId == student, ct);
        var today = DateOnly.FromDateTime(clock.UtcNow.UtcDateTime);
        var current = portfolio
            ?.Assignments.Where(x =>
                x.Type == StudentBranchAssignmentType.Primary
                && x.EffectiveFrom <= today
                && (!x.EffectiveTo.HasValue || x.EffectiveTo.Value >= today)
                && x.Status != StudentBranchAssignmentStatus.Ended
            )
            .OrderByDescending(x => x.EffectiveFrom)
            .FirstOrDefault();
        if (current is not null)
            return current.BranchId;
        var enrollment = await db
            .Enrollments.AsNoTracking()
            .Where(x => x.OrganizationId == org && x.StudentId == student)
            .OrderByDescending(x => x.CreatedAtUtc)
            .FirstOrDefaultAsync(ct);
        return enrollment?.BranchId;
    }

    private static InternalTransferResponse Map(InternalTransferCase x) =>
        new(
            x.Id,
            x.StudentId.Value,
            x.SourceBranchId.Value,
            x.TargetBranchId.Value,
            x.Mode,
            x.Elements,
            x.EffectiveOn,
            x.TemporaryUntil,
            x.Reason,
            x.Status,
            x.AnalysisExpiresAtUtc,
            x.Impacts.OrderBy(i => i.Type)
                .Select(i => new InternalTransferImpactItem(
                    i.Type,
                    i.AffectedCount,
                    i.Status,
                    i.MessageKey,
                    i.RequiresAction
                ))
                .ToArray()
        );
}

internal sealed class InternalTransferImpactAnalyzer(StudentsDbContext db)
    : IInternalTransferImpactAnalyzer
{
    public async Task<IReadOnlyList<InternalTransferImpactSeed>> AnalyzeAsync(
        OrganizationId org,
        PersonId student,
        BranchId source,
        BranchId target,
        InternalTransferElement elements,
        CancellationToken ct = default
    )
    {
        var result = new List<InternalTransferImpactSeed>();
        void Add(
            InternalTransferElement flag,
            InternalTransferImpactType type,
            int count,
            InternalTransferImpactStatus status,
            string key,
            bool action
        )
        {
            if (elements.HasFlag(flag))
                result.Add(new(type, count, status, key, action));
        }
        Add(
            InternalTransferElement.Enrollment,
            InternalTransferImpactType.Enrollment,
            await db
                .Enrollments.AsNoTracking()
                .CountAsync(x => x.OrganizationId == org && x.StudentId == student, ct),
            InternalTransferImpactStatus.Passed,
            "students.internalTransfer.impacts.enrollmentPreserved",
            false
        );
        Add(
            InternalTransferElement.FutureSessions,
            InternalTransferImpactType.FutureSessions,
            0,
            InternalTransferImpactStatus.NotEvaluated,
            "students.internalTransfer.impacts.futureSessionsRevalidation",
            true
        );
        Add(
            InternalTransferElement.Instructor,
            InternalTransferImpactType.Instructor,
            await db
                .StudentInstructorPortfolios.AsNoTracking()
                .Where(x => x.OrganizationId == org && x.StudentId == student)
                .SelectMany(x => x.Assignments)
                .CountAsync(
                    x =>
                        x.Status
                        == DriveOS
                            .Modules
                            .Students
                            .Domain
                            .Instructors
                            .StudentInstructorAssignmentStatus
                            .Active,
                    ct
                ),
            InternalTransferImpactStatus.Warning,
            "students.internalTransfer.impacts.instructorReview",
            true
        );
        Add(
            InternalTransferElement.Vehicles,
            InternalTransferImpactType.Vehicles,
            0,
            InternalTransferImpactStatus.NotEvaluated,
            "students.internalTransfer.impacts.vehicleReview",
            true
        );
        Add(
            InternalTransferElement.Pricing,
            InternalTransferImpactType.Pricing,
            0,
            InternalTransferImpactStatus.Passed,
            "students.internalTransfer.impacts.existingPricingPreserved",
            false
        );
        Add(
            InternalTransferElement.Credits,
            InternalTransferImpactType.Credits,
            0,
            InternalTransferImpactStatus.Warning,
            "students.internalTransfer.impacts.creditsReview",
            true
        );
        Add(
            InternalTransferElement.Documents,
            InternalTransferImpactType.Documents,
            0,
            InternalTransferImpactStatus.Passed,
            "students.internalTransfer.impacts.documentsPreserved",
            false
        );
        Add(
            InternalTransferElement.Exams,
            InternalTransferImpactType.Exams,
            0,
            InternalTransferImpactStatus.NotEvaluated,
            "students.internalTransfer.impacts.examReview",
            true
        );
        Add(
            InternalTransferElement.Payments,
            InternalTransferImpactType.Payments,
            0,
            InternalTransferImpactStatus.Passed,
            "students.internalTransfer.impacts.paymentsPreserved",
            false
        );
        Add(
            InternalTransferElement.Communications,
            InternalTransferImpactType.Communications,
            0,
            InternalTransferImpactStatus.Warning,
            "students.internalTransfer.impacts.communicationReview",
            true
        );
        Add(
            InternalTransferElement.MeetingPoint,
            InternalTransferImpactType.MeetingPoint,
            0,
            InternalTransferImpactStatus.Warning,
            "students.internalTransfer.impacts.meetingPointReview",
            true
        );
        return result;
    }
}
