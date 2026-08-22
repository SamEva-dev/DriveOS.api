using DriveOS.Modules.ExamsCertification.Domain.Registrations.Convocations.Events;
using DriveOS.SharedKernel.Auditing;
using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.ExamsCertification.Domain.Registrations.Convocations;

/// <summary>
/// Owns the official convocation history for one exam registration.
/// Official information is append-only through immutable revisions; a provider correction never rewrites a previously received convocation.
/// Delivery/acknowledgement and the internal meeting instructions are operational metadata attached to the current official revision.
/// </summary>
public sealed class ExamConvocation : AggregateRoot<ExamConvocationId>, IAuditableEntity
{
    private readonly List<ExamConvocationRevision> _revisions = [];

    private ExamConvocation() { }

    private ExamConvocation(
        ExamConvocationId id,
        OrganizationId organizationId,
        ExamRegistrationId registrationId,
        PersonId studentId,
        UserId actor,
        DateTimeOffset now) : base(id)
    {
        OrganizationId = organizationId;
        RegistrationId = registrationId;
        StudentId = studentId;
        DeliveryStatus = ExamConvocationDeliveryStatus.Pending;
        CreatedByUserId = actor;
        CreatedAtUtc = now.ToUniversalTime();
    }

    public OrganizationId OrganizationId { get; private set; }
    public ExamRegistrationId RegistrationId { get; private set; }
    public PersonId StudentId { get; private set; }
    public int CurrentVersion { get; private set; }
    public ExamConvocationDeliveryStatus DeliveryStatus { get; private set; }
    public ExamConvocationDeliveryChannel? DeliveryChannel { get; private set; }
    public DateTimeOffset? DeliveredAtUtc { get; private set; }
    public UserId? DeliveredByUserId { get; private set; }
    public DateTimeOffset? AcknowledgedAtUtc { get; private set; }
    public DateTimeOffset? InternalMeetingAtUtc { get; private set; }
    public string? InternalMeetingInstructions { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public UserId? CreatedByUserId { get; private set; }
    public DateTimeOffset? LastModifiedAtUtc { get; private set; }
    public UserId? LastModifiedByUserId { get; private set; }
    public IReadOnlyCollection<ExamConvocationRevision> Revisions => _revisions.AsReadOnly();
    public ExamConvocationRevision? CurrentRevision => _revisions.OrderByDescending(x => x.Version).FirstOrDefault();

    public static Result<ExamConvocation> Create(
        ExamConvocationId id,
        OrganizationId organizationId,
        ExamRegistrationId registrationId,
        PersonId studentId,
        UserId actor,
        DateTimeOffset now)
    {
        if (id.IsEmpty || organizationId.IsEmpty || registrationId.IsEmpty || studentId.IsEmpty)
            return Result.Failure<ExamConvocation>(ExamConvocationErrors.InvalidIdentifier);

        var convocation = new ExamConvocation(id, organizationId, registrationId, studentId, actor, now);
        convocation.RaiseDomainEvent(new ExamConvocationCreatedDomainEvent(id, organizationId, registrationId, studentId));
        return Result.Success(convocation);
    }

    public Result<ExamConvocationRevision> ReceiveOfficialRevision(
        ExamConvocationRevisionId revisionId,
        ExamCenterId examCenterId,
        string centerName,
        string? centerAddress,
        string timeZoneId,
        DateTimeOffset scheduledStartUtc,
        DateTimeOffset scheduledEndUtc,
        string providerCode,
        string? officialReference,
        string? candidateReference,
        string? instructions,
        string? requiredDocuments,
        string? providerPayloadReference,
        Guid operationId,
        string requestFingerprint,
        UserId actor,
        DateTimeOffset receivedAtUtc)
    {
        if (revisionId.IsEmpty || examCenterId.IsEmpty)
            return Result.Failure<ExamConvocationRevision>(ExamConvocationErrors.CenterRequired);
        if (scheduledEndUtc <= scheduledStartUtc)
            return Result.Failure<ExamConvocationRevision>(ExamConvocationErrors.InvalidPeriod);
        if (string.IsNullOrWhiteSpace(providerCode))
            return Result.Failure<ExamConvocationRevision>(ExamConvocationErrors.ProviderRequired);
        if (operationId == Guid.Empty || string.IsNullOrWhiteSpace(requestFingerprint))
            return Result.Failure<ExamConvocationRevision>(ExamConvocationErrors.InvalidOperation);

        ExamConvocationRevision? replay = _revisions.SingleOrDefault(x => x.OperationId == operationId);
        if (replay is not null)
        {
            return string.Equals(replay.RequestFingerprint, requestFingerprint, StringComparison.Ordinal)
                ? Result.Success(replay)
                : Result.Failure<ExamConvocationRevision>(ExamConvocationErrors.OperationConflict);
        }

        int version = CurrentVersion + 1;
        var revision = ExamConvocationRevision.Create(
            revisionId, Id, version, examCenterId, centerName, centerAddress, timeZoneId,
            scheduledStartUtc, scheduledEndUtc, providerCode, officialReference, candidateReference,
            instructions, requiredDocuments, providerPayloadReference, operationId, requestFingerprint, receivedAtUtc);

        _revisions.Add(revision);
        CurrentVersion = version;
        DeliveryStatus = ExamConvocationDeliveryStatus.Pending;
        DeliveryChannel = null;
        DeliveredAtUtc = null;
        DeliveredByUserId = null;
        AcknowledgedAtUtc = null;
        InternalMeetingAtUtc = null;
        InternalMeetingInstructions = null;
        Touch(actor, receivedAtUtc);

        RaiseDomainEvent(new ExamConvocationRevisionReceivedDomainEvent(
            Id, revision.Id, OrganizationId, RegistrationId, version, revision.ScheduledStartUtc));
        return Result.Success(revision);
    }

    public Result SetInternalMeeting(DateTimeOffset? meetingAtUtc, string? instructions, UserId actor, DateTimeOffset now)
    {
        ExamConvocationRevision? current = CurrentRevision;
        if (current is null) return Result.Failure(ExamConvocationErrors.NotFound);
        if (meetingAtUtc.HasValue && meetingAtUtc.Value >= current.ScheduledStartUtc)
            return Result.Failure(ExamConvocationErrors.InvalidInternalMeetingTime);

        InternalMeetingAtUtc = meetingAtUtc?.ToUniversalTime();
        InternalMeetingInstructions = string.IsNullOrWhiteSpace(instructions) ? null : instructions.Trim();
        Touch(actor, now);
        return Result.Success();
    }

    public Result MarkDelivered(ExamConvocationDeliveryChannel channel, UserId actor, DateTimeOffset now)
    {
        if (CurrentRevision is null) return Result.Failure(ExamConvocationErrors.NotFound);
        if (DeliveryStatus == ExamConvocationDeliveryStatus.Acknowledged)
            return Result.Failure(ExamConvocationErrors.DeliveryAlreadyAcknowledged);

        DeliveryStatus = ExamConvocationDeliveryStatus.Delivered;
        DeliveryChannel = channel;
        DeliveredAtUtc = now.ToUniversalTime();
        DeliveredByUserId = actor;
        Touch(actor, now);
        RaiseDomainEvent(new ExamConvocationDeliveredDomainEvent(Id, OrganizationId, RegistrationId, CurrentVersion, channel, DeliveredAtUtc.Value));
        return Result.Success();
    }

    public Result MarkAcknowledged(UserId actor, DateTimeOffset now)
    {
        if (DeliveryStatus != ExamConvocationDeliveryStatus.Delivered || DeliveredAtUtc is null)
            return Result.Failure(ExamConvocationErrors.MustBeDeliveredFirst);

        DeliveryStatus = ExamConvocationDeliveryStatus.Acknowledged;
        AcknowledgedAtUtc = now.ToUniversalTime();
        Touch(actor, now);
        RaiseDomainEvent(new ExamConvocationAcknowledgedDomainEvent(Id, OrganizationId, RegistrationId, CurrentVersion, AcknowledgedAtUtc.Value));
        return Result.Success();
    }

    private void Touch(UserId actor, DateTimeOffset now)
    {
        LastModifiedAtUtc = now.ToUniversalTime();
        LastModifiedByUserId = actor;
    }

    public void SetCreatedAudit(DateTimeOffset createdAtUtc, UserId? createdByUserId)
    {
        if (CreatedAtUtc != default)
            return;

        CreatedAtUtc = createdAtUtc.ToUniversalTime();
        CreatedByUserId = createdByUserId;
    }

    public void SetModifiedAudit(DateTimeOffset modifiedAtUtc, UserId? modifiedByUserId)
    {
        LastModifiedAtUtc = modifiedAtUtc.ToUniversalTime();
        LastModifiedByUserId = modifiedByUserId;
    }
}

public sealed class ExamConvocationRevision : Entity<ExamConvocationRevisionId>
{
    private ExamConvocationRevision() { }

