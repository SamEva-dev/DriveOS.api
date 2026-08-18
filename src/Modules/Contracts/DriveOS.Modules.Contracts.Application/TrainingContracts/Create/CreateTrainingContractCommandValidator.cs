using FluentValidation;
namespace DriveOS.Modules.Contracts.Application.TrainingContracts.Create;
public sealed class CreateTrainingContractCommandValidator : AbstractValidator<CreateTrainingContractCommand>
{
    public CreateTrainingContractCommandValidator()
    {
        RuleFor(x => x.OrganizationId).Must(x => !x.IsEmpty);
        RuleFor(x => x.EnrollmentId).Must(x => !x.IsEmpty);
        RuleFor(x => x.SourceOfferId).Must(x => !x.IsEmpty);
        RuleFor(x => x.ContractNumber).NotEmpty().MaximumLength(80);
        RuleFor(x => x.EndDate).GreaterThanOrEqualTo(x => x.StartDate).When(x => x.EndDate.HasValue);
        RuleFor(x => x.PracticalHours).InclusiveBetween(0, 1000);
        RuleFor(x => x.ServicesSnapshot).NotEmpty().MaximumLength(20000);
        RuleFor(x => x.PaymentScheduleSnapshot).NotEmpty().MaximumLength(20000);
        RuleFor(x => x.CancellationTerms).NotEmpty().MaximumLength(20000);
        RuleFor(x => x.BookingRules).NotEmpty().MaximumLength(20000);
        RuleFor(x => x.StudentObligations).NotEmpty().MaximumLength(20000);
        RuleFor(x => x.ProviderObligations).NotEmpty().MaximumLength(20000);
        RuleFor(x => x.ExamPresentationTerms).NotEmpty().MaximumLength(20000);
        RuleFor(x => x.DataProcessingTerms).NotEmpty().MaximumLength(20000);
    }
}
