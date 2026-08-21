using DriveOS.Modules.TrainingDelivery.Domain.Incidents.Events;
using DriveOS.SharedKernel.Auditing;
using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;
using System.Security.Cryptography;
using System.Text;

namespace DriveOS.Modules.TrainingDelivery.Domain.Incidents;

/// <summary>
/// Aggregate root representing a real operational incident linked to an executed training session.
/// It is intentionally distinct from a routine pedagogical/safety intervention: an incident has its own severity,
/// participants, immediate containment actions, evidence, escalation workflow and immutable audit history.
/// </summary>
public sealed class TrainingIncident : AggregateRoot<TrainingIncidentId>, IAuditableEntity
{
    private readonly List<TrainingIncidentParticipant> _participants = [];
    private readonly List<TrainingIncidentEvidence> _evidence = [];
    private readonly List<TrainingIncidentHistoryEntry> _history = [];
    private TrainingIncident() { }
    private TrainingIncident(TrainingIncidentId id) : base(id) { }

    /// <summary>Tenant owning the incident and defining the mandatory security boundary for every operation.</summary>
    public OrganizationId OrganizationId { get; private set; }
    /// <summary>Training Delivery session during which, or in relation to which, the incident occurred.</summary>
    public TrainingSessionId TrainingSessionId { get; private set; }
    /// <summary>Student concerned by the source session. This is a session snapshot, not a new owner of student identity.</summary>
    public PersonId StudentId { get; private set; }
    /// <summary>Instructor actually involved when known, otherwise the current/planned instructor snapshot.</summary>
    public UserId InstructorId { get; private set; }
    /// <summary>Vehicle actually involved when known. Fleet remains authoritative for the vehicle and its technical state.</summary>
    public Guid? VehicleId { get; private set; }
    /// <summary>Branch context in which the incident occurred.</summary>
    public BranchId? BranchId { get; private set; }
    /// <summary>Organization that effectively performed the training service.</summary>
    public OrganizationId PerformingOrganizationId { get; private set; }
    /// <summary>Functional classification of the incident.</summary>
    public TrainingIncidentType IncidentType { get; private set; }
    /// <summary>Operational severity used to prioritize treatment and determine whether escalation is mandatory.</summary>
    public TrainingIncidentSeverity Severity { get; private set; }
    /// <summary>Current workflow status. Historical transitions are never inferred solely from this current value.</summary>
    public TrainingIncidentStatus Status { get; private set; }
    /// <summary>Actual UTC instant at which the incident occurred.</summary>
    public DateTimeOffset OccurredAtUtc { get; private set; }
    /// <summary>Factual description of what happened.</summary>
    public string Description { get; private set; } = string.Empty;
    /// <summary>Immediate containment/safety actions performed at the time of the incident.</summary>
    public string ImmediateActions { get; private set; } = string.Empty;
    /// <summary>Whether the incident needs an explicit management/operational escalation workflow.</summary>
    public bool EscalationRequired { get; private set; }
    /// <summary>Whether Fleet must receive the incident because a vehicle or vehicle condition is materially involved.</summary>
    public bool RequiresFleetFollowUp { get; private set; }
    /// <summary>Whether Compliance/Audit must receive the incident for formal review.</summary>
    public bool RequiresComplianceFollowUp { get; private set; }
    /// <summary>UTC instant at which the incident was formally escalated.</summary>
    public DateTimeOffset? EscalatedAtUtc { get; private set; }
    /// <summary>Authenticated user who formally escalated the incident.</summary>
    public UserId? EscalatedByUserId { get; private set; }
    /// <summary>Final factual resolution recorded before closure.</summary>
    public string? Resolution { get; private set; }
    /// <summary>UTC instant of the resolution decision.</summary>
    public DateTimeOffset? ResolvedAtUtc { get; private set; }
    /// <summary>Authenticated user who resolved the incident.</summary>
    public UserId? ResolvedByUserId { get; private set; }
    /// <summary>UTC instant at which the incident workflow was finally closed.</summary>
    public DateTimeOffset? ClosedAtUtc { get; private set; }
    /// <summary>Authenticated user who closed the incident.</summary>
    public UserId? ClosedByUserId { get; private set; }
    /// <summary>Idempotency operation used to create the incident from a mobile or online client.</summary>
    public Guid ReportOperationId { get; private set; }
    /// <summary>Deterministic fingerprint associated with <see cref="ReportOperationId"/> to detect conflicting retries.</summary>
    public string ReportRequestFingerprint { get; private set; } = string.Empty;
    /// <summary>People/resources explicitly involved in the incident.</summary>
    public IReadOnlyCollection<TrainingIncidentParticipant> Participants => _participants.AsReadOnly();
    /// <summary>Document references supporting the incident. Files remain owned by the document platform.</summary>
    public IReadOnlyCollection<TrainingIncidentEvidence> Evidence => _evidence.AsReadOnly();
    /// <summary>Append-only audit trail of the incident lifecycle and idempotent mutations.</summary>
    public IReadOnlyCollection<TrainingIncidentHistoryEntry> History => _history.AsReadOnly();
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public UserId? CreatedByUserId { get; private set; }
    public DateTimeOffset? LastModifiedAtUtc { get; private set; }
    public UserId? LastModifiedByUserId { get; private set; }

