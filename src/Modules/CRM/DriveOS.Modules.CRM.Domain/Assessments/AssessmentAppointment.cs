using DriveOS.Modules.CRM.Domain.Assessments.Events;
using DriveOS.Modules.CRM.Domain.Leads;
using DriveOS.SharedKernel.Auditing;
using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.CRM.Domain.Assessments;

public sealed class AssessmentAppointment : AggregateRoot<AssessmentAppointmentId>, IAuditableEntity
{
    private AssessmentAppointment() { }

    private AssessmentAppointment(
        AssessmentAppointmentId id,
        OrganizationId organizationId,
        LeadId leadId,
        BranchId? branchId,
        DateTimeOffset startsAtUtc,
        DateTimeOffset endsAtUtc,
        AssessmentType type,
        AssessmentDeliveryMode deliveryMode,
        AssessmentLocationKind locationKind,
        string? locationDetails,
        UserId? evaluatorUserId,
        Guid? vehicleId,
        Guid? roomId,
        Guid? simulatorId,
        decimal? priceAmount,
        string? priceCurrency,
        string? notes
    )
        : base(id)
    {
        OrganizationId = organizationId;
        LeadId = leadId;
        BranchId = branchId;
        StartsAtUtc = startsAtUtc;
        EndsAtUtc = endsAtUtc;
        Type = type;
        DeliveryMode = deliveryMode;
        LocationKind = locationKind;
        LocationDetails = locationDetails;
        EvaluatorUserId = evaluatorUserId;
        VehicleId = vehicleId;
        RoomId = roomId;
        SimulatorId = simulatorId;
        PriceAmount = priceAmount;
        PriceCurrency = priceCurrency;
        Notes = notes;
        Status = AssessmentAppointmentStatus.Scheduled;
    }

