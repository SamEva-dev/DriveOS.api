using DriveOS.Modules.Organizations.Domain.Branches;
using FluentValidation;

namespace DriveOS.Modules.Organizations.Application.Branches.UpdateBranch;

internal sealed class UpdateBranchCommandValidator :
    AbstractValidator<UpdateBranchCommand>
{
    public UpdateBranchCommandValidator()
    {
        RuleFor(command => command.OrganizationId)
            .Must(id => !id.IsEmpty);

        RuleFor(command => command.BranchId)
            .Must(id => !id.IsEmpty);

        RuleFor(command => command.Name)
            .NotEmpty()
            .MaximumLength(BranchName.MaximumLength);

        RuleFor(command => command.BranchType)
            .IsInEnum();

        RuleFor(command => command.AddressLine1)
            .NotEmpty()
            .MaximumLength(BranchAddress.AddressLineMaximumLength);

        RuleFor(command => command.AddressLine2)
            .MaximumLength(BranchAddress.AddressLineMaximumLength);

        RuleFor(command => command.PostalCode)
            .NotEmpty()
            .MaximumLength(BranchAddress.PostalCodeMaximumLength);

        RuleFor(command => command.City)
            .NotEmpty()
            .MaximumLength(BranchAddress.CityMaximumLength);

        RuleFor(command => command.TimeZoneId)
            .NotEmpty()
            .MaximumLength(100);
    }
}
