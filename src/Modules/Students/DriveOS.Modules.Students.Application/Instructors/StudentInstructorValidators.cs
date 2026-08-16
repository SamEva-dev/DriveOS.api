using DriveOS.Modules.Students.Domain.Instructors;
using FluentValidation;

namespace DriveOS.Modules.Students.Application.Instructors;

public sealed class GetInstructorSuggestionsQueryValidator
    : AbstractValidator<GetInstructorSuggestionsQuery>
{
    public GetInstructorSuggestionsQueryValidator() =>
        RuleFor(x => x.TrainingCategory).NotEmpty().MaximumLength(50);
}

public sealed class AssignStudentInstructorCommandValidator
    : AbstractValidator<AssignStudentInstructorCommand>
{
    public AssignStudentInstructorCommandValidator()
    {
        RuleFor(x => x.InstructorId).NotEmpty();
        RuleFor(x => x.TrainingCategory).NotEmpty().MaximumLength(50);
        RuleFor(x => x.MaximumScope).NotEqual(StudentInstructorScope.None);
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(500);
        RuleFor(x => x.EffectiveTo)
            .GreaterThanOrEqualTo(x => x.EffectiveFrom)
            .When(x => x.EffectiveTo.HasValue);
    }
}

public sealed class ReplacePrimaryInstructorCommandValidator
    : AbstractValidator<ReplacePrimaryInstructorCommand>
{
    public ReplacePrimaryInstructorCommandValidator()
    {
        RuleFor(x => x.InstructorId).NotEmpty();
        RuleFor(x => x.TrainingCategory).NotEmpty().MaximumLength(50);
        RuleFor(x => x.MaximumScope).NotEqual(StudentInstructorScope.None);
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(500);
        RuleFor(x => x.EffectiveTo)
            .GreaterThanOrEqualTo(x => x.EffectiveFrom)
            .When(x => x.EffectiveTo.HasValue);
    }
}

public sealed class EndStudentInstructorAssignmentCommandValidator
    : AbstractValidator<EndStudentInstructorAssignmentCommand>
{
    public EndStudentInstructorAssignmentCommandValidator()
    {
        RuleFor(x => x.AssignmentId).NotEmpty();
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(500);
    }
}
