using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.TrainingDelivery.Domain.GroupSessions;

public enum GroupSessionAttendanceStatus { Pending = 0, Present = 1, Absent = 2, Late = 3, Excused = 4 }
public enum GroupSessionAttendanceMethod { Manual = 1, QrCode = 2, Signature = 3, Imported = 4 }
public enum GroupSessionCertificateStatus { NotRequested = 0, Ready = 1, Issued = 2 }

/// <summary>
/// Aggregate root for a collective Training Delivery session. It deliberately remains separate from the
/// individual TrainingSession aggregate: shared delivery data belongs here while attendance and assessments
/// remain participant-specific children.
/// </summary>
public sealed class GroupTrainingSession : AggregateRoot<GroupTrainingSessionId>
{
    private readonly List<GroupTrainingSessionParticipant> _participants = [];
    private readonly List<GroupTrainingSessionOperation> _operations = [];
    private GroupTrainingSession() { }

    private GroupTrainingSession(GroupTrainingSessionId id, GroupTrainingSessionMaterialization m) : base(id)
    {
        OrganizationId = m.OrganizationId;
        SourceBookingId = m.SourceBookingId;
        Program = m.Program.Trim();
        Capacity = m.Capacity;
        TrainerId = m.TrainerId;
        BranchId = m.BranchId;
        RoomResourceId = m.RoomResourceId;
        RoomName = m.RoomName;
        PlannedStartAtUtc = m.PlannedStartAtUtc.ToUniversalTime();
        PlannedEndAtUtc = m.PlannedEndAtUtc.ToUniversalTime();
        SharedObjectives = m.SharedObjectives?.Trim();
        foreach (PersonId studentId in m.ParticipantStudentIds.Distinct())
            _participants.Add(GroupTrainingSessionParticipant.Create(Id, studentId, false));
    }

    public OrganizationId OrganizationId { get; private set; }
    public BookingId SourceBookingId { get; private set; }
    public string Program { get; private set; } = string.Empty;
    public int Capacity { get; private set; }
    public UserId TrainerId { get; private set; }
    public BranchId? BranchId { get; private set; }
    public Guid? RoomResourceId { get; private set; }
    public string? RoomName { get; private set; }
    public DateTimeOffset PlannedStartAtUtc { get; private set; }
    public DateTimeOffset PlannedEndAtUtc { get; private set; }
    public string? SharedObjectives { get; private set; }
    public string? CollectiveReport { get; private set; }
    public IReadOnlyCollection<GroupTrainingSessionParticipant> Participants => _participants.AsReadOnly();
    public IReadOnlyCollection<GroupTrainingSessionOperation> Operations => _operations.AsReadOnly();

    public static Result<GroupTrainingSession> Materialize(GroupTrainingSessionMaterialization m)
    {
        if (m.OrganizationId.Value == Guid.Empty || m.SourceBookingId.Value == Guid.Empty || m.TrainerId.Value == Guid.Empty ||
            string.IsNullOrWhiteSpace(m.Program) || m.Capacity <= 0 || m.PlannedEndAtUtc <= m.PlannedStartAtUtc ||
            m.ParticipantStudentIds.Count == 0)
            return Result.Failure<GroupTrainingSession>(GroupTrainingSessionErrors.SourceBookingIncomplete);
        if (m.ParticipantStudentIds.Distinct().Count() > m.Capacity)
            return Result.Failure<GroupTrainingSession>(GroupTrainingSessionErrors.CapacityExceeded);
        return Result.Success(new GroupTrainingSession(GroupTrainingSessionId.New(), m));
    }

    public Result<GroupTrainingSessionParticipant> AddAuthorizedParticipant(PersonId studentId, Guid operationId)
    {
        if (Replay(operationId))
        {
            GroupTrainingSessionParticipant? replayed = _participants.FirstOrDefault(x => x.StudentId == studentId);
            return replayed is not null ? Result.Success(replayed) : Result.Failure<GroupTrainingSessionParticipant>(GroupTrainingSessionErrors.OperationConflict);
        }
        if (_participants.Any(x => x.StudentId == studentId)) return Result.Failure<GroupTrainingSessionParticipant>(GroupTrainingSessionErrors.ParticipantAlreadyExists);
        if (_participants.Count >= Capacity) return Result.Failure<GroupTrainingSessionParticipant>(GroupTrainingSessionErrors.CapacityExceeded);
        GroupTrainingSessionParticipant participant = GroupTrainingSessionParticipant.Create(Id, studentId, true);
        _participants.Add(participant); _operations.Add(GroupTrainingSessionOperation.Create(Id, operationId)); return Result.Success(participant);
    }

