using FluentValidation;

namespace DriveOS.Modules.Organizations.Application.OrganizationSequences.Create;

internal sealed class CreateOrganizationSequenceCommandValidator
    : AbstractValidator<CreateOrganizationSequenceCommand>
{
    public CreateOrganizationSequenceCommandValidator()
    {
        RuleFor(command => command.OrganizationId).NotEmpty();
        RuleFor(command => command.Code).NotEmpty().MaximumLength(50);
        RuleFor(command => command.Pattern).NotEmpty().MaximumLength(200);
        RuleFor(command => command.Padding).InclusiveBetween(1, 18);
        RuleFor(command => command.InitialValue).GreaterThan(0);

        RuleFor(command => command.BranchId)
            .NotNull()
            .When(command => command.Scope == Domain.OrganizationSequences.OrganizationSequenceScope.Branch);

        RuleFor(command => command.BranchId)
            .Null()
            .When(command => command.Scope == Domain.OrganizationSequences.OrganizationSequenceScope.Organization);
    }
}
