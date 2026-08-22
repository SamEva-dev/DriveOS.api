using DriveOS.Modules.ExamsCertification.Domain.Registrations.Assignments.Events;
using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.ExamsCertification.Domain.Registrations.Assignments;

/// <summary>
/// Owns the definitive internal assignment of the instructor and vehicle used for an exam support operation.
/// Scheduling remains owned by BC-09 and compliance/qualification remain owned by their authoritative contexts.
/// This aggregate stores only the validated assignment snapshot and the resulting scheduling booking reference.
/// </summary>
public sealed class ExamResourceAssignment : AggregateRoot<ExamResourceAssignmentId>
{
    private ExamResourceAssignment() { }

    private ExamResourceAssignment(
        ExamResourceAssignmentId id,
        OrganizationId organizationId,
        ExamRegistrationId registrationId,
        PersonId studentId,
        ExamOperationalPlanId operationalPlanId,
        int convocationVersion,
        Guid operationId,
        string requestFingerprint,
        UserId actor,
        DateTimeOffset now) : base(id)
    {
        OrganizationId = organizationId;
        RegistrationId = registrationId;
        StudentId = studentId;
        OperationalPlanId = operationalPlanId;
        ConvocationVersion = convocationVersion;
        OperationId = operationId;
        RequestFingerprint = requestFingerprint;
        Status = ExamResourceAssignmentStatus.PendingScheduling;
        CreatedAtUtc = now.ToUniversalTime();
        CreatedByUserId = actor;
    }

    public OrganizationId OrganizationId { get; private set; }
    public ExamRegistrationId RegistrationId { get; private set; }
    public PersonId StudentId { get; private set; }
    public ExamOperationalPlanId OperationalPlanId { get; private set; }
    public int ConvocationVersion { get; private set; }
    public Guid OperationId { get; private set; }
    public string RequestFingerprint { get; private set; } = string.Empty;

    public CalendarResourceId? InstructorCalendarResourceId { get; private set; }
    public UserId? InstructorId { get; private set; }
    public bool InstructorQualificationVerified { get; private set; }
    public bool InstructorAvailabilityVerified { get; private set; }
    public IReadOnlyCollection<string> InstructorWarnings => _instructorWarnings;
    private readonly List<string> _instructorWarnings = [];

    public CalendarResourceId? VehicleCalendarResourceId { get; private set; }
    public VehicleId? VehicleId { get; private set; }
    public bool VehicleTechnicalCompatibilityVerified { get; private set; }
    public bool VehicleInsuranceVerified { get; private set; }
    public bool VehicleMaintenanceVerified { get; private set; }
    public bool VehicleLocationVerified { get; private set; }
    public bool VehicleOwnershipVerified { get; private set; }
    public IReadOnlyCollection<string> VehicleExternalReviews => _vehicleExternalReviews;
    private readonly List<string> _vehicleExternalReviews = [];

