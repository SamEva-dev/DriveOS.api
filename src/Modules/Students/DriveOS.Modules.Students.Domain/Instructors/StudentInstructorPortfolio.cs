using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;
using DriveOS.Modules.Students.Domain.Events;

namespace DriveOS.Modules.Students.Domain.Instructors;

public sealed class StudentInstructorPortfolio : AggregateRoot<StudentInstructorPortfolioId>
{
    private readonly List<StudentInstructorAssignment> assignments = [];
    private readonly List<StudentInstructorAccessGrant> accessGrants = [];
    private readonly List<StudentInstructorHistory> history = [];

    private StudentInstructorPortfolio() { }

    private StudentInstructorPortfolio(StudentInstructorPortfolioId id, OrganizationId org, PersonId student)
        : base(id)
    {
        OrganizationId = org;
        StudentId = student;
        RaiseDomainEvent(new StudentAggregateChangedDomainEvent<StudentInstructorPortfolioId>(Id, StudentId, OrganizationId, "Created"));
    }

    public OrganizationId OrganizationId { get; private set; }
    public PersonId StudentId { get; private set; }
    public IReadOnlyCollection<StudentInstructorAssignment> Assignments => assignments;
    public IReadOnlyCollection<StudentInstructorAccessGrant> AccessGrants => accessGrants;
    public IReadOnlyCollection<StudentInstructorHistory> History => history;

    public static Result<StudentInstructorPortfolio> Create(OrganizationId org, PersonId student) =>
        org.IsEmpty || student.IsEmpty
            ? Result.Failure<StudentInstructorPortfolio>(StudentInstructorErrors.InvalidOwner)
            : Result.Success(new StudentInstructorPortfolio(StudentInstructorPortfolioId.New(), org, student));

    public Result<Guid> Assign(
        UserId instructor,
        StudentInstructorAssignmentType type,
        DateOnly from,
        DateOnly? to,
        string trainingCategory,
        StudentInstructorScope scope,
        string reason,
        UserId actor,
        DateTimeOffset now
    )
    {
        if (
            instructor.IsEmpty
            || to < from
            || string.IsNullOrWhiteSpace(trainingCategory)
            || scope == StudentInstructorScope.None
        )
            return Result.Failure<Guid>(StudentInstructorErrors.InvalidAssignment);
        if (string.IsNullOrWhiteSpace(reason))
            return Result.Failure<Guid>(StudentInstructorErrors.ReasonRequired);
        if (
            type == StudentInstructorAssignmentType.PrimaryInstructor
            && assignments.Any(x => x.Type == type && x.IsEffectiveOrPlanned(now))
        )
            return Result.Failure<Guid>(StudentInstructorErrors.PrimaryAlreadyExists);
        var assignment = StudentInstructorAssignment.Create(
            Id,
            instructor,
            type,
            from,
            to,
            trainingCategory.Trim(),
            scope,
            reason.Trim(),
            actor,
            now
        );
        assignments.Add(assignment);
        accessGrants.Add(
            StudentInstructorAccessGrant.Create(Id, assignment.Id, instructor, scope, from, to, now)
        );
        history.Add(
            StudentInstructorHistory.Create(Id, assignment.Id, "Assigned", reason, actor, now)
        );
        return Result.Success(assignment.Id);
    }

    public Result ReplacePrimary(
        UserId instructor,
        DateOnly from,
        DateOnly? to,
        string trainingCategory,
        StudentInstructorScope scope,
        string reason,
        UserId actor,
        DateTimeOffset now
    )
    {
        if (string.IsNullOrWhiteSpace(reason))
            return Result.Failure(StudentInstructorErrors.ReasonRequired);
        foreach (
            var current in assignments.Where(x =>
                x.Type == StudentInstructorAssignmentType.PrimaryInstructor
                && x.IsEffectiveOrPlanned(now)
            )
        )
        {
            current.End(
                StudentInstructorAssignmentStatus.Replaced,
                DateOnly.FromDateTime(now.UtcDateTime),
                actor,
                now
            );
            accessGrants.Single(x => x.AssignmentId == current.Id).Revoke(actor, now);
            history.Add(
                StudentInstructorHistory.Create(Id, current.Id, "Replaced", reason, actor, now)
            );
        }
        var result = Assign(
            instructor,
            StudentInstructorAssignmentType.PrimaryInstructor,
            from,
            to,
            trainingCategory,
            scope,
            reason,
            actor,
            now
        );
        return result.IsSuccess ? Result.Success() : Result.Failure(result.Error);
    }

    public Result End(Guid id, string reason, UserId actor, DateTimeOffset now)
    {
        var item = assignments.SingleOrDefault(x => x.Id == id);
        if (item is null)
            return Result.Failure(StudentInstructorErrors.AssignmentNotFound);
        if (string.IsNullOrWhiteSpace(reason))
            return Result.Failure(StudentInstructorErrors.ReasonRequired);
        item.End(
            StudentInstructorAssignmentStatus.Ended,
            DateOnly.FromDateTime(now.UtcDateTime),
            actor,
            now
        );
        accessGrants.Single(x => x.AssignmentId == id).Revoke(actor, now);
        history.Add(StudentInstructorHistory.Create(Id, id, "Ended", reason, actor, now));
        return Result.Success();
    }
}

