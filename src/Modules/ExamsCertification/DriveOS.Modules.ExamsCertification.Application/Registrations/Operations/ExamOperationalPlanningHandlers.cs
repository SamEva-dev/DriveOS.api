using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Application.Abstractions.Time;
using DriveOS.Modules.ExamsCertification.Application.Persistence;
using DriveOS.Modules.ExamsCertification.Domain.Registrations;
using DriveOS.Modules.ExamsCertification.Domain.Registrations.Convocations;
using DriveOS.Modules.ExamsCertification.Domain.Registrations.Operations;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.ExamsCertification.Application.Registrations.Operations;

public sealed class RefreshExamOperationalPlanCommandHandler(
    IExamRegistrationRepository registrations, IExamConvocationRepository convocations, IExamOperationalPlanRepository plans,
    IExamOperationalPlanningGateway planningGateway, IExamsCertificationUnitOfWork unitOfWork, IClock clock)
    : ICommandHandler<RefreshExamOperationalPlanCommand, ExamOperationalPlanResponse>
{
    public async Task<Result<ExamOperationalPlanResponse>> Handle(RefreshExamOperationalPlanCommand command, CancellationToken cancellationToken)
    {
        ExamRegistration? registration = await registrations.GetByIdAsync(command.OrganizationId, command.RegistrationId, cancellationToken);
        if (registration is null) return Result.Failure<ExamOperationalPlanResponse>(ExamRegistrationErrors.NotFound);
        ExamConvocation? convocation = await convocations.GetByRegistrationAsync(command.OrganizationId, command.RegistrationId, cancellationToken);
        if (convocation?.CurrentRevision is not { } revision) return Result.Failure<ExamOperationalPlanResponse>(ExamOperationalPlanErrors.ConvocationRequired);

        DateTimeOffset meetingAt = command.MeetingAtUtc ?? convocation.InternalMeetingAtUtc ?? revision.ScheduledStartUtc.AddMinutes(-45);
        DateTimeOffset windowStart = meetingAt.AddMinutes(-command.TravelBufferBeforeMinutes);
        DateTimeOffset windowEnd = revision.ScheduledEndUtc.AddMinutes(command.TravelBufferAfterMinutes);
        ExamOperationalPlanningAssessment assessment = await planningGateway.AssessAsync(command.OrganizationId, command.DepartureBranchId, windowStart, windowEnd, cancellationToken);

        ExamOperationalPlan? plan = await plans.GetByRegistrationForUpdateAsync(command.OrganizationId, command.RegistrationId, cancellationToken);
        if (plan is null)
        {
            Result<ExamOperationalPlan> created = ExamOperationalPlan.Create(ExamOperationalPlanId.New(), command.OrganizationId, command.RegistrationId, registration.StudentId, command.ActorUserId, clock.UtcNow);
            if (created.IsFailure) return Result.Failure<ExamOperationalPlanResponse>(created.Error);
            plan = created.Value; plans.Add(plan);
        }

        bool hasConflicts = assessment.GeneralConflicts.Count > 0;
        int instructorAvailable = assessment.InstructorCandidates.Count(x => x.IsAvailable);
        int vehicleAvailable = assessment.VehicleCandidates.Count(x => x.IsAvailable);
        string? summary = BuildSummary(assessment, command.InstructorRequired, command.VehicleRequired);
        Result refreshed = plan.RefreshFromConvocation(revision.Version, revision.ScheduledStartUtc, revision.ScheduledEndUtc, meetingAt,
            command.TravelBufferBeforeMinutes, command.TravelBufferAfterMinutes, command.DepartureBranchId, command.InstructorRequired, command.VehicleRequired,
            command.MeetingInstructions ?? convocation.InternalMeetingInstructions, hasConflicts, instructorAvailable, vehicleAvailable, summary, command.ActorUserId, clock.UtcNow);
        if (refreshed.IsFailure) return Result.Failure<ExamOperationalPlanResponse>(refreshed.Error);
        await unitOfWork.CommitAsync(cancellationToken);
        return Result.Success(Map(plan));
    }

    private static string? BuildSummary(ExamOperationalPlanningAssessment a, bool instructorRequired, bool vehicleRequired)
    {
        var issues = new List<string>(a.GeneralConflicts);
        if (instructorRequired && !a.InstructorCandidates.Any(x => x.IsAvailable)) issues.Add("NoInstructorAvailable");
        if (vehicleRequired && !a.VehicleCandidates.Any(x => x.IsAvailable)) issues.Add("NoVehicleAvailable");
        return issues.Count == 0 ? null : string.Join(';', issues.Distinct(StringComparer.Ordinal));
    }

    internal static ExamOperationalPlanResponse Map(ExamOperationalPlan x) => new(x.Id.Value, x.RegistrationId.Value, x.StudentId.Value, x.ConvocationVersion,
        x.OfficialStartUtc, x.OfficialEndUtc, x.MeetingAtUtc, x.OperationalWindowStartUtc, x.OperationalWindowEndUtc, x.TravelBufferBeforeMinutes,
        x.TravelBufferAfterMinutes, x.DepartureBranchId?.Value, x.InstructorRequired, x.VehicleRequired, x.MeetingInstructions, x.HasSchedulingConflicts,
        x.InstructorCandidatesAvailable, x.VehicleCandidatesAvailable, x.ConflictSummary, x.Status.ToString(), x.LastAssessedAtUtc);
}

public sealed class GetExamOperationalPlanQueryHandler(IExamOperationalPlanRepository plans) : IQueryHandler<GetExamOperationalPlanQuery, ExamOperationalPlanResponse>
{
    public async Task<Result<ExamOperationalPlanResponse>> Handle(GetExamOperationalPlanQuery query, CancellationToken cancellationToken)
    {
        ExamOperationalPlan? plan = await plans.GetByRegistrationAsync(query.OrganizationId, query.RegistrationId, cancellationToken);
        return plan is null ? Result.Failure<ExamOperationalPlanResponse>(ExamOperationalPlanErrors.NotFound) : Result.Success(RefreshExamOperationalPlanCommandHandler.Map(plan));
    }
}

public sealed class GetExamOperationalPlanningOptionsQueryHandler(IExamConvocationRepository convocations, IExamOperationalPlanningGateway gateway)
    : IQueryHandler<GetExamOperationalPlanningOptionsQuery, ExamOperationalPlanningOptionsResponse>
{
    public async Task<Result<ExamOperationalPlanningOptionsResponse>> Handle(GetExamOperationalPlanningOptionsQuery query, CancellationToken cancellationToken)
    {
        ExamConvocation? c = await convocations.GetByRegistrationAsync(query.OrganizationId, query.RegistrationId, cancellationToken);
        if (c?.CurrentRevision is not { } r) return Result.Failure<ExamOperationalPlanningOptionsResponse>(ExamOperationalPlanErrors.ConvocationRequired);
        DateTimeOffset meeting = query.MeetingAtUtc ?? c.InternalMeetingAtUtc ?? r.ScheduledStartUtc.AddMinutes(-45);
        DateTimeOffset start = meeting.AddMinutes(-query.TravelBufferBeforeMinutes);
        DateTimeOffset end = r.ScheduledEndUtc.AddMinutes(query.TravelBufferAfterMinutes);
        ExamOperationalPlanningAssessment a = await gateway.AssessAsync(query.OrganizationId, query.DepartureBranchId, start, end, cancellationToken);
        return Result.Success(new ExamOperationalPlanningOptionsResponse(start, end, a.InstructorCandidates, a.VehicleCandidates, a.GeneralConflicts));
    }
}
