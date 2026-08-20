using DriveOS.Modules.SchedulingCapacity.Application.Bookings;
using DriveOS.Modules.SchedulingCapacity.Application.Replacements;
using DriveOS.Modules.SchedulingCapacity.Application.Persistence;
using DriveOS.Application.Abstractions.Time;
using DriveOS.Modules.SchedulingCapacity.Domain.Bookings;
using DriveOS.Modules.SchedulingCapacity.Domain.CalendarResources;
using DriveOS.Modules.SchedulingCapacity.Infrastructure.Persistence;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;
using Microsoft.EntityFrameworkCore;

namespace DriveOS.Modules.SchedulingCapacity.Infrastructure.Read;

internal sealed class InstructorReplacementService(
    SchedulingCapacityDbContext db,
    IBookingConflictAssessmentService conflicts,
    IBookingReferenceValidationGateway referenceValidation,
    IBookingCapacityLock capacityLock,
    IInstructorReplacementEligibilityGateway eligibility,
    ISchedulingCapacityUnitOfWork unitOfWork,
    IClock clock) : IInstructorReplacementService
{
    public async Task<IReadOnlyCollection<InstructorReplacementSuggestionResponse>> SuggestAsync(
        OrganizationId organizationId,
        UserId previousInstructorId,
        IReadOnlyCollection<BookingId> bookingIds,
        string trainingCategory,
        CancellationToken cancellationToken = default)
    {
        Booking[] bookings = await LoadBookings(organizationId, bookingIds, false, cancellationToken);
        if (bookings.Length == 0) return [];

        CalendarResource[] allResources = await db.CalendarResources.AsNoTracking()
            .Where(x => x.OrganizationId == organizationId && x.ResourceType == CalendarResourceType.Instructor && x.Status == CalendarResourceStatus.Active)
            .ToArrayAsync(cancellationToken);

        CalendarResource[] candidates = allResources
            .Where(x => x.ExternalResourceId != previousInstructorId.Value)
            .GroupBy(x => x.ExternalResourceId)
            .Select(g => g.OrderByDescending(x => bookings.Count(b => b.BranchId == x.BranchId)).ThenBy(x => x.DisplayName).First())
            .ToArray();

        var results = new List<InstructorReplacementSuggestionResponse>();
        foreach (CalendarResource candidate in candidates)
        {
            var factors = new List<string>();
            var externalReviews = new HashSet<string>(StringComparer.Ordinal);
            int compatible = 0;
            bool qualificationVerified = true;
            bool continuity = false;

            foreach (Booking booking in bookings)
            {
                CalendarResource? previousResource = ResolveInstructorResource(allResources, booking, previousInstructorId);
                CalendarResource? replacementResource = ResolveCandidateResource(allResources, booking, new UserId(candidate.ExternalResourceId));
                if (previousResource is null || replacementResource is null) continue;

                string effectiveTrainingCategory = ResolveTrainingCategory(booking, trainingCategory);
                if (string.IsNullOrWhiteSpace(effectiveTrainingCategory))
                {
                    qualificationVerified = false;
                    externalReviews.Add("booking.training-category.missing");
                    continue;
                }

                PersonId? studentId = ResolveStudent(booking);
                InstructorReplacementEligibility check = await eligibility.EvaluateAsync(
                    organizationId, studentId, new UserId(candidate.ExternalResourceId), booking.BranchId, effectiveTrainingCategory, cancellationToken);
                qualificationVerified &= check.QualificationVerified;
                continuity |= check.HasStudentContinuity;
                foreach (string warning in check.Warnings) externalReviews.Add(warning);
                if (!check.IsEligible) continue;

                Booking probe = CloneForReplacement(booking, previousResource.Id, replacementResource.Id, previousInstructorId, new UserId(candidate.ExternalResourceId));
                BookingConflictAssessment assessment = await conflicts.AssessAsync(probe, cancellationToken);
                if (assessment.IsConflictFree) compatible++;
            }

            if (qualificationVerified) factors.Add("qualification:verified");
            if (continuity) factors.Add("continuity:student-known");
            if (candidate.BranchId.HasValue && bookings.Any(x => x.BranchId == candidate.BranchId)) factors.Add("branch:matching");
            factors.Add($"availability:{compatible}/{bookings.Length}");
            externalReviews.Add("workforce.load.external-review");
            externalReviews.Add("workforce.language.external-review");
            externalReviews.Add("marketplace.contract.external-review");
            externalReviews.Add("marketplace.cost.external-review");

            int score = compatible * 40 + (qualificationVerified ? 25 : 0) + (continuity ? 20 : 0) +
                        (candidate.BranchId.HasValue && bookings.Any(x => x.BranchId == candidate.BranchId) ? 15 : 0);

            results.Add(new InstructorReplacementSuggestionResponse(
                candidate.ExternalResourceId, candidate.Id.Value, candidate.DisplayName, candidate.BranchId?.Value,
                qualificationVerified, compatible == bookings.Length, continuity, null, compatible, bookings.Length,
                score, factors, externalReviews.ToArray()));
        }

        return results.OrderByDescending(x => x.Score).ThenBy(x => x.DisplayName).ToArray();
    }

    public async Task<InstructorReplacementPreviewResponse?> PreviewAsync(
        OrganizationId organizationId,
        Guid operationId,
        UserId previousInstructorId,
        UserId replacementInstructorId,
        int mode,
        IReadOnlyCollection<BookingId> bookingIds,
        string trainingCategory,
        DateTimeOffset? accessExpiresAtUtc,
        CancellationToken cancellationToken = default)
    {
        if (!Enum.IsDefined(typeof(InstructorReplacementMode), mode)) return null;
        Booking[] bookings = await LoadBookings(organizationId, bookingIds, false, cancellationToken);
        if (bookings.Length != bookingIds.Distinct().Count()) return null;
        CalendarResource[] resources = await LoadInstructorResources(organizationId, cancellationToken);

        var blocking = new List<string>();
        var reviews = new HashSet<string>(StringComparer.Ordinal);
        foreach (Booking booking in bookings)
        {
            CalendarResource? previous = ResolveInstructorResource(resources, booking, previousInstructorId);
            CalendarResource? replacement = ResolveCandidateResource(resources, booking, replacementInstructorId);
            if (previous is null) { blocking.Add($"booking:{booking.Id.Value}:previous-instructor-resource-not-found"); continue; }
            if (replacement is null) { blocking.Add($"booking:{booking.Id.Value}:replacement-instructor-resource-not-found"); continue; }

            string effectiveTrainingCategory = ResolveTrainingCategory(booking, trainingCategory);
            if (string.IsNullOrWhiteSpace(effectiveTrainingCategory)) { blocking.Add($"booking:{booking.Id.Value}:training-category-missing"); continue; }

            PersonId? student = ResolveStudent(booking);
            InstructorReplacementEligibility check = await eligibility.EvaluateAsync(organizationId, student, replacementInstructorId, booking.BranchId, effectiveTrainingCategory, cancellationToken);
            foreach (string warning in check.Warnings) reviews.Add(warning);
            if (!check.IsEligible || !check.QualificationVerified) { blocking.Add($"booking:{booking.Id.Value}:instructor-not-eligible"); continue; }

            Booking probe = CloneForReplacement(booking, previous.Id, replacement.Id, previousInstructorId, replacementInstructorId);
            BookingConflictAssessment assessment = await conflicts.AssessAsync(probe, cancellationToken);
            foreach (BookingConflict conflict in assessment.Conflicts)
                blocking.Add($"booking:{booking.Id.Value}:conflict:{conflict.Type}");
        }

        reviews.Add("marketplace.contract.external-review");
        reviews.Add("marketplace.remuneration.external-review");
        reviews.Add("identity.temporary-access.event-driven");

        return new InstructorReplacementPreviewResponse(
            operationId, previousInstructorId.Value, replacementInstructorId.Value, mode,
            bookings.Select(x => x.Id.Value).ToArray(), ResolveStudents(bookings), blocking.Count == 0,
            blocking, reviews.ToArray());
    }

    public async Task<Result<InstructorReplacementApplyResponse>> ApplyAsync(
        OrganizationId organizationId,
        Guid operationId,
        UserId previousInstructorId,
        UserId replacementInstructorId,
        int mode,
        IReadOnlyCollection<BookingId> bookingIds,
        string trainingCategory,
        string reason,
        DateTimeOffset? accessExpiresAtUtc,
        CancellationToken cancellationToken = default)
    {
        if (operationId == Guid.Empty || !Enum.IsDefined(typeof(InstructorReplacementMode), mode) || bookingIds.Count == 0)
            return Result.Failure<InstructorReplacementApplyResponse>(BookingErrors.InvalidInstructorReplacement);

        await unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            Booking[] bookings = await LoadBookings(organizationId, bookingIds, true, cancellationToken);
            if (bookings.Length != bookingIds.Distinct().Count())
            {
                await unitOfWork.RollbackTransactionAsync(cancellationToken);
                return Result.Failure<InstructorReplacementApplyResponse>(BookingApplicationErrors.NotFound);
            }

            if (bookings.All(x => x.InstructorReplacementHistory.Any(h => h.OperationId == operationId && h.ReplacementInstructorId == replacementInstructorId)))
            {
                await unitOfWork.RollbackTransactionAsync(cancellationToken);
                return Result.Success(new InstructorReplacementApplyResponse(operationId, bookings.Length, bookings.Select(x => x.Id.Value).ToArray(), ResolveStudents(bookings)));
            }

            CalendarResource[] resources = await LoadInstructorResources(organizationId, cancellationToken);
            var replacements = new List<(Booking Booking, CalendarResource Previous, CalendarResource Replacement)>();
            foreach (Booking booking in bookings)
            {
                CalendarResource? previous = ResolveInstructorResource(resources, booking, previousInstructorId);
                CalendarResource? replacement = ResolveCandidateResource(resources, booking, replacementInstructorId);
                if (previous is null || replacement is null)
                {
                    await unitOfWork.RollbackTransactionAsync(cancellationToken);
                    return Result.Failure<InstructorReplacementApplyResponse>(BookingErrors.PreviousInstructorResourceNotFound);
                }

                string effectiveTrainingCategory = ResolveTrainingCategory(booking, trainingCategory);
                if (string.IsNullOrWhiteSpace(effectiveTrainingCategory))
                {
                    await unitOfWork.RollbackTransactionAsync(cancellationToken);
                    return Result.Failure<InstructorReplacementApplyResponse>(BookingErrors.InvalidInstructorReplacement);
                }

                PersonId? student = ResolveStudent(booking);
                InstructorReplacementEligibility check = await eligibility.EvaluateAsync(organizationId, student, replacementInstructorId, booking.BranchId, effectiveTrainingCategory, cancellationToken);
                if (!check.IsEligible || !check.QualificationVerified)
                {
                    await unitOfWork.RollbackTransactionAsync(cancellationToken);
                    return Result.Failure<InstructorReplacementApplyResponse>(BookingErrors.InvalidInstructorReplacement);
                }
                replacements.Add((booking, previous, replacement));
            }

            await capacityLock.AcquireAsync(organizationId,
                replacements.SelectMany(x => x.Booking.Resources.Select(r => r.CalendarResourceId).Append(x.Replacement.Id)).Distinct().ToArray(), cancellationToken);

            DateTimeOffset now = clock.UtcNow;
            foreach ((Booking booking, CalendarResource previous, CalendarResource replacement) in replacements)
            {
                Result changed = booking.ReplaceInstructor(operationId, previous.Id, replacement.Id, previousInstructorId, replacementInstructorId,
                    (InstructorReplacementMode)mode, reason, now, accessExpiresAtUtc);
                if (changed.IsFailure)
                {
                    await unitOfWork.RollbackTransactionAsync(cancellationToken);
                    return Result.Failure<InstructorReplacementApplyResponse>(changed.Error);
                }

                Error? referenceError = await BookingReferenceRevalidation.ValidateAsync(booking, referenceValidation, cancellationToken);
                if (referenceError is not null)
                {
                    await unitOfWork.RollbackTransactionAsync(cancellationToken);
                    return Result.Failure<InstructorReplacementApplyResponse>(referenceError);
                }

                BookingConflictAssessment assessment = await conflicts.AssessAsync(booking, cancellationToken);
                if (!assessment.IsConflictFree)
                {
                    await unitOfWork.RollbackTransactionAsync(cancellationToken);
                    return Result.Failure<InstructorReplacementApplyResponse>(BookingErrors.ResourceConflict);
                }
            }

            await unitOfWork.CommitTransactionAsync(cancellationToken);
            return Result.Success(new InstructorReplacementApplyResponse(operationId, bookings.Length,
                bookings.Select(x => x.Id.Value).ToArray(), ResolveStudents(bookings)));
        }
        catch
        {
            if (unitOfWork.HasActiveTransaction) await unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }

    private async Task<Booking[]> LoadBookings(OrganizationId organizationId, IReadOnlyCollection<BookingId> bookingIds, bool tracking, CancellationToken ct)
    {
        BookingId[] ids = bookingIds.Distinct().ToArray();
        IQueryable<Booking> query = db.Bookings
            .Include(x => x.Resources).Include(x => x.Participants).Include(x => x.RescheduleHistory)
            .Include(x => x.Cancellations).Include(x => x.AttendanceHistory).Include(x => x.InstructorReplacementHistory)
            .Where(x => x.OrganizationId == organizationId && ids.Contains(x.Id));
        if (!tracking) query = query.AsNoTracking();
        return await query.OrderBy(x => x.StartAtUtc).ToArrayAsync(ct);
    }

    private Task<CalendarResource[]> LoadInstructorResources(OrganizationId organizationId, CancellationToken ct) =>
        db.CalendarResources.AsNoTracking().Where(x => x.OrganizationId == organizationId && x.ResourceType == CalendarResourceType.Instructor && x.Status == CalendarResourceStatus.Active).ToArrayAsync(ct);

    private static CalendarResource? ResolveInstructorResource(IEnumerable<CalendarResource> resources, Booking booking, UserId instructorId) =>
        resources.FirstOrDefault(x => x.ExternalResourceId == instructorId.Value && booking.Resources.Any(r => r.CalendarResourceId == x.Id));

    private static CalendarResource? ResolveCandidateResource(IEnumerable<CalendarResource> resources, Booking booking, UserId instructorId) =>
        resources.Where(x => x.ExternalResourceId == instructorId.Value)
            .OrderByDescending(x => x.BranchId == booking.BranchId).ThenBy(x => x.DisplayName).FirstOrDefault();

    private static PersonId? ResolveStudent(Booking booking)
    {
        BookingParticipant? participant = booking.Participants.FirstOrDefault(x => x.ParticipantType == BookingParticipantType.Student);
        return participant is null ? null : new PersonId(participant.ExternalParticipantId);
    }


    private static string ResolveTrainingCategory(Booking booking, string requestFallback) =>
        !string.IsNullOrWhiteSpace(booking.TrainingCategory)
            ? booking.TrainingCategory!
            : requestFallback?.Trim() ?? string.Empty;

    private static Guid[] ResolveStudents(IEnumerable<Booking> bookings) => bookings.SelectMany(x => x.Participants)
        .Where(x => x.ParticipantType == BookingParticipantType.Student).Select(x => x.ExternalParticipantId).Distinct().ToArray();

    private static Booking CloneForReplacement(Booking source, CalendarResourceId previousResourceId, CalendarResourceId replacementResourceId, UserId previousInstructorId, UserId replacementInstructorId)
    {
        Booking probe = Booking.Create(source.Id, source.OrganizationId, source.BranchId, source.BookingType, source.StartAtUtc, source.EndAtUtc, source.Title).Value;
        foreach (BookingResource r in source.Resources)
            probe.AddResource(BookingResourceId.New(), r.CalendarResourceId == previousResourceId ? replacementResourceId : r.CalendarResourceId, r.Quantity);
        foreach (BookingParticipant p in source.Participants)
            probe.AddParticipant(BookingParticipantId.New(), p.ParticipantType,
                p.ParticipantType == BookingParticipantType.Instructor && p.ExternalParticipantId == previousInstructorId.Value ? replacementInstructorId.Value : p.ExternalParticipantId);
        return probe;
    }
}
