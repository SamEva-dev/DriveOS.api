using FluentValidation;

namespace DriveOS.Modules.FundingBilling.Application.Invoices.Issue;

internal sealed class IssueInvoiceCommandValidator : AbstractValidator<IssueInvoiceCommand>
{
    public IssueInvoiceCommandValidator()
    {
        RuleFor(x => x.OrganizationId).Must(x => !x.IsEmpty);
        RuleFor(x => x.InvoiceId).Must(x => !x.IsEmpty);
        RuleFor(x => x.ActorUserId).Must(x => !x.IsEmpty);
        RuleFor(x => x.IssueDate).NotEqual(default(DateOnly));
        RuleFor(x => x.DueDate).NotEqual(default(DateOnly)).GreaterThanOrEqualTo(x => x.IssueDate);
    }
}
