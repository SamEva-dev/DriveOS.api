using DriveOS.Application.Abstractions.Time;
using DriveOS.Modules.SchedulingCapacity.Application.Bookings;
using DriveOS.Modules.SchedulingCapacity.Application.Persistence;
using DriveOS.Modules.SchedulingCapacity.Application.Replacements;
using DriveOS.Modules.SchedulingCapacity.Domain.Bookings;
using DriveOS.Modules.SchedulingCapacity.Domain.CalendarResources;
using DriveOS.Modules.SchedulingCapacity.Infrastructure.Persistence;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;
using Microsoft.EntityFrameworkCore;

namespace DriveOS.Modules.SchedulingCapacity.Infrastructure.Read;

internal sealed class VehicleReplacementService(SchedulingCapacityDbContext db, IBookingConflictAssessmentService conflicts,
    IBookingReferenceValidationGateway referenceValidation, IBookingCapacityLock capacityLock, IVehicleReplacementEligibilityGateway eligibility,
    ISchedulingCapacityUnitOfWork unitOfWork, IClock clock) : IVehicleReplacementService
{
    public async Task<IReadOnlyCollection<VehicleReplacementSuggestionResponse>> SuggestAsync(OrganizationId organizationId, Guid previousVehicleId,
        IReadOnlyCollection<BookingId> bookingIds, VehicleReplacementRequirements requirements, CancellationToken cancellationToken = default)
    {
        Booking[] bookings = await LoadBookings(organizationId, bookingIds, false, cancellationToken);
        if (bookings.Length == 0) return [];
        CalendarResource[] resources = await LoadVehicleResources(organizationId, cancellationToken);
        var results = new List<VehicleReplacementSuggestionResponse>();
        foreach (CalendarResource candidate in resources.Where(x => x.ExternalResourceId != previousVehicleId))
        {
            int compatible = 0; var blocking = new HashSet<string>(StringComparer.Ordinal); var reviews = new HashSet<string>(StringComparer.Ordinal);
            bool tech = true, insurance = true, maintenance = true, location = true, ownership = true;
            foreach (Booking booking in bookings)
            {
                CalendarResource? previous = ResolveVehicleResource(resources, booking, previousVehicleId);
                if (previous is null) { blocking.Add($"booking:{booking.Id.Value}:previous-vehicle-resource-not-found"); continue; }
                VehicleReplacementRequirements effectiveRequirements = requirements with { TrainingCategory = string.IsNullOrWhiteSpace(booking.TrainingCategory) ? requirements.TrainingCategory : booking.TrainingCategory };
                VehicleReplacementEligibility check = await eligibility.EvaluateAsync(organizationId, candidate.ExternalResourceId, booking.BranchId, effectiveRequirements, booking.StartAtUtc, booking.EndAtUtc, cancellationToken);
                tech &= check.TechnicalCompatibilityVerified; insurance &= check.InsuranceVerified; maintenance &= check.MaintenanceVerified; location &= check.LocationVerified; ownership &= check.OwnershipVerified;
                foreach (string r in check.BlockingReasons) blocking.Add(r); foreach (string r in check.ExternalReviews) reviews.Add(r);
                if (!check.IsEligible) continue;
                Booking probe = CloneForReplacement(booking, previous.Id, candidate.Id);
                BookingConflictAssessment assessment = await conflicts.AssessAsync(probe, cancellationToken);
                if (assessment.IsConflictFree) compatible++; else foreach (BookingConflict c in assessment.Conflicts) blocking.Add($"booking:{booking.Id.Value}:conflict:{c.Type}");
            }
            var factors = new List<string> { $"availability:{compatible}/{bookings.Length}" };
            if (candidate.BranchId.HasValue && bookings.Any(x => x.BranchId == candidate.BranchId)) factors.Add("branch:matching");
            int score = compatible * 40 + (tech ? 20 : 0) + (insurance ? 10 : 0) + (maintenance ? 10 : 0) + (location ? 10 : 0) +
                (candidate.BranchId.HasValue && bookings.Any(x => x.BranchId == candidate.BranchId) ? 10 : 0) + (ownership ? 10 : 0);
            results.Add(new(candidate.ExternalResourceId, candidate.Id.Value, candidate.DisplayName, candidate.BranchId?.Value, compatible == bookings.Length, tech, insurance, maintenance, location, ownership,
                compatible, bookings.Length, score, factors, blocking.ToArray(), reviews.ToArray()));
        }
        return results.OrderByDescending(x => x.Score).ThenBy(x => x.DisplayName).ToArray();
    }

    public async Task<VehicleReplacementPreviewResponse?> PreviewAsync(OrganizationId organizationId, Guid operationId, Guid previousVehicleId, Guid replacementVehicleId, int mode,
        IReadOnlyCollection<BookingId> bookingIds, VehicleReplacementRequirements requirements, CancellationToken cancellationToken = default)
    {
        if (operationId == Guid.Empty || !Enum.IsDefined(typeof(VehicleReplacementMode), mode) || bookingIds.Count == 0) return null;
        Booking[] bookings = await LoadBookings(organizationId, bookingIds, false, cancellationToken);
        if (bookings.Length != bookingIds.Distinct().Count()) return null;
        CalendarResource[] resources = await LoadVehicleResources(organizationId, cancellationToken);
        var blocking = new HashSet<string>(StringComparer.Ordinal); var reviews = new HashSet<string>(StringComparer.Ordinal);
        foreach (Booking booking in bookings)
        {
            CalendarResource? previous = ResolveVehicleResource(resources, booking, previousVehicleId);
            CalendarResource? replacement = ResolveCandidateResource(resources, booking, replacementVehicleId);
            if (previous is null) { blocking.Add($"booking:{booking.Id.Value}:previous-vehicle-resource-not-found"); continue; }
            if (replacement is null) { blocking.Add($"booking:{booking.Id.Value}:replacement-vehicle-resource-not-found"); continue; }
            VehicleReplacementRequirements effectiveRequirements = requirements with { TrainingCategory = string.IsNullOrWhiteSpace(booking.TrainingCategory) ? requirements.TrainingCategory : booking.TrainingCategory };
            VehicleReplacementEligibility check = await eligibility.EvaluateAsync(organizationId, replacementVehicleId, booking.BranchId, effectiveRequirements, booking.StartAtUtc, booking.EndAtUtc, cancellationToken);
            foreach (string r in check.BlockingReasons) blocking.Add($"booking:{booking.Id.Value}:{r}"); foreach (string r in check.ExternalReviews) reviews.Add(r);
            if (!check.IsEligible || !check.TechnicalCompatibilityVerified || !check.InsuranceVerified || !check.MaintenanceVerified || !check.LocationVerified || !check.OwnershipVerified) continue;
            Booking probe = CloneForReplacement(booking, previous.Id, replacement.Id);
            BookingConflictAssessment assessment = await conflicts.AssessAsync(probe, cancellationToken);
            foreach (BookingConflict c in assessment.Conflicts) blocking.Add($"booking:{booking.Id.Value}:conflict:{c.Type}");
        }
        return new(operationId, previousVehicleId, replacementVehicleId, mode, bookings.Select(x => x.Id.Value).ToArray(), blocking.Count == 0, blocking.ToArray(), reviews.ToArray());
    }

    public async Task<Result<VehicleReplacementApplyResponse>> ApplyAsync(OrganizationId organizationId, Guid operationId, Guid previousVehicleId, Guid replacementVehicleId, int mode,
        IReadOnlyCollection<BookingId> bookingIds, VehicleReplacementRequirements requirements, string reason, CancellationToken cancellationToken = default)
    {
        if (operationId == Guid.Empty || previousVehicleId == Guid.Empty || replacementVehicleId == Guid.Empty || previousVehicleId == replacementVehicleId ||
            !Enum.IsDefined(typeof(VehicleReplacementMode), mode) || bookingIds.Count == 0)
            return Result.Failure<VehicleReplacementApplyResponse>(BookingErrors.InvalidVehicleReplacement);
        await unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            Booking[] bookings = await LoadBookings(organizationId, bookingIds, true, cancellationToken);
            if (bookings.Length != bookingIds.Distinct().Count()) { await unitOfWork.RollbackTransactionAsync(cancellationToken); return Result.Failure<VehicleReplacementApplyResponse>(BookingApplicationErrors.NotFound); }
            if (bookings.All(x => x.VehicleReplacementHistory.Any(h => h.OperationId == operationId && h.ReplacementVehicleId == replacementVehicleId)))
            { await unitOfWork.RollbackTransactionAsync(cancellationToken); return Result.Success(new VehicleReplacementApplyResponse(operationId, bookings.Length, bookings.Select(x => x.Id.Value).ToArray())); }
            CalendarResource[] resources = await LoadVehicleResources(organizationId, cancellationToken);
            var changes = new List<(Booking Booking, CalendarResource Previous, CalendarResource Replacement)>();
            foreach (Booking booking in bookings)
            {
                CalendarResource? previous = ResolveVehicleResource(resources, booking, previousVehicleId);
                CalendarResource? replacement = ResolveCandidateResource(resources, booking, replacementVehicleId);
                if (previous is null || replacement is null) { await unitOfWork.RollbackTransactionAsync(cancellationToken); return Result.Failure<VehicleReplacementApplyResponse>(BookingErrors.PreviousVehicleResourceNotFound); }
                VehicleReplacementRequirements effectiveRequirements = requirements with { TrainingCategory = string.IsNullOrWhiteSpace(booking.TrainingCategory) ? requirements.TrainingCategory : booking.TrainingCategory };
                VehicleReplacementEligibility check = await eligibility.EvaluateAsync(organizationId, replacementVehicleId, booking.BranchId, effectiveRequirements, booking.StartAtUtc, booking.EndAtUtc, cancellationToken);
                if (!check.IsEligible || !check.TechnicalCompatibilityVerified || !check.InsuranceVerified || !check.MaintenanceVerified || !check.LocationVerified || !check.OwnershipVerified)
                { await unitOfWork.RollbackTransactionAsync(cancellationToken); return Result.Failure<VehicleReplacementApplyResponse>(BookingErrors.VehicleReplacementCompatibilityNotVerified); }
                changes.Add((booking, previous, replacement));
            }
            await capacityLock.AcquireAsync(organizationId, changes.SelectMany(x => x.Booking.Resources.Select(r => r.CalendarResourceId).Append(x.Replacement.Id)).Distinct().ToArray(), cancellationToken);
            DateTimeOffset now = clock.UtcNow;
            foreach (var change in changes)
            {
                Result changed = change.Booking.ReplaceVehicle(operationId, change.Previous.Id, change.Replacement.Id, previousVehicleId, replacementVehicleId, (VehicleReplacementMode)mode, reason, now);
                if (changed.IsFailure) { await unitOfWork.RollbackTransactionAsync(cancellationToken); return Result.Failure<VehicleReplacementApplyResponse>(changed.Error); }
                Error? referenceError = await BookingReferenceRevalidation.ValidateAsync(change.Booking, referenceValidation, cancellationToken);
                if (referenceError is not null) { await unitOfWork.RollbackTransactionAsync(cancellationToken); return Result.Failure<VehicleReplacementApplyResponse>(referenceError); }
                BookingConflictAssessment assessment = await conflicts.AssessAsync(change.Booking, cancellationToken);
                if (!assessment.IsConflictFree) { await unitOfWork.RollbackTransactionAsync(cancellationToken); return Result.Failure<VehicleReplacementApplyResponse>(BookingErrors.ResourceConflict); }
            }
            await unitOfWork.CommitTransactionAsync(cancellationToken);
            return Result.Success(new VehicleReplacementApplyResponse(operationId, bookings.Length, bookings.Select(x => x.Id.Value).ToArray()));
        }
        catch { if (unitOfWork.HasActiveTransaction) await unitOfWork.RollbackTransactionAsync(cancellationToken); throw; }
    }

    private async Task<Booking[]> LoadBookings(OrganizationId organizationId, IReadOnlyCollection<BookingId> bookingIds, bool tracking, CancellationToken ct)
    {
        BookingId[] ids = bookingIds.Distinct().ToArray();
        IQueryable<Booking> query = db.Bookings.Include(x => x.Resources).Include(x => x.Participants).Include(x => x.RescheduleHistory)
            .Include(x => x.Cancellations).Include(x => x.AttendanceHistory).Include(x => x.InstructorReplacementHistory).Include(x => x.VehicleReplacementHistory)
            .Where(x => x.OrganizationId == organizationId && ids.Contains(x.Id));
        if (!tracking) query = query.AsNoTracking();
        return await query.OrderBy(x => x.StartAtUtc).ToArrayAsync(ct);
    }
    private Task<CalendarResource[]> LoadVehicleResources(OrganizationId organizationId, CancellationToken ct) => db.CalendarResources.AsNoTracking()
        .Where(x => x.OrganizationId == organizationId && (x.ResourceType == CalendarResourceType.Vehicle || x.ResourceType == CalendarResourceType.ExamVehicle) && x.Status == CalendarResourceStatus.Active).ToArrayAsync(ct);
    private static CalendarResource? ResolveVehicleResource(IEnumerable<CalendarResource> resources, Booking booking, Guid vehicleId) =>
        resources.FirstOrDefault(x => x.ExternalResourceId == vehicleId && booking.Resources.Any(r => r.CalendarResourceId == x.Id));
    private static CalendarResource? ResolveCandidateResource(IEnumerable<CalendarResource> resources, Booking booking, Guid vehicleId) => resources
        .Where(x => x.ExternalResourceId == vehicleId).OrderByDescending(x => x.BranchId == booking.BranchId).ThenBy(x => x.DisplayName).FirstOrDefault();
    private static Booking CloneForReplacement(Booking source, CalendarResourceId previousResourceId, CalendarResourceId replacementResourceId)
    {
        Booking probe = Booking.Create(source.Id, source.OrganizationId, source.BranchId, source.BookingType, source.StartAtUtc, source.EndAtUtc, source.Title).Value;
        foreach (BookingResource r in source.Resources) probe.AddResource(BookingResourceId.New(), r.CalendarResourceId == previousResourceId ? replacementResourceId : r.CalendarResourceId, r.Quantity);
        foreach (BookingParticipant p in source.Participants) probe.AddParticipant(BookingParticipantId.New(), p.ParticipantType, p.ExternalParticipantId);
        return probe;
    }
}
