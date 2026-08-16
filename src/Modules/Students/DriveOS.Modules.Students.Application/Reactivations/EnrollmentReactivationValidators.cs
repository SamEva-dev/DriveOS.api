using DriveOS.Modules.Students.Domain.Suspensions;
using FluentValidation;

namespace DriveOS.Modules.Students.Application.Reactivations;

public sealed class CreateEnrollmentReactivationCommandValidator
    : AbstractValidator<CreateEnrollmentReactivationCommand>
{
    public CreateEnrollmentReactivationCommandValidator()
    {
        RuleFor(x => x.SuspensionId).NotEmpty();
        RuleFor(x => x.ResumeDate).NotEmpty();
        RuleFor(x => x.Conditions)
            .NotEmpty()
            .When(x => x.Mode == EnrollmentReactivationMode.Conditional);
        RuleFor(x => x.Checks)
            .NotNull()
            .Must(x =>
                x is not null
                && x.Select(c => c.Type).Distinct().Count()
                    == Enum.GetValues<ReactivationCheckType>().Length
            )
            .WithMessage("errors.students.reactivation.checksIncomplete");
    }
}

public sealed class ReviewEnrollmentReactivationCheckCommandValidator
    : AbstractValidator<ReviewEnrollmentReactivationCheckCommand>
{
    public ReviewEnrollmentReactivationCheckCommandValidator()
    {
        RuleFor(x => x.ReactivationId).NotEmpty();
        RuleFor(x => x.Detail).MaximumLength(1000);
    }
}
