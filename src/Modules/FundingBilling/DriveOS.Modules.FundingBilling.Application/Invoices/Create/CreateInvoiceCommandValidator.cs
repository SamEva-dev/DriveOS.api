using FluentValidation;

namespace DriveOS.Modules.FundingBilling.Application.Invoices.Create;

internal sealed class CreateInvoiceCommandValidator : AbstractValidator<CreateInvoiceCommand>
{
    public CreateInvoiceCommandValidator()
    {
        RuleFor(x => x.OrganizationId).Must(x => !x.IsEmpty);
        RuleFor(x => x.BillingAccountId).Must(x => !x.IsEmpty);
        RuleFor(x => x.ActorUserId).Must(x => !x.IsEmpty);
        RuleFor(x => x.Lines).NotNull().Must(x => x.Count > 0).WithMessage("At least one invoice line is required.");
        RuleForEach(x => x.Lines).ChildRules(line =>
        {
            line.RuleFor(x => x.Description).NotEmpty().MaximumLength(500);
            line.RuleFor(x => x.Unit).NotEmpty().MaximumLength(40);
            line.RuleFor(x => x.Quantity).GreaterThan(0m);
            line.RuleFor(x => x.UnitPrice).GreaterThanOrEqualTo(0m);
            line.RuleFor(x => x.DiscountAmount).GreaterThanOrEqualTo(0m);
            line.RuleFor(x => x.TaxRate).InclusiveBetween(0m, 100m);
        });
    }
}