    public static Result<TrainingIncident> Report(
        TrainingIncidentId id, OrganizationId organizationId, TrainingSessionId sessionId,
        PersonId studentId, UserId instructorId, Guid? vehicleId, BranchId? branchId, OrganizationId performingOrganizationId,
        Guid operationId, TrainingIncidentType type, TrainingIncidentSeverity severity, DateTimeOffset occurredAtUtc,
        string description, string immediateActions, IEnumerable<(TrainingIncidentParticipantType Type, Guid? ReferenceId, string? Label)> additionalParticipants,
        UserId actor, DateTimeOffset now)
    {
        if (id.IsEmpty || organizationId.IsEmpty || sessionId.IsEmpty || studentId.IsEmpty || instructorId.IsEmpty || performingOrganizationId.IsEmpty || actor.IsEmpty || operationId == Guid.Empty)
            return Result.Failure<TrainingIncident>(TrainingIncidentErrors.Invalid);
        if (!Enum.IsDefined(type) || !Enum.IsDefined(severity)) return Result.Failure<TrainingIncident>(TrainingIncidentErrors.Invalid);
        var desc = NormalizeRequired(description, 5000); var actions = NormalizeRequired(immediateActions, 3000);
        if (desc is null || actions is null) return Result.Failure<TrainingIncident>(TrainingIncidentErrors.Invalid);
        var occurred = occurredAtUtc.ToUniversalTime(); var utcNow = now.ToUniversalTime();
        if (occurred > utcNow.AddMinutes(5)) return Result.Failure<TrainingIncident>(TrainingIncidentErrors.OccurredAtInvalid);

        var extra = (additionalParticipants ?? []).Select(p => $"{(int)p.Type}:{p.ReferenceId}:{p.Label?.Trim()}").OrderBy(x => x).ToArray();
        string fingerprint = Fingerprint((int)type, (int)severity, occurred.ToString("O"), desc, actions, studentId.Value, instructorId.Value, vehicleId, branchId?.Value, string.Join(";", extra));
        var incident = new TrainingIncident(id)
        {
            OrganizationId=organizationId, TrainingSessionId=sessionId, StudentId=studentId, InstructorId=instructorId,
            VehicleId=vehicleId, BranchId=branchId, PerformingOrganizationId=performingOrganizationId,
            ReportOperationId=operationId, ReportRequestFingerprint=fingerprint, IncidentType=type, Severity=severity,
            Status=TrainingIncidentStatus.Open, OccurredAtUtc=occurred, Description=desc, ImmediateActions=actions,
            EscalationRequired=severity is TrainingIncidentSeverity.High or TrainingIncidentSeverity.Critical,
            RequiresFleetFollowUp=type is TrainingIncidentType.VehicleIssue or TrainingIncidentType.Accident or TrainingIncidentType.PropertyDamage,
            RequiresComplianceFollowUp=severity is TrainingIncidentSeverity.High or TrainingIncidentSeverity.Critical || type is TrainingIncidentType.DataPrivacy or TrainingIncidentType.Accident,
            CreatedAtUtc=utcNow, CreatedByUserId=actor
        };
        incident.AddParticipantInternal(TrainingIncidentParticipantType.Student, studentId.Value, null);
        incident.AddParticipantInternal(TrainingIncidentParticipantType.Instructor, instructorId.Value, null);
        if (vehicleId.HasValue) incident.AddParticipantInternal(TrainingIncidentParticipantType.Vehicle, vehicleId.Value, null);
        foreach (var p in additionalParticipants ?? [])
        {
            if (!Enum.IsDefined(p.Type) || (!p.ReferenceId.HasValue && string.IsNullOrWhiteSpace(p.Label)))
                return Result.Failure<TrainingIncident>(TrainingIncidentErrors.ParticipantInvalid);
            incident.AddParticipantInternal(p.Type, p.ReferenceId, p.Label);
        }
        incident.AddHistory(operationId, fingerprint, TrainingIncidentHistoryAction.Reported, TrainingIncidentStatus.Open, TrainingIncidentStatus.Open, null, actor, utcNow);
        incident.RaiseDomainEvent(new TrainingIncidentReportedDomainEvent(id, organizationId, sessionId, type, severity));
        if (incident.EscalationRequired)
            incident.RaiseDomainEvent(new TrainingIncidentEscalationRequestedDomainEvent(id, organizationId, sessionId, severity, incident.RequiresFleetFollowUp, incident.RequiresComplianceFollowUp));
        return Result.Success(incident);
    }


