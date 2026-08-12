using FluentValidation;

namespace DriveOS.Modules.CRM.Application.Leads.ConvertLead;

public sealed class ConvertLeadCommandValidator : AbstractValidator<ConvertLeadCommand>
{
    public ConvertLeadCommandValidator()
    {
        RuleFor(x => x.OrganizationId).NotEmpty()
            .WithErrorCode("Crm.Conversions.OrganizationId.Empty");
        RuleFor(x => x.LeadId).NotEmpty()
            .WithErrorCode("Crm.Conversions.LeadId.Empty");
        RuleFor(x => x.AcceptedOfferId).NotEmpty();
        RuleFor(x => x.BranchId).NotEmpty();
        RuleFor(x => x.ResponsibleUserId).NotEmpty();
        RuleFor(x => x.TrainingCode).NotEmpty().MaximumLength(100);
        RuleFor(x => x.GuardianSummary).MaximumLength(2000);
        RuleFor(x => x.PayerSummary).MaximumLength(2000);
        RuleForEach(x => x.RequiredDocumentCodes).NotEmpty().MaximumLength(100);
    }
}