    public Result RecordAttendance(PersonId studentId, GroupSessionAttendanceStatus status, GroupSessionAttendanceMethod method, DateTimeOffset? checkInAtUtc, DateTimeOffset? checkOutAtUtc, Guid actorUserId, Guid operationId)
    {
        GroupTrainingSessionParticipant? p = _participants.SingleOrDefault(x => x.StudentId == studentId);
        if (p is null) return Result.Failure(GroupTrainingSessionErrors.ParticipantNotFound);
        if (Replay(operationId)) return p.AttendanceStatus == status && p.AttendanceMethod == method ? Result.Success() : Result.Failure(GroupTrainingSessionErrors.OperationConflict);
        Result r = p.RecordAttendance(status, method, checkInAtUtc, checkOutAtUtc, actorUserId);
        if (r.IsSuccess) _operations.Add(GroupTrainingSessionOperation.Create(Id, operationId)); return r;
    }

    public Result RecordIndividualAssessment(PersonId studentId, Guid? competencyId, int? level, decimal? quizScore, string? observation, Guid actorUserId, Guid operationId)
    {
        GroupTrainingSessionParticipant? p = _participants.SingleOrDefault(x => x.StudentId == studentId);
        if (p is null) return Result.Failure(GroupTrainingSessionErrors.ParticipantNotFound);
        if (Replay(operationId))
            return p.CompetencyId == competencyId && p.AssessmentLevel == level && p.QuizScore == quizScore && string.Equals(p.IndividualObservation, observation?.Trim(), StringComparison.Ordinal)
                ? Result.Success() : Result.Failure(GroupTrainingSessionErrors.OperationConflict);
        Result r = p.RecordAssessment(competencyId, level, quizScore, observation, actorUserId);
        if (r.IsSuccess) _operations.Add(GroupTrainingSessionOperation.Create(Id, operationId)); return r;
    }

    public Result SaveCollectiveReport(string report, string? sharedObjectives, Guid operationId)
    {
        if (Replay(operationId)) return string.Equals(CollectiveReport, report.Trim(), StringComparison.Ordinal) && string.Equals(SharedObjectives, sharedObjectives?.Trim(), StringComparison.Ordinal) ? Result.Success() : Result.Failure(GroupTrainingSessionErrors.OperationConflict);
        if (string.IsNullOrWhiteSpace(report) || report.Length > 4000 || (sharedObjectives?.Length ?? 0) > 2000)
            return Result.Failure(GroupTrainingSessionErrors.InvalidReport);
        CollectiveReport = report.Trim(); SharedObjectives = sharedObjectives?.Trim(); _operations.Add(GroupTrainingSessionOperation.Create(Id, operationId)); return Result.Success();
    }

    public Result MarkCertificateReady(PersonId studentId, Guid operationId)
    {
        GroupTrainingSessionParticipant? p = _participants.SingleOrDefault(x => x.StudentId == studentId);
        if (p is null) return Result.Failure(GroupTrainingSessionErrors.ParticipantNotFound);
        if (Replay(operationId)) return p.CertificateStatus == GroupSessionCertificateStatus.Ready ? Result.Success() : Result.Failure(GroupTrainingSessionErrors.OperationConflict);
        p.MarkCertificateReady(); _operations.Add(GroupTrainingSessionOperation.Create(Id, operationId)); return Result.Success();
    }

    private bool Replay(Guid operationId) => operationId != Guid.Empty && _operations.Any(x => x.OperationId == operationId);
}