    public BookingId? SchedulingBookingId { get; private set; }
    public string? SchedulingErrorCode { get; private set; }
    public ExamResourceAssignmentStatus Status { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public UserId? CreatedByUserId { get; private set; }
    public DateTimeOffset? LastModifiedAtUtc { get; private set; }
    public UserId? LastModifiedByUserId { get; private set; }

    public static Result<ExamResourceAssignment> Create(
        ExamResourceAssignmentId id,
        OrganizationId organizationId,
        ExamRegistrationId registrationId,
        PersonId studentId,
        ExamOperationalPlanId operationalPlanId,
        int convocationVersion,
        Guid operationId,
        string requestFingerprint,
        CalendarResourceId? instructorCalendarResourceId,
        UserId? instructorId,
        bool instructorRequired,
        bool instructorQualificationVerified,
        bool instructorAvailabilityVerified,
        IEnumerable<string>? instructorWarnings,
        CalendarResourceId? vehicleCalendarResourceId,
        VehicleId? vehicleId,
        bool vehicleRequired,
        bool vehicleTechnicalCompatibilityVerified,
        bool vehicleInsuranceVerified,
        bool vehicleMaintenanceVerified,
        bool vehicleLocationVerified,
        bool vehicleOwnershipVerified,
        IEnumerable<string>? vehicleExternalReviews,
        UserId actor,
        DateTimeOffset now)
    {
        if (id.IsEmpty || organizationId.IsEmpty || registrationId.IsEmpty || studentId.IsEmpty || operationalPlanId.IsEmpty || operationId == Guid.Empty)
            return Result.Failure<ExamResourceAssignment>(ExamResourceAssignmentErrors.InvalidIdentifier);
        if (instructorRequired && (!instructorCalendarResourceId.HasValue || !instructorId.HasValue))
            return Result.Failure<ExamResourceAssignment>(ExamResourceAssignmentErrors.InstructorRequired);
        if (vehicleRequired && (!vehicleCalendarResourceId.HasValue || !vehicleId.HasValue))
            return Result.Failure<ExamResourceAssignment>(ExamResourceAssignmentErrors.VehicleRequired);
        if (instructorRequired && (!instructorQualificationVerified || !instructorAvailabilityVerified))
            return Result.Failure<ExamResourceAssignment>(ExamResourceAssignmentErrors.InstructorNotEligible);
        if (vehicleRequired && !(vehicleTechnicalCompatibilityVerified && vehicleInsuranceVerified && vehicleMaintenanceVerified && vehicleLocationVerified && vehicleOwnershipVerified))
            return Result.Failure<ExamResourceAssignment>(ExamResourceAssignmentErrors.VehicleNotEligible);

        var x = new ExamResourceAssignment(id, organizationId, registrationId, studentId, operationalPlanId, convocationVersion,
            operationId, requestFingerprint, actor, now)
        {
            InstructorCalendarResourceId = instructorCalendarResourceId,
            InstructorId = instructorId,
            InstructorQualificationVerified = instructorQualificationVerified,
            InstructorAvailabilityVerified = instructorAvailabilityVerified,
            VehicleCalendarResourceId = vehicleCalendarResourceId,
            VehicleId = vehicleId,
            VehicleTechnicalCompatibilityVerified = vehicleTechnicalCompatibilityVerified,
            VehicleInsuranceVerified = vehicleInsuranceVerified,
            VehicleMaintenanceVerified = vehicleMaintenanceVerified,
            VehicleLocationVerified = vehicleLocationVerified,
            VehicleOwnershipVerified = vehicleOwnershipVerified
        };
        x._instructorWarnings.AddRange((instructorWarnings ?? []).Where(v => !string.IsNullOrWhiteSpace(v)).Select(v => v.Trim()).Distinct(StringComparer.Ordinal));
        x._vehicleExternalReviews.AddRange((vehicleExternalReviews ?? []).Where(v => !string.IsNullOrWhiteSpace(v)).Select(v => v.Trim()).Distinct(StringComparer.Ordinal));
        x.RaiseDomainEvent(new ExamResourceAssignmentCreatedDomainEvent(id, organizationId, registrationId));
        return Result.Success(x);
    }

    public Result MarkScheduled(BookingId bookingId, UserId actor, DateTimeOffset now)
    {
        if (bookingId.IsEmpty) return Result.Failure(ExamResourceAssignmentErrors.SchedulingFailed);
        SchedulingBookingId = bookingId;
        SchedulingErrorCode = null;
        Status = ExamResourceAssignmentStatus.Assigned;
        LastModifiedAtUtc = now.ToUniversalTime();
        LastModifiedByUserId = actor;
        RaiseDomainEvent(new ExamResourcesAssignedDomainEvent(Id, OrganizationId, RegistrationId, bookingId));
        return Result.Success();
    }

    public void MarkSchedulingFailed(string errorCode, UserId actor, DateTimeOffset now)
    {
        SchedulingErrorCode = string.IsNullOrWhiteSpace(errorCode) ? "Exams.ResourceAssignment.SchedulingFailed" : errorCode.Trim();
        Status = ExamResourceAssignmentStatus.SchedulingFailed;
        LastModifiedAtUtc = now.ToUniversalTime();
        LastModifiedByUserId = actor;
    }
}
