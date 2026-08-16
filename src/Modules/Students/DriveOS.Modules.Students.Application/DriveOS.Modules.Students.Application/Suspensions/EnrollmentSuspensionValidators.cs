using DriveOS.Modules.Students.Domain.Suspensions;
using FluentValidation;

namespace DriveOS.Modules.Students.Application.Suspensions;

public sealed class SuspendEnrollmentCommandValidator : AbstractValidator<SuspendEnrollmentCommand>
{
    public SuspendEnrollmentCommandValidator()
    {
        RuleFor(x => x.Scope).NotEqual(EnrollmentSuspensionScope.None);
        RuleFor(x => x.StartDate).NotEmpty();
        RuleFor(x => x.ExpectedEndDate).GreaterThan(x => x.StartDate);
        RuleFor(x => x.ReviewDate)
            .GreaterThanOrEqualTo(x => x.StartDate)
            .LessThanOrEqualTo(x => x.ExpectedEndDate);
        RuleFor(x => x.ImmediateActions).NotEmpty().MaximumLength(2000);
        RuleFor(x => x.FutureBookingsCount).GreaterThanOrEqualTo(0);
        RuleFor(x => x.CreditDecision).NotEmpty().MaximumLength(1000);
        RuleFor(x => x.NotificationPlan).NotEmpty().MaximumLength(1000);
    }
}