    private ExamConvocationRevision(
        ExamConvocationRevisionId id,
        ExamConvocationId convocationId,
        int version,
        ExamCenterId examCenterId,
        string centerName,
        string? centerAddress,
        string timeZoneId,
        DateTimeOffset scheduledStartUtc,
        DateTimeOffset scheduledEndUtc,
        string providerCode,
        string? officialReference,
        string? candidateReference,
        string? instructions,
        string? requiredDocuments,
        string? providerPayloadReference,
        Guid operationId,
        string requestFingerprint,
        DateTimeOffset receivedAtUtc) : base(id)
    {
        ConvocationId = convocationId;
        Version = version;
        ExamCenterId = examCenterId;
        CenterName = centerName.Trim();
        CenterAddress = string.IsNullOrWhiteSpace(centerAddress) ? null : centerAddress.Trim();
        TimeZoneId = string.IsNullOrWhiteSpace(timeZoneId) ? "UTC" : timeZoneId.Trim();
        ScheduledStartUtc = scheduledStartUtc.ToUniversalTime();
        ScheduledEndUtc = scheduledEndUtc.ToUniversalTime();
        ProviderCode = providerCode.Trim();
        OfficialReference = string.IsNullOrWhiteSpace(officialReference) ? null : officialReference.Trim();
        CandidateReference = string.IsNullOrWhiteSpace(candidateReference) ? null : candidateReference.Trim();
        Instructions = string.IsNullOrWhiteSpace(instructions) ? null : instructions.Trim();
        RequiredDocuments = string.IsNullOrWhiteSpace(requiredDocuments) ? null : requiredDocuments.Trim();
        ProviderPayloadReference = string.IsNullOrWhiteSpace(providerPayloadReference) ? null : providerPayloadReference.Trim();
        OperationId = operationId;
        RequestFingerprint = requestFingerprint.Trim();
        ReceivedAtUtc = receivedAtUtc.ToUniversalTime();
    }

