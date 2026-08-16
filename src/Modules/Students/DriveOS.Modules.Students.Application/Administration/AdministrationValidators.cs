using FluentValidation;

namespace DriveOS.Modules.Students.Application.Administration;

public sealed class ConfigureRequirementCommandValidator
    : AbstractValidator<ConfigureRequirementCommand>
{
    public ConfigureRequirementCommandValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(80);
        RuleFor(x => x.LabelKey).NotEmpty().MaximumLength(200);
        RuleFor(x => x.PolicySource).NotEmpty().MaximumLength(100);
    }
}

public sealed class AddAdministrativeBlockCommandValidator
    : AbstractValidator<AddAdministrativeBlockCommand>
{
    public AddAdministrativeBlockCommandValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(80);
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(500);
    }
}

public sealed class RequestComplianceExceptionCommandValidator
    : AbstractValidator<RequestComplianceExceptionCommand>
{
    public RequestComplianceExceptionCommandValidator()
    {
        RuleFor(x => x.Reason).NotEmpty().MinimumLength(10).MaximumLength(500);
    }
}
