using DriveOS.Modules.SchedulingCapacity.Application.Bookings;
using DriveOS.Modules.SchedulingCapacity.Domain.Bookings;
using DriveOS.Modules.SchedulingCapacity.Domain.CalendarResources;
using DriveOS.Modules.SchedulingCapacity.Infrastructure.Persistence;
using DriveOS.Modules.Students.Application.Instructors;
using DriveOS.Modules.Students.Application.References;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace DriveOS.Api.Integrations.SchedulingCapacity;

internal sealed class BookingReferenceValidationGateway(
    SchedulingCapacityDbContext schedulingDbContext,
    IStudentReferenceReadService students,
    IInstructorEligibilityGateway instructorEligibilityGateway) : IBookingReferenceValidationGateway
{
    public async Task<Error?> ValidateAsync(
        OrganizationId organizationId,
        BranchId? branchId,
        int bookingType,
        string? trainingCategory,
        IReadOnlyCollection<CreateBookingResourceRequest> resources,
        IReadOnlyCollection<CreateBookingParticipantRequest> participants,
        CancellationToken cancellationToken = default)
    {
        if (!Enum.IsDefined(typeof(BookingType), bookingType))
            return BookingErrors.InvalidType;

        BookingType type = (BookingType)bookingType;
        if (type is BookingType.TrainingSession or BookingType.InitialAssessment)
        {
            if (string.IsNullOrWhiteSpace(trainingCategory))
                return BookingReferenceValidationErrors.TrainingCategoryRequired;
            if (!participants.Any(x => x.ParticipantType == (int)BookingParticipantType.Student))
                return BookingReferenceValidationErrors.StudentParticipantRequired;
            if (!participants.Any(x => x.ParticipantType == (int)BookingParticipantType.Instructor))
                return BookingReferenceValidationErrors.InstructorParticipantRequired;
        }

        Guid[] resourceIds = resources.Select(x => x.CalendarResourceId).Distinct().ToArray();
        CalendarResourceId[] typedResourceIds = resourceIds.Select(static id => new CalendarResourceId(id)).ToArray();
        var calendarResourceQuery = WhereStrongIdIn(
            schedulingDbContext.CalendarResources
                .AsNoTracking()
                .Where(x => x.OrganizationId == organizationId),
            x => x.Id,
            typedResourceIds);

        CalendarResource[] calendarResources = await calendarResourceQuery.ToArrayAsync(cancellationToken);

        if (calendarResources.Length != resourceIds.Length)
            return BookingReferenceValidationErrors.CalendarResourceNotFound;

        if (branchId.HasValue && calendarResources.Any(x => x.BranchId.HasValue && x.BranchId != branchId))
            return BookingReferenceValidationErrors.ResourceBranchMismatch;

        foreach (CreateBookingResourceRequest requestedResource in resources)
        {
            CalendarResource resource = calendarResources.Single(x => x.Id.Value == requestedResource.CalendarResourceId);
            if (requestedResource.Quantity < 1 || requestedResource.Quantity > resource.Capacity)
                return BookingReferenceValidationErrors.ResourceQuantityExceedsCapacity;
        }

        Guid[] studentIds = participants
            .Where(x => x.ParticipantType == (int)BookingParticipantType.Student)
            .Select(x => x.ExternalParticipantId)
            .Distinct()
            .ToArray();

        if (studentIds.Length > 0)
        {
            IReadOnlyCollection<Guid> existingStudentIds = await students.FindExistingActiveIdsAsync(
                organizationId, studentIds, cancellationToken);

            if (existingStudentIds.Count != studentIds.Length)
                return BookingReferenceValidationErrors.StudentNotFound;
        }

        if (!string.IsNullOrWhiteSpace(trainingCategory))
        {
            foreach (Guid instructorId in participants
                .Where(x => x.ParticipantType == (int)BookingParticipantType.Instructor)
                .Select(x => x.ExternalParticipantId)
                .Distinct())
            {
                InstructorEligibility eligibility = await instructorEligibilityGateway.VerifyAsync(
                    organizationId,
                    new UserId(instructorId),
                    branchId,
                    trainingCategory.Trim(),
                    cancellationToken);
                if (!eligibility.IsEligible)
                    return BookingReferenceValidationErrors.InstructorNotEligible;
            }
        }

        foreach (CreateBookingParticipantRequest participant in participants)
        {
            CalendarResourceType? expectedResourceType = participant.ParticipantType switch
            {
                (int)BookingParticipantType.Student => CalendarResourceType.Student,
                (int)BookingParticipantType.Instructor => CalendarResourceType.Instructor,
                _ => null
            };

            if (expectedResourceType is null)
                continue;

            bool isSchedulableResource = calendarResources.Any(x =>
                x.ResourceType == expectedResourceType.Value &&
                x.ExternalResourceId == participant.ExternalParticipantId);

            if (!isSchedulableResource)
                return BookingReferenceValidationErrors.ParticipantCalendarResourceRequired;
        }

        return null;
    }
    private static IQueryable<TEntity> WhereStrongIdIn<TEntity, TId>(
        IQueryable<TEntity> query,
        Expression<Func<TEntity, TId>> selector,
        IReadOnlyCollection<TId> values)
    {
        if (values.Count == 0)
            return query.Where(_ => false);

        Expression body = Expression.Constant(false);
        foreach (TId value in values)
        {
            body = Expression.OrElse(
                body,
                Expression.Equal(selector.Body, Expression.Constant(value, typeof(TId))));
        }

        return query.Where(Expression.Lambda<Func<TEntity, bool>>(body, selector.Parameters));
    }

}
