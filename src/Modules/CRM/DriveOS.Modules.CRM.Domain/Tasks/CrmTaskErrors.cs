using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.CRM.Domain.Tasks;

public static class CrmTaskErrors
{
    public static readonly Error InvalidIdentifier = Error.Validation("Crm.Tasks.Identifier.Invalid", "errors.crm.tasks.identifier.invalid");
    public static readonly Error NotFound = Error.NotFound("Crm.Tasks.NotFound", "errors.crm.tasks.notFound");
    public static readonly Error TitleRequired = Error.Validation("Crm.Tasks.Title.Required", "errors.crm.tasks.title.required");
    public static readonly Error TitleTooLong = Error.Validation("Crm.Tasks.Title.TooLong", "errors.crm.tasks.title.tooLong");
    public static readonly Error NotesTooLong = Error.Validation("Crm.Tasks.Notes.TooLong", "errors.crm.tasks.notes.tooLong");
    public static readonly Error DueDateRequired = Error.Validation("Crm.Tasks.DueDate.Required", "errors.crm.tasks.dueDate.required");
    public static readonly Error AlreadyClosed = Error.Conflict("Crm.Tasks.AlreadyClosed", "errors.crm.tasks.alreadyClosed");
}
