using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Modules.Students.Domain.Instructors;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Students.Application.Instructors;

public sealed record StudentInstructorsResponse(
    Guid StudentId,
    Guid? PrimaryInstructorId,
    IReadOnlyList<StudentInstructorAssignmentItem> Assignments,
    IReadOnlyList<StudentInstructorHistoryItem> History
);

public sealed record StudentInstructorAssignmentItem(
    Guid Id,
    Guid InstructorId,
    StudentInstructorAssignmentType Type,
    DateOnly EffectiveFrom,
    DateOnly? EffectiveTo,
    string TrainingCategory,
    StudentInstructorScope MaximumScope,
    string Reason,
    StudentInstructorAssignmentStatus Status
);

public sealed record StudentInstructorHistoryItem(
    Guid Id,
    Guid AssignmentId,
    string Action,
    string Reason,
    Guid ActorUserId,
    DateTimeOffset OccurredAtUtc
);

public sealed record InstructorSuggestionItem(
    Guid InstructorId,
    Guid BranchId,
    string? DisplayName,
    string TrainingCategory,
    bool QualificationVerified,
    InstructorMetricStatus AvailabilityStatus,
    decimal? LoadPercentage,
    DateTimeOffset? NextAvailabilityUtc,
    double? AverageDistanceKm,
    bool HasInitialAssessment,
    bool IsPartner,
    IReadOnlyList<string> Warnings
);

public sealed record InstructorEligibility(bool IsEligible, IReadOnlyList<string> Warnings);

/// <summary>
/// Cross-bounded-context Workforce decision used by Students without taking a dependency on Workforce.
/// The API composition root supplies the authoritative implementation.
/// </summary>
public sealed record InstructorWorkforceEligibility(bool IsEligible, string? ReasonCode);

public interface IInstructorWorkforceEligibilityGateway
{
    Task<InstructorWorkforceEligibility> VerifyAsync(
        OrganizationId organizationId,
        UserId instructorId,
        BranchId? branchId,
        string trainingCategory,
        DateOnly effectiveDate,
        CancellationToken ct = default);
}

public sealed record GetStudentInstructorsQuery(OrganizationId OrganizationId, PersonId StudentId)
    : IQuery<StudentInstructorsResponse>;

public sealed record GetInstructorSuggestionsQuery(
    OrganizationId OrganizationId,
    PersonId StudentId,
    BranchId? BranchId,
    string TrainingCategory
) : IQuery<IReadOnlyList<InstructorSuggestionItem>>;

public sealed record AssignStudentInstructorCommand(
    OrganizationId OrganizationId,
    PersonId StudentId,
    UserId InstructorId,
    StudentInstructorAssignmentType Type,
    DateOnly EffectiveFrom,
    DateOnly? EffectiveTo,
    string TrainingCategory,
    StudentInstructorScope MaximumScope,
    string Reason,
    UserId ActorUserId
) : ICommand<Guid>;

public sealed record ReplacePrimaryInstructorCommand(
    OrganizationId OrganizationId,
    PersonId StudentId,
    UserId InstructorId,
    DateOnly EffectiveFrom,
    DateOnly? EffectiveTo,
    string TrainingCategory,
    StudentInstructorScope MaximumScope,
    string Reason,
    UserId ActorUserId
) : ICommand;

public sealed record EndStudentInstructorAssignmentCommand(
    OrganizationId OrganizationId,
    PersonId StudentId,
    Guid AssignmentId,
    string Reason,
    UserId ActorUserId
) : ICommand;

public interface IStudentInstructorService
{
    Task<StudentInstructorsResponse?> GetAsync(
        GetStudentInstructorsQuery query,
        CancellationToken ct = default
    );
    Task<IReadOnlyList<InstructorSuggestionItem>> GetSuggestionsAsync(
        GetInstructorSuggestionsQuery query,
        CancellationToken ct = default
    );
    Task<Result<Guid>> AssignAsync(
        AssignStudentInstructorCommand command,
        CancellationToken ct = default
    );
    Task<Result> ReplacePrimaryAsync(
        ReplacePrimaryInstructorCommand command,
        CancellationToken ct = default
    );
    Task<Result> EndAsync(
        EndStudentInstructorAssignmentCommand command,
        CancellationToken ct = default
    );
}

public interface IInstructorEligibilityGateway
{
    Task<InstructorEligibility> VerifyAsync(
        OrganizationId organizationId,
        UserId instructorId,
        BranchId? branchId,
        string trainingCategory,
        CancellationToken ct = default
    );
    Task<IReadOnlyList<InstructorSuggestionItem>> SuggestAsync(
        OrganizationId organizationId,
        BranchId? branchId,
        string trainingCategory,
        CancellationToken ct = default
    );
}

public static class StudentInstructorApplicationErrors
{
    public static readonly Error StudentNotFound = Error.NotFound(
        "Students.Instructors.Student.NotFound",
        "errors.students.instructors.student.notFound"
    );
}