    public bool MatchesReportRetry(TrainingIncidentType type, TrainingIncidentSeverity severity, DateTimeOffset occurredAtUtc, string description, string immediateActions, IEnumerable<(TrainingIncidentParticipantType Type, Guid? ReferenceId, string? Label)> additionalParticipants)
    {
        var desc=NormalizeRequired(description,5000); var actions=NormalizeRequired(immediateActions,3000);
        if(desc is null||actions is null)return false;
        var extra=(additionalParticipants??[]).Select(p=>$"{(int)p.Type}:{p.ReferenceId}:{p.Label?.Trim()}").OrderBy(x=>x).ToArray();
        string fp=Fingerprint((int)type,(int)severity,occurredAtUtc.ToUniversalTime().ToString("O"),desc,actions,StudentId.Value,InstructorId.Value,VehicleId,BranchId?.Value,string.Join(";",extra));
        return ReportRequestFingerprint==fp;
    }

    public Result AddEvidence(Guid operationId, Guid documentId, string evidenceType, string? description, UserId actor, DateTimeOffset now)
    {
        string normalizedType=evidenceType?.Trim() ?? string.Empty;
        if (documentId==Guid.Empty || normalizedType.Length is <1 or >100 || description?.Length>1000 || actor.IsEmpty) return Result.Failure(TrainingIncidentErrors.EvidenceInvalid);
        string fp=Fingerprint(documentId, normalizedType, description);
        var idem=CheckMutation(operationId, fp); if (idem is not null) return idem;
        _evidence.Add(new TrainingIncidentEvidence(TrainingIncidentEvidenceId.New(), Id, documentId, normalizedType, description, actor, now));
        AddHistory(operationId,fp,TrainingIncidentHistoryAction.EvidenceAdded,Status,Status,"evidence",actor,now); Touch(actor,now); return Result.Success();
    }

    public Result Escalate(Guid operationId, string reason, UserId actor, DateTimeOffset now)
    {
        string? normalized=NormalizeRequired(reason,2000); if(normalized is null||actor.IsEmpty) return Result.Failure(TrainingIncidentErrors.Invalid);
        string fp=Fingerprint("escalate",normalized); var idem=CheckMutation(operationId,fp); if(idem is not null)return idem;
        if(Status is TrainingIncidentStatus.Resolved or TrainingIncidentStatus.Closed) return Result.Failure(TrainingIncidentErrors.AlreadyResolved);
        var from=Status; Status=TrainingIncidentStatus.Escalated; EscalationRequired=false; EscalatedAtUtc=now.ToUniversalTime(); EscalatedByUserId=actor;
        AddHistory(operationId,fp,TrainingIncidentHistoryAction.Escalated,from,Status,normalized,actor,now); Touch(actor,now);
        RaiseDomainEvent(new TrainingIncidentEscalatedDomainEvent(Id,OrganizationId,TrainingSessionId,actor)); return Result.Success();
    }

    public Result StartReview(Guid operationId, string? reason, UserId actor, DateTimeOffset now)
    {
        string fp=Fingerprint("review",reason); var idem=CheckMutation(operationId,fp); if(idem is not null)return idem;
        if(Status is TrainingIncidentStatus.Resolved or TrainingIncidentStatus.Closed) return Result.Failure(TrainingIncidentErrors.AlreadyResolved);
        var from=Status; Status=TrainingIncidentStatus.UnderReview; AddHistory(operationId,fp,TrainingIncidentHistoryAction.ReviewStarted,from,Status,reason,actor,now); Touch(actor,now); return Result.Success();
    }

