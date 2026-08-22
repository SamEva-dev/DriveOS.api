using DriveOS.Modules.ExamsCertification.Domain.Registrations.Operations.Events;
using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.ExamsCertification.Domain.Registrations.Operations;

/// <summary>
/// Operational projection of an official exam convocation. This aggregate owns the internal logistics
/// window only; it never changes the official exam date, center or provider data held by ExamConvocation.
/// </summary>
public sealed class ExamOperationalPlan : AggregateRoot<ExamOperationalPlanId>
{
    private ExamOperationalPlan() { }

    private ExamOperationalPlan(ExamOperationalPlanId id, OrganizationId organizationId, ExamRegistrationId registrationId,
        PersonId studentId, UserId actor, DateTimeOffset now) : base(id)
    {
        OrganizationId = organizationId;
        RegistrationId = registrationId;
        StudentId = studentId;
        Status = ExamOperationalPlanStatus.Draft;
        CreatedAtUtc = now.ToUniversalTime();
        CreatedByUserId = actor;
    }

    public OrganizationId OrganizationId { get; private set; }
    public ExamRegistrationId RegistrationId { get; private set; }
    public PersonId StudentId { get; private set; }
    public int ConvocationVersion { get; private set; }
    public DateTimeOffset OfficialStartUtc { get; private set; }
    public DateTimeOffset OfficialEndUtc { get; private set; }
    public DateTimeOffset MeetingAtUtc { get; private set; }
    public DateTimeOffset OperationalWindowStartUtc { get; private set; }
    public DateTimeOffset OperationalWindowEndUtc { get; private set; }
    public int TravelBufferBeforeMinutes { get; private set; }
    public int TravelBufferAfterMinutes { get; private set; }
    public BranchId? DepartureBranchId { get; private set; }
    public bool InstructorRequired { get; private set; }
    public bool VehicleRequired { get; private set; }
    public string? MeetingInstructions { get; private set; }
    public bool HasSchedulingConflicts { get; private set; }
    public int InstructorCandidatesAvailable { get; private set; }
    public int VehicleCandidatesAvailable { get; private set; }
    public string? ConflictSummary { get; private set; }
    public DateTimeOffset LastAssessedAtUtc { get; private set; }
    public ExamOperationalPlanStatus Status { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public UserId? CreatedByUserId { get; private set; }
    public DateTimeOffset? LastModifiedAtUtc { get; private set; }
    public UserId? LastModifiedByUserId { get; private set; }

    public static Result<ExamOperationalPlan> Create(ExamOperationalPlanId id, OrganizationId organizationId,
        ExamRegistrationId registrationId, PersonId studentId, UserId actor, DateTimeOffset now)
    {
        if (id.IsEmpty || organizationId.IsEmpty || registrationId.IsEmpty || studentId.IsEmpty)
            return Result.Failure<ExamOperationalPlan>(ExamOperationalPlanErrors.InvalidIdentifier);

        var plan = new ExamOperationalPlan(id, organizationId, registrationId, studentId, actor, now);
        plan.RaiseDomainEvent(new ExamOperationalPlanCreatedDomainEvent(id, organizationId, registrationId, studentId));
        return Result.Success(plan);
    }

    public Result RefreshFromConvocation(int convocationVersion, DateTimeOffset officialStartUtc, DateTimeOffset officialEndUtc,
        DateTimeOffset meetingAtUtc, int travelBufferBeforeMinutes, int travelBufferAfterMinutes, BranchId? departureBranchId,
        bool instructorRequired, bool vehicleRequired, string? meetingInstructions, bool hasConflicts,
        int instructorCandidatesAvailable, int vehicleCandidatesAvailable, string? conflictSummary, UserId actor, DateTimeOffset now)
    {
        if (convocationVersion <= 0) return Result.Failure(ExamOperationalPlanErrors.ConvocationRequired);
        if (officialEndUtc <= officialStartUtc) return Result.Failure(ExamOperationalPlanErrors.InvalidWindow);
        if (meetingAtUtc >= officialStartUtc) return Result.Failure(ExamOperationalPlanErrors.InvalidMeetingTime);
        if (travelBufferBeforeMinutes is < 0 or > 360 || travelBufferAfterMinutes is < 0 or > 360)
            return Result.Failure(ExamOperationalPlanErrors.InvalidBuffer);

        OfficialStartUtc = officialStartUtc.ToUniversalTime();
        OfficialEndUtc = officialEndUtc.ToUniversalTime();
        MeetingAtUtc = meetingAtUtc.ToUniversalTime();
        TravelBufferBeforeMinutes = travelBufferBeforeMinutes;
        TravelBufferAfterMinutes = travelBufferAfterMinutes;
        OperationalWindowStartUtc = MeetingAtUtc.AddMinutes(-travelBufferBeforeMinutes);
        OperationalWindowEndUtc = OfficialEndUtc.AddMinutes(travelBufferAfterMinutes);
        DepartureBranchId = departureBranchId;
        InstructorRequired = instructorRequired;
        VehicleRequired = vehicleRequired;
        MeetingInstructions = string.IsNullOrWhiteSpace(meetingInstructions) ? null : meetingInstructions.Trim();
        HasSchedulingConflicts = hasConflicts;
        InstructorCandidatesAvailable = Math.Max(0, instructorCandidatesAvailable);
        VehicleCandidatesAvailable = Math.Max(0, vehicleCandidatesAvailable);
        ConflictSummary = string.IsNullOrWhiteSpace(conflictSummary) ? null : conflictSummary.Trim();
        ConvocationVersion = convocationVersion;
        LastAssessedAtUtc = now.ToUniversalTime();
        Status = hasConflicts || (instructorRequired && InstructorCandidatesAvailable == 0) || (vehicleRequired && VehicleCandidatesAvailable == 0)
            ? ExamOperationalPlanStatus.ConflictDetected
            : ExamOperationalPlanStatus.ReadyForAssignment;
        LastModifiedAtUtc = now.ToUniversalTime();
        LastModifiedByUserId = actor;
        RaiseDomainEvent(new ExamOperationalPlanRefreshedDomainEvent(Id, OrganizationId, RegistrationId, ConvocationVersion, HasSchedulingConflicts));
        return Result.Success();
    }
}
