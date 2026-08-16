using DriveOS.Modules.Students.Domain.Enrollments;
using FluentValidation;

namespace DriveOS.Modules.Students.Application.Enrollments.StartDirectEnrollment;

public sealed class StartDirectEnrollmentCommandValidator
    : AbstractValidator<StartDirectEnrollmentCommand>
{
    public StartDirectEnrollmentCommandValidator()
    {
        RuleFor(x => x.OrganizationId).Must(x => !x.IsEmpty);
        RuleFor(x => x.BranchId).Must(x => !x.IsEmpty);
        RuleFor(x => x.ExistingStudentId).Must(x => x is null || !x.Value.IsEmpty);
        RuleFor(x => x.IdempotencyKey).NotEmpty().MinimumLength(8).MaximumLength(100);
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Email)
            .EmailAddress()
            .MaximumLength(254)
            .When(x => !string.IsNullOrWhiteSpace(x.Email));
        RuleFor(x => x.Phone).MaximumLength(40);
        RuleFor(x => x)
            .Must(x => !string.IsNullOrWhiteSpace(x.Email) || !string.IsNullOrWhiteSpace(x.Phone))
            .WithErrorCode("Students.DirectEnrollment.Contact.Required")
            .WithMessage("errors.students.directEnrollment.contact.required");
        RuleFor(x => x.TrainingCode).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Source).IsInEnum().NotEqual(EnrollmentSource.CrmConversion);
        RuleFor(x => x.RegulatoryCountryCode).NotEmpty().MinimumLength(2).MaximumLength(3);
        RuleFor(x => x.PreferredLanguageCode).NotEmpty().MinimumLength(2).MaximumLength(10);
        RuleFor(x => x.RequiredConsentsAccepted)
            .Equal(true)
            .WithErrorCode("Students.Enrollment.Consents.Required")
            .WithMessage("errors.students.enrollment.consents.required");
    }
}
