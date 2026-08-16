using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.CRM.Domain.Assessments;

public static class AssessmentAppointmentErrors
{
    public static readonly Error InvalidIdentifier = Error.Validation(
        "Crm.Assessments.Id.Invalid",
        "errors.crm.assessments.id.invalid"
    );

    public static readonly Error NotFound = Error.NotFound(
        "Crm.Assessments.NotFound",
        "errors.crm.assessments.notFound"
    );

    public static readonly Error InvalidPeriod = Error.Validation(
        "Crm.Assessments.Period.Invalid",
        "errors.crm.assessments.period.invalid"
    );

    public static readonly Error NotesTooLong = Error.Validation(
        "Crm.Assessments.Notes.TooLong",
        "errors.crm.assessments.notes.tooLong"
    );

    public static readonly Error BranchNotAvailable = Error.Validation(
        "Crm.Assessments.Branch.NotAvailable",
        "errors.crm.assessments.branch.notAvailable"
    );

    public static readonly Error AlreadyClosed = Error.Conflict(
        "Crm.Assessments.AlreadyClosed",
        "errors.crm.assessments.alreadyClosed"
    );

    public static readonly Error BranchRequired = Error.Validation(
        "Crm.Assessments.Branch.Required",
        "errors.crm.assessments.branch.required"
    );

    public static readonly Error LocationDetailsRequired = Error.Validation(
        "Crm.Assessments.Location.DetailsRequired",
        "errors.crm.assessments.location.detailsRequired"
    );

    public static readonly Error LocationDetailsTooLong = Error.Validation(
        "Crm.Assessments.Location.DetailsTooLong",
        "errors.crm.assessments.location.detailsTooLong"
    );

    public static readonly Error InvalidRemoteLocation = Error.Validation(
        "Crm.Assessments.Location.InvalidRemote",
        "errors.crm.assessments.location.invalidRemote"
    );

    public static readonly Error SimulatorRequired = Error.Validation(
        "Crm.Assessments.Simulator.Required",
        "errors.crm.assessments.simulator.required"
    );

    public static readonly Error InvalidResourceIdentifier = Error.Validation(
        "Crm.Assessments.Resource.Id.Invalid",
        "errors.crm.assessments.resource.id.invalid"
    );

    public static readonly Error InvalidPrice = Error.Validation(
        "Crm.Assessments.Price.Invalid",
        "errors.crm.assessments.price.invalid"
    );

    public static readonly Error IncompletePrice = Error.Validation(
        "Crm.Assessments.Price.Incomplete",
        "errors.crm.assessments.price.incomplete"
    );

    public static readonly Error InvalidCurrency = Error.Validation(
        "Crm.Assessments.Price.Currency.Invalid",
        "errors.crm.assessments.price.currency.invalid"
    );

    public static readonly Error SchedulingConflict = Error.Conflict(
        "Crm.Assessments.Scheduling.Conflict",
        "errors.crm.assessments.scheduling.conflict"
    );
}
