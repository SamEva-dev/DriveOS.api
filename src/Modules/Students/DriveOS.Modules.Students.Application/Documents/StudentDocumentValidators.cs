using FluentValidation;

namespace DriveOS.Modules.Students.Application.Documents;

public sealed class RequestStudentDocumentCommandValidator
    : AbstractValidator<RequestStudentDocumentCommand>
{
    public RequestStudentDocumentCommandValidator()
    {
        RuleFor(x => x.DocumentType).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Visibility).NotEmpty();
    }
}

public sealed class UploadStudentDocumentCommandValidator
    : AbstractValidator<UploadStudentDocumentCommand>
{
    public UploadStudentDocumentCommandValidator()
    {
        RuleFor(x => x.DocumentId).NotEmpty();
        RuleFor(x => x.FileName).NotEmpty().MaximumLength(255);
        RuleFor(x => x.ContentType).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Length).InclusiveBetween(1, 15 * 1024 * 1024);
    }
}

public sealed class ValidateStudentDocumentCommandValidator
    : AbstractValidator<ValidateStudentDocumentCommand>
{
    public ValidateStudentDocumentCommandValidator()
    {
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(500).When(x => !x.Approve);
    }
}