public sealed class GroupTrainingSessionParticipant
{
    private GroupTrainingSessionParticipant() { }
    private GroupTrainingSessionParticipant(GroupTrainingSessionParticipantId id, GroupTrainingSessionId groupSessionId, PersonId studentId, bool addedOutsideOriginalList)
    { Id=id; GroupTrainingSessionId=groupSessionId; StudentId=studentId; AddedOutsideOriginalList=addedOutsideOriginalList; }
    public GroupTrainingSessionParticipantId Id { get; private set; }
    public GroupTrainingSessionId GroupTrainingSessionId { get; private set; }
    public PersonId StudentId { get; private set; }
    public bool AddedOutsideOriginalList { get; private set; }
    public GroupSessionAttendanceStatus AttendanceStatus { get; private set; }
    public GroupSessionAttendanceMethod? AttendanceMethod { get; private set; }
    public DateTimeOffset? CheckInAtUtc { get; private set; }
    public DateTimeOffset? CheckOutAtUtc { get; private set; }
    public Guid? AttendanceRecordedBy { get; private set; }
    public Guid? CompetencyId { get; private set; }
    public int? AssessmentLevel { get; private set; }
    public decimal? QuizScore { get; private set; }
    public string? IndividualObservation { get; private set; }
    public Guid? AssessmentRecordedBy { get; private set; }
    public GroupSessionCertificateStatus CertificateStatus { get; private set; }
    internal static GroupTrainingSessionParticipant Create(GroupTrainingSessionId sessionId, PersonId studentId, bool outside) => new(GroupTrainingSessionParticipantId.New(), sessionId, studentId, outside);
    internal Result RecordAttendance(GroupSessionAttendanceStatus status, GroupSessionAttendanceMethod method, DateTimeOffset? inAt, DateTimeOffset? outAt, Guid actor)
    {
        if (!Enum.IsDefined(status) || !Enum.IsDefined(method) || actor == Guid.Empty || (inAt.HasValue && outAt.HasValue && outAt < inAt)) return Result.Failure(GroupTrainingSessionErrors.InvalidAttendance);
        AttendanceStatus=status; AttendanceMethod=method; CheckInAtUtc=inAt?.ToUniversalTime(); CheckOutAtUtc=outAt?.ToUniversalTime(); AttendanceRecordedBy=actor; return Result.Success();
    }
    internal Result RecordAssessment(Guid? competencyId, int? level, decimal? quizScore, string? observation, Guid actor)
    {
        if (actor == Guid.Empty || (level.HasValue && (level < 0 || level > 10)) || (quizScore.HasValue && (quizScore < 0 || quizScore > 100)) || (observation?.Length ?? 0) > 1000 || (!level.HasValue && !quizScore.HasValue && string.IsNullOrWhiteSpace(observation))) return Result.Failure(GroupTrainingSessionErrors.InvalidAssessment);
        CompetencyId=competencyId; AssessmentLevel=level; QuizScore=quizScore; IndividualObservation=observation?.Trim(); AssessmentRecordedBy=actor; return Result.Success();
    }
    internal void MarkCertificateReady() => CertificateStatus = GroupSessionCertificateStatus.Ready;
}

public sealed class GroupTrainingSessionOperation
{
    private GroupTrainingSessionOperation() { }
    private GroupTrainingSessionOperation(GroupTrainingSessionOperationId id, GroupTrainingSessionId groupSessionId, Guid operationId) { Id=id; GroupTrainingSessionId=groupSessionId; OperationId=operationId; }
    public GroupTrainingSessionOperationId Id { get; private set; }
    public GroupTrainingSessionId GroupTrainingSessionId { get; private set; }
    public Guid OperationId { get; private set; }
    internal static GroupTrainingSessionOperation Create(GroupTrainingSessionId groupSessionId, Guid operationId) => new(GroupTrainingSessionOperationId.New(), groupSessionId, operationId);
}

public sealed record GroupTrainingSessionMaterialization(OrganizationId OrganizationId, BookingId SourceBookingId, string Program, int Capacity, UserId TrainerId, BranchId? BranchId, Guid? RoomResourceId, string? RoomName, DateTimeOffset PlannedStartAtUtc, DateTimeOffset PlannedEndAtUtc, string? SharedObjectives, IReadOnlyCollection<PersonId> ParticipantStudentIds);