public sealed class StudentInstructorAssignment
{
    private StudentInstructorAssignment() { }

    public Guid Id { get; private set; }
    public StudentInstructorPortfolioId StudentInstructorPortfolioId { get; private set; }
    public UserId InstructorId { get; private set; }
    public StudentInstructorAssignmentType Type { get; private set; }
    public DateOnly EffectiveFrom { get; private set; }
    public DateOnly? EffectiveTo { get; private set; }
    public string TrainingCategory { get; private set; } = string.Empty;
    public StudentInstructorScope MaximumScope { get; private set; }
    public string Reason { get; private set; } = string.Empty;
    public StudentInstructorAssignmentStatus Status { get; private set; }
    public UserId CreatedByUserId { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public UserId? EndedByUserId { get; private set; }
    public DateTimeOffset? EndedAtUtc { get; private set; }

    internal static StudentInstructorAssignment Create(
        Guid portfolio,
        UserId instructor,
        StudentInstructorAssignmentType type,
        DateOnly from,
        DateOnly? to,
        string category,
        StudentInstructorScope scope,
        string reason,
        UserId actor,
        DateTimeOffset now
    ) =>
        new()
        {
            Id = Guid.NewGuid(),
            StudentInstructorPortfolioId = new StudentInstructorPortfolioId(portfolio),
            InstructorId = instructor,
            Type = type,
            EffectiveFrom = from,
            EffectiveTo = to,
            TrainingCategory = category,
            MaximumScope = scope,
            Reason = reason,
            Status =
                from > DateOnly.FromDateTime(now.UtcDateTime)
                    ? StudentInstructorAssignmentStatus.Planned
                    : StudentInstructorAssignmentStatus.Active,
            CreatedByUserId = actor,
            CreatedAtUtc = now,
        };

    internal bool IsEffectiveOrPlanned(DateTimeOffset now) =>
        (
            Status
            is StudentInstructorAssignmentStatus.Active
                or StudentInstructorAssignmentStatus.Planned
        ) && (!EffectiveTo.HasValue || EffectiveTo.Value >= DateOnly.FromDateTime(now.UtcDateTime));

    internal void End(
        StudentInstructorAssignmentStatus status,
        DateOnly date,
        UserId actor,
        DateTimeOffset now
    )
    {
        EffectiveTo = date;
        Status = status;
        EndedByUserId = actor;
        EndedAtUtc = now;
    }
}

public sealed class StudentInstructorAccessGrant
{
    private StudentInstructorAccessGrant() { }

    public Guid Id { get; private set; }
    public StudentInstructorPortfolioId StudentInstructorPortfolioId { get; private set; }
    public Guid AssignmentId { get; private set; }
    public UserId InstructorId { get; private set; }
    public StudentInstructorScope Scope { get; private set; }
    public DateOnly EffectiveFrom { get; private set; }
    public DateOnly? EffectiveTo { get; private set; }
    public DateTimeOffset GrantedAtUtc { get; private set; }
    public DateTimeOffset? RevokedAtUtc { get; private set; }
    public UserId? RevokedByUserId { get; private set; }

    internal static StudentInstructorAccessGrant Create(
        Guid portfolio,
        Guid assignment,
        UserId instructor,
        StudentInstructorScope scope,
        DateOnly from,
        DateOnly? to,
        DateTimeOffset now
    ) =>
        new()
        {
            Id = Guid.NewGuid(),
            StudentInstructorPortfolioId = new StudentInstructorPortfolioId(portfolio),
            AssignmentId = assignment,
            InstructorId = instructor,
            Scope = scope,
            EffectiveFrom = from,
            EffectiveTo = to,
            GrantedAtUtc = now,
        };

    internal void Revoke(UserId actor, DateTimeOffset now)
    {
        RevokedAtUtc = now;
        RevokedByUserId = actor;
        EffectiveTo = DateOnly.FromDateTime(now.UtcDateTime);
    }
}

public sealed class StudentInstructorHistory
{
    private StudentInstructorHistory() { }

    public Guid Id { get; private set; }
    public StudentInstructorPortfolioId StudentInstructorPortfolioId { get; private set; }
    public Guid AssignmentId { get; private set; }
    public string Action { get; private set; } = string.Empty;
    public string Reason { get; private set; } = string.Empty;
    public UserId ActorUserId { get; private set; }
    public DateTimeOffset OccurredAtUtc { get; private set; }

    internal static StudentInstructorHistory Create(
        Guid portfolio,
        Guid assignment,
        string action,
        string reason,
        UserId actor,
        DateTimeOffset now
    ) =>
        new()
        {
            Id = Guid.NewGuid(),
            StudentInstructorPortfolioId = new StudentInstructorPortfolioId( portfolio),
            AssignmentId = assignment,
            Action = action,
            Reason = reason.Trim(),
            ActorUserId = actor,
            OccurredAtUtc = now,
        };
}
