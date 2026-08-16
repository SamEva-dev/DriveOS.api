using DriveOS.Application.Abstractions.Messaging;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Students.Application.Instructors;

public sealed class GetStudentInstructorsQueryHandler(IStudentInstructorService service)
    : IQueryHandler<GetStudentInstructorsQuery, StudentInstructorsResponse>
{
    public async Task<Result<StudentInstructorsResponse>> Handle(
        GetStudentInstructorsQuery query,
        CancellationToken ct
    )
    {
        var result = await service.GetAsync(query, ct);
        return result is null
            ? Result.Failure<StudentInstructorsResponse>(
                StudentInstructorApplicationErrors.StudentNotFound
            )
            : Result.Success(result);
    }
}

public sealed class GetInstructorSuggestionsQueryHandler(IStudentInstructorService service)
    : IQueryHandler<GetInstructorSuggestionsQuery, IReadOnlyList<InstructorSuggestionItem>>
{
    public async Task<Result<IReadOnlyList<InstructorSuggestionItem>>> Handle(
        GetInstructorSuggestionsQuery query,
        CancellationToken ct
    ) => Result.Success(await service.GetSuggestionsAsync(query, ct));
}

public sealed class AssignStudentInstructorCommandHandler(IStudentInstructorService service)
    : ICommandHandler<AssignStudentInstructorCommand, Guid>
{
    public Task<Result<Guid>> Handle(
        AssignStudentInstructorCommand command,
        CancellationToken ct
    ) => service.AssignAsync(command, ct);
}

public sealed class ReplacePrimaryInstructorCommandHandler(IStudentInstructorService service)
    : ICommandHandler<ReplacePrimaryInstructorCommand>
{
    public Task<Result> Handle(ReplacePrimaryInstructorCommand command, CancellationToken ct) =>
        service.ReplacePrimaryAsync(command, ct);
}

public sealed class EndStudentInstructorAssignmentCommandHandler(IStudentInstructorService service)
    : ICommandHandler<EndStudentInstructorAssignmentCommand>
{
    public Task<Result> Handle(
        EndStudentInstructorAssignmentCommand command,
        CancellationToken ct
    ) => service.EndAsync(command, ct);
}
