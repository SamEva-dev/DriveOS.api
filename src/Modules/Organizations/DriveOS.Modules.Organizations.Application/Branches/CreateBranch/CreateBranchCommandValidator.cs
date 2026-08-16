using DriveOS.Modules.Organizations.Domain.Branches;
using FluentValidation;

namespace DriveOS.Modules.Organizations.Application.Branches.CreateBranch;

public sealed class CreateBranchCommandValidator : AbstractValidator<CreateBranchCommand>
{
    public CreateBranchCommandValidator()
    {
        RuleFor(command => command.OrganizationId)
            .Must(id => !id.IsEmpty)
            .WithErrorCode("Branches.OrganizationId.Empty")
            .WithMessage("errors.branches.organizationId.empty");

        RuleFor(command => command.Name).NotEmpty().MaximumLength(BranchName.MaximumLength);

        RuleFor(command => command.Code)
            .NotEmpty()
            .MaximumLength(BranchCode.MaximumLength)
            .Matches("^[A-Za-z0-9][A-Za-z0-9_-]*$");

        RuleFor(command => command.BranchType).IsInEnum();

        RuleFor(command => command.AddressLine1)
            .NotEmpty()
            .MaximumLength(BranchAddress.AddressLineMaximumLength);

        RuleFor(command => command.AddressLine2)
            .MaximumLength(BranchAddress.AddressLineMaximumLength);

        RuleFor(command => command.PostalCode)
            .NotEmpty()
            .MaximumLength(BranchAddress.PostalCodeMaximumLength);

        RuleFor(command => command.City).NotEmpty().MaximumLength(BranchAddress.CityMaximumLength);

        RuleFor(command => command.TimeZoneId).NotEmpty().MaximumLength(100);
    }
}