    public ExamConvocationId ConvocationId { get; private set; }
    public int Version { get; private set; }
    public ExamCenterId ExamCenterId { get; private set; }
    public string CenterName { get; private set; } = string.Empty;
    public string? CenterAddress { get; private set; }
    public string TimeZoneId { get; private set; } = "UTC";
    public DateTimeOffset ScheduledStartUtc { get; private set; }
    public DateTimeOffset ScheduledEndUtc { get; private set; }
    public string ProviderCode { get; private set; } = string.Empty;
    public string? OfficialReference { get; private set; }
    public string? CandidateReference { get; private set; }
    public string? Instructions { get; private set; }
    public string? RequiredDocuments { get; private set; }
    public string? ProviderPayloadReference { get; private set; }
    public Guid OperationId { get; private set; }
    public string RequestFingerprint { get; private set; } = string.Empty;
    public DateTimeOffset ReceivedAtUtc { get; private set; }

    internal static ExamConvocationRevision Create(
        ExamConvocationRevisionId id, ExamConvocationId convocationId, int version, ExamCenterId examCenterId,
        string centerName, string? centerAddress, string timeZoneId, DateTimeOffset scheduledStartUtc, DateTimeOffset scheduledEndUtc,
        string providerCode, string? officialReference, string? candidateReference, string? instructions, string? requiredDocuments,
        string? providerPayloadReference, Guid operationId, string requestFingerprint, DateTimeOffset receivedAtUtc) =>
        new(id, convocationId, version, examCenterId, centerName, centerAddress, timeZoneId, scheduledStartUtc, scheduledEndUtc,
            providerCode, officialReference, candidateReference, instructions, requiredDocuments, providerPayloadReference,
            operationId, requestFingerprint, receivedAtUtc);
}