    public OrganizationId OrganizationId { get; private set; }
    public LeadId LeadId { get; private set; }
    public BranchId? BranchId { get; private set; }
    public DateTimeOffset StartsAtUtc { get; private set; }
    public DateTimeOffset EndsAtUtc { get; private set; }
    public AssessmentType Type { get; private set; }
    public AssessmentDeliveryMode DeliveryMode { get; private set; }
    public AssessmentLocationKind LocationKind { get; private set; }
    public string? LocationDetails { get; private set; }
    public UserId? EvaluatorUserId { get; private set; }
    public Guid? VehicleId { get; private set; }
    public Guid? RoomId { get; private set; }
    public Guid? SimulatorId { get; private set; }
    public decimal? PriceAmount { get; private set; }
    public string? PriceCurrency { get; private set; }
    public string? Notes { get; private set; }
    public AssessmentAppointmentStatus Status { get; private set; }
    public DateTimeOffset? ClosedAtUtc { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public UserId? CreatedByUserId { get; private set; }
    public DateTimeOffset? LastModifiedAtUtc { get; private set; }
    public UserId? LastModifiedByUserId { get; private set; }

    public static Result<AssessmentAppointment> Schedule(
        AssessmentAppointmentId id,
        OrganizationId organizationId,
        LeadId leadId,
        BranchId? branchId,
        DateTimeOffset startsAtUtc,
        DateTimeOffset endsAtUtc,
        AssessmentType type,
        AssessmentDeliveryMode deliveryMode,
        AssessmentLocationKind locationKind,
        string? locationDetails,
        UserId? evaluatorUserId,
        Guid? vehicleId,
        Guid? roomId,
        Guid? simulatorId,
        decimal? priceAmount,
        string? priceCurrency,
        string? notes
    )
    {
        if (id == AssessmentAppointmentId.Empty)
            return Result.Failure<AssessmentAppointment>(
                AssessmentAppointmentErrors.InvalidIdentifier
            );
        if (startsAtUtc == default || endsAtUtc <= startsAtUtc)
            return Result.Failure<AssessmentAppointment>(AssessmentAppointmentErrors.InvalidPeriod);

        Result detailsValidation = ValidateSchedulingDetails(
            branchId,
            deliveryMode,
            locationKind,
            locationDetails,
            vehicleId,
            roomId,
            simulatorId,
            priceAmount,
            priceCurrency
        );
        if (detailsValidation.IsFailure)
            return Result.Failure<AssessmentAppointment>(detailsValidation.Error);

        string? normalizedNotes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
        if (normalizedNotes?.Length > 2000)
            return Result.Failure<AssessmentAppointment>(AssessmentAppointmentErrors.NotesTooLong);

        var appointment = new AssessmentAppointment(
            id,
            organizationId,
            leadId,
            branchId,
            startsAtUtc.ToUniversalTime(),
            endsAtUtc.ToUniversalTime(),
            type,
            deliveryMode,
            locationKind,
            Normalize(locationDetails),
            evaluatorUserId,
            vehicleId,
            roomId,
            simulatorId,
            priceAmount,
            NormalizeCurrency(priceCurrency),
            normalizedNotes
        );

        appointment.RaiseDomainEvent(
            new AssessmentAppointmentScheduledDomainEvent(
                appointment.Id,
                appointment.OrganizationId,
                appointment.LeadId,
                appointment.BranchId,
                appointment.StartsAtUtc,
                appointment.EndsAtUtc
            )
        );

        return Result.Success(appointment);
    }

    private static Result ValidateSchedulingDetails(
        BranchId? branchId,
        AssessmentDeliveryMode deliveryMode,
        AssessmentLocationKind locationKind,
        string? locationDetails,
        Guid? vehicleId,
        Guid? roomId,
        Guid? simulatorId,
        decimal? priceAmount,
        string? priceCurrency
    )
    {
        if (locationKind == AssessmentLocationKind.Branch && !branchId.HasValue)
            return Result.Failure(AssessmentAppointmentErrors.BranchRequired);
        if (
            locationKind != AssessmentLocationKind.Branch
            && string.IsNullOrWhiteSpace(locationDetails)
        )
            return Result.Failure(AssessmentAppointmentErrors.LocationDetailsRequired);
        if (Normalize(locationDetails)?.Length > 500)
            return Result.Failure(AssessmentAppointmentErrors.LocationDetailsTooLong);
        if (
            deliveryMode == AssessmentDeliveryMode.Remote
            && locationKind != AssessmentLocationKind.VideoConference
        )
            return Result.Failure(AssessmentAppointmentErrors.InvalidRemoteLocation);
        if (locationKind == AssessmentLocationKind.Simulator && !simulatorId.HasValue)
            return Result.Failure(AssessmentAppointmentErrors.SimulatorRequired);
        if (vehicleId == Guid.Empty || roomId == Guid.Empty || simulatorId == Guid.Empty)
            return Result.Failure(AssessmentAppointmentErrors.InvalidResourceIdentifier);
        if (priceAmount is < 0)
            return Result.Failure(AssessmentAppointmentErrors.InvalidPrice);
        if (priceAmount.HasValue != !string.IsNullOrWhiteSpace(priceCurrency))
            return Result.Failure(AssessmentAppointmentErrors.IncompletePrice);
        if (priceCurrency is not null && NormalizeCurrency(priceCurrency)?.Length != 3)
            return Result.Failure(AssessmentAppointmentErrors.InvalidCurrency);

        return Result.Success();
    }

    private static string? Normalize(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static string? NormalizeCurrency(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim().ToUpperInvariant();

    public Result Reschedule(DateTimeOffset startsAtUtc, DateTimeOffset endsAtUtc)
    {
        if (!IsActive(Status))
            return Result.Failure(AssessmentAppointmentErrors.AlreadyClosed);
        if (startsAtUtc == default || endsAtUtc <= startsAtUtc)
            return Result.Failure(AssessmentAppointmentErrors.InvalidPeriod);

        StartsAtUtc = startsAtUtc.ToUniversalTime();
        EndsAtUtc = endsAtUtc.ToUniversalTime();
        Status = AssessmentAppointmentStatus.Rescheduled;

        RaiseDomainEvent(
            new AssessmentAppointmentRescheduledDomainEvent(
                Id,
                OrganizationId,
                StartsAtUtc,
                EndsAtUtc
            )
        );

        return Result.Success();
    }

    public Result Complete(DateTimeOffset nowUtc)
    {
        Result result = Close(AssessmentAppointmentStatus.Completed, nowUtc);
        if (result.IsFailure)
            return result;
        RaiseDomainEvent(
            new AssessmentAppointmentCompletedDomainEvent(Id, OrganizationId, ClosedAtUtc!.Value)
        );
        return Result.Success();
    }

    public Result Cancel(DateTimeOffset nowUtc)
    {
        Result result = Close(AssessmentAppointmentStatus.Cancelled, nowUtc);
        if (result.IsFailure)
            return result;

        RaiseDomainEvent(
            new AssessmentAppointmentCancelledDomainEvent(Id, OrganizationId, ClosedAtUtc!.Value)
        );

        return Result.Success();
    }

    public Result MarkNoShow(DateTimeOffset nowUtc) =>
        Close(AssessmentAppointmentStatus.NoShow, nowUtc);

    private Result Close(AssessmentAppointmentStatus target, DateTimeOffset nowUtc)
    {
        if (!IsActive(Status))
            return Result.Failure(AssessmentAppointmentErrors.AlreadyClosed);

        Status = target;
        ClosedAtUtc = nowUtc.ToUniversalTime();
        return Result.Success();
    }

    private static bool IsActive(AssessmentAppointmentStatus status) =>
        status
            is AssessmentAppointmentStatus.Scheduled
                or AssessmentAppointmentStatus.Confirmed
                or AssessmentAppointmentStatus.Rescheduled;

    public void SetCreatedAudit(DateTimeOffset at, UserId? by)
    {
        if (CreatedAtUtc == default)
        {
            CreatedAtUtc = at;
            CreatedByUserId = by;
        }
    }

    public void SetModifiedAudit(DateTimeOffset at, UserId? by)
    {
        LastModifiedAtUtc = at;
        LastModifiedByUserId = by;
    }
}
