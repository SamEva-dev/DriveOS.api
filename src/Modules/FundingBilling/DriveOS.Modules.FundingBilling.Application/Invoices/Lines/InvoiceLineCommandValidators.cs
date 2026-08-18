using FluentValidation;

namespace DriveOS.Modules.FundingBilling.Application.Invoices.Lines;

internal sealed class AddInvoiceLineCommandValidator : AbstractValidator<AddInvoiceLineCommand>
{
    public AddInvoiceLineCommandValidator()
    {
        RuleFor(x => x.OrganizationId).Must(x => !x.IsEmpty);
        RuleFor(x => x.InvoiceId).Must(x => !x.IsEmpty);
        RuleFor(x => x.ActorUserId).Must(x => !x.IsEmpty);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(500);
        RuleFor(x => x.Unit).NotEmpty().MaximumLength(40);
        RuleFor(x => x.Quantity).GreaterThan(0m);
        RuleFor(x => x.UnitPrice).GreaterThanOrEqualTo(0m);
        RuleFor(x => x.DiscountAmount).GreaterThanOrEqualTo(0m);
        RuleFor(x => x.TaxRate).InclusiveBetween(0m, 100m);
    }
}

internal sealed class RemoveInvoiceLineCommandValidator : AbstractValidator<RemoveInvoiceLineCommand>
{
    public RemoveInvoiceLineCommandValidator()
    {
        RuleFor(x => x.OrganizationId).Must(x => !x.IsEmpty);
        RuleFor(x => x.InvoiceId).Must(x => !x.IsEmpty);
        RuleFor(x => x.InvoiceLineId).Must(x => !x.IsEmpty);
        RuleFor(x => x.ActorUserId).Must(x => !x.IsEmpty);
    }
}
