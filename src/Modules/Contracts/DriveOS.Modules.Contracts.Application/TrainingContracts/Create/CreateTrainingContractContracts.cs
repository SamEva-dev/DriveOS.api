using DriveOS.Application.Abstractions.Messaging;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Contracts.Application.TrainingContracts.Create;

public sealed record CreateTrainingContractCommand(
    OrganizationId OrganizationId,
    DraftEnrollmentId EnrollmentId,
    CommercialOfferId SourceOfferId,
    string ContractNumber,
    DateOnly StartDate,
    DateOnly? EndDate,
    decimal PracticalHours,
    string ServicesSnapshot,
    string PaymentScheduleSnapshot,
    string CancellationTerms,
    string BookingRules,
    string StudentObligations,
    string ProviderObligations,
    string ExamPresentationTerms,
    string DataProcessingTerms,
    string? ProviderLegalReference,
    string? StudentLegalReference,
    UserId ActorUserId) : ICommand<TrainingContractId>;

public sealed record TrainingContractSourceSnapshot(
    OrganizationId OrganizationId,
    BranchId BranchId,
    PersonId StudentId,
    string StudentDisplayName,
    string ProviderDisplayName,
    CommercialOfferId OfferId,
    int OfferVersion,
    string TrainingCode,
    decimal TotalAmount,
    string Currency);

public interface ITrainingContractSourceGateway
{
    Task<Result<TrainingContractSourceSnapshot>> ResolveAsync(
        OrganizationId organizationId,
        DraftEnrollmentId enrollmentId,
        CommercialOfferId offerId,
        CancellationToken cancellationToken = default);
}

public static class CreateTrainingContractErrors
{
    public static readonly Error EnrollmentNotFound = Error.NotFound("Contracts.Source.Enrollment.NotFound", "errors.contracts.source.enrollment.notFound");
    public static readonly Error OfferNotFound = Error.NotFound("Contracts.Source.Offer.NotFound", "errors.contracts.source.offer.notFound");
    public static readonly Error OfferNotAccepted = Error.Conflict("Contracts.Source.Offer.NotAccepted", "errors.contracts.source.offer.notAccepted");
    public static readonly Error SourceMismatch = Error.Conflict("Contracts.Source.Mismatch", "errors.contracts.source.mismatch");
    public static readonly Error StudentNotFound = Error.NotFound("Contracts.Source.Student.NotFound", "errors.contracts.source.student.notFound");
    public static readonly Error OrganizationNotFound = Error.NotFound("Contracts.Source.Organization.NotFound", "errors.contracts.source.organization.notFound");
    public static readonly Error ContractNumberAlreadyExists = Error.Conflict("Contracts.TrainingContract.Number.AlreadyExists", "errors.contracts.trainingContract.number.alreadyExists");
}