    public Result Resolve(Guid operationId, string resolution, UserId actor, DateTimeOffset now)
    {
        string? normalized=NormalizeRequired(resolution,4000); if(normalized is null||actor.IsEmpty)return Result.Failure(TrainingIncidentErrors.ResolutionRequired);
        string fp=Fingerprint("resolve",normalized); var idem=CheckMutation(operationId,fp); if(idem is not null)return idem;
        if(Status==TrainingIncidentStatus.Closed)return Result.Failure(TrainingIncidentErrors.AlreadyClosed);
        if(Status==TrainingIncidentStatus.Resolved)return Result.Failure(TrainingIncidentErrors.AlreadyResolved);
        if(Severity==TrainingIncidentSeverity.Critical && !EscalatedAtUtc.HasValue)return Result.Failure(TrainingIncidentErrors.CriticalMustBeEscalated);
        var from=Status; Status=TrainingIncidentStatus.Resolved; Resolution=normalized; ResolvedAtUtc=now.ToUniversalTime(); ResolvedByUserId=actor;
        AddHistory(operationId,fp,TrainingIncidentHistoryAction.Resolved,from,Status,normalized,actor,now); Touch(actor,now);
        RaiseDomainEvent(new TrainingIncidentResolvedDomainEvent(Id,OrganizationId,TrainingSessionId,actor)); return Result.Success();
    }

    public Result Close(Guid operationId, string? note, UserId actor, DateTimeOffset now)
    {
        string fp=Fingerprint("close",note); var idem=CheckMutation(operationId,fp); if(idem is not null)return idem;
        if(Status==TrainingIncidentStatus.Closed)return Result.Failure(TrainingIncidentErrors.AlreadyClosed);
        if(Status!=TrainingIncidentStatus.Resolved)return Result.Failure(TrainingIncidentErrors.ResolutionRequired);
        var from=Status; Status=TrainingIncidentStatus.Closed; ClosedAtUtc=now.ToUniversalTime(); ClosedByUserId=actor;
        AddHistory(operationId,fp,TrainingIncidentHistoryAction.Closed,from,Status,note,actor,now); Touch(actor,now);
        RaiseDomainEvent(new TrainingIncidentClosedDomainEvent(Id,OrganizationId,TrainingSessionId,actor)); return Result.Success();
    }

    private void AddParticipantInternal(TrainingIncidentParticipantType type, Guid? referenceId, string? label)
    {
        if(_participants.Any(x=>x.Type==type&&x.ReferenceId==referenceId&&x.Label==label?.Trim()))return;
        _participants.Add(new TrainingIncidentParticipant(TrainingIncidentParticipantId.New(),Id,type,referenceId,label));
    }
    private Result? CheckMutation(Guid operationId,string fingerprint)
    {
        if(operationId==Guid.Empty)return Result.Failure(TrainingIncidentErrors.Invalid);
        var existing=_history.FirstOrDefault(x=>x.OperationId==operationId);
        return existing is null ? null : existing.RequestFingerprint==fingerprint ? Result.Success() : Result.Failure(TrainingIncidentErrors.OperationConflict);
    }
    private void AddHistory(Guid operationId,string fp,TrainingIncidentHistoryAction action,TrainingIncidentStatus from,TrainingIncidentStatus to,string? reason,UserId actor,DateTimeOffset now) =>
        _history.Add(new TrainingIncidentHistoryEntry(TrainingIncidentHistoryEntryId.New(),Id,operationId,fp,action,from,to,reason,actor,now));
    private void Touch(UserId actor,DateTimeOffset now){LastModifiedAtUtc=now.ToUniversalTime();LastModifiedByUserId=actor;}
    private static string? NormalizeRequired(string? value,int max){var x=value?.Trim();return string.IsNullOrWhiteSpace(x)||x.Length>max?null:x;}
    private static string Fingerprint(params object?[] values){var s=string.Join("|",values.Select(v=>v?.ToString()?.Trim()??""));return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(s)));}
    public void SetCreatedAudit(DateTimeOffset createdAtUtc, UserId? createdByUserId){CreatedAtUtc=createdAtUtc.ToUniversalTime();CreatedByUserId=createdByUserId;}
    public void SetModifiedAudit(DateTimeOffset modifiedAtUtc, UserId? modifiedByUserId){LastModifiedAtUtc=modifiedAtUtc.ToUniversalTime();LastModifiedByUserId=modifiedByUserId;}
}
