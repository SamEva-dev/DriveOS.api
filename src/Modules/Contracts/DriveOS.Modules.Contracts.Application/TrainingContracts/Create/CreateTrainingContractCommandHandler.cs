using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Application.Abstractions.Time;
using DriveOS.Modules.Contracts.Application.Persistence;
using DriveOS.Modules.Contracts.Domain.TrainingContracts;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Contracts.Application.TrainingContracts.Create;

public sealed class CreateTrainingContractCommandHandler(
    ITrainingContractSourceGateway sourceGateway,
    ITrainingContractRepository contracts,
    IContractsUnitOfWork unitOfWork,
    IClock clock) : ICommandHandler<CreateTrainingContractCommand, TrainingContractId>
{
    public async Task<Result<TrainingContractId>> Handle(CreateTrainingContractCommand command, CancellationToken cancellationToken)
    {
        Result<TrainingContractSourceSnapshot> sourceResult = await sourceGateway.ResolveAsync(
            command.OrganizationId, command.EnrollmentId, command.SourceOfferId, cancellationToken);
        if (sourceResult.IsFailure) return Result.Failure<TrainingContractId>(sourceResult.Error);
        TrainingContractSourceSnapshot source = sourceResult.Value;

        if (await contracts.GetByContractNumberAsync(command.OrganizationId, command.ContractNumber.Trim(), cancellationToken) is not null)
            return Result.Failure<TrainingContractId>(CreateTrainingContractErrors.ContractNumberAlreadyExists);

        Result<TrainingContractTermsSnapshot> termsResult = TrainingContractTermsSnapshot.Create(
            source.TrainingCode, command.PracticalHours, command.ServicesSnapshot,
            command.PaymentScheduleSnapshot, command.CancellationTerms, command.BookingRules,
            command.StudentObligations, command.ProviderObligations,
            command.ExamPresentationTerms, command.DataProcessingTerms);
        if (termsResult.IsFailure) return Result.Failure<TrainingContractId>(termsResult.Error);

        Result<TrainingContractParty> provider = TrainingContractParty.ForOrganization(
            TrainingContractPartyKind.TrainingProvider, source.OrganizationId,
            source.ProviderDisplayName, command.ProviderLegalReference);
        if (provider.IsFailure) return Result.Failure<TrainingContractId>(provider.Error);
        Result<TrainingContractParty> student = TrainingContractParty.ForPerson(
            TrainingContractPartyKind.Student, source.StudentId,
            source.StudentDisplayName, command.StudentLegalReference);
        if (student.IsFailure) return Result.Failure<TrainingContractId>(student.Error);

        Result<TrainingContract> contractResult = TrainingContract.CreateDraft(
            TrainingContractId.New(), source.OrganizationId, source.BranchId, source.StudentId,
            source.OfferId, source.OfferVersion, command.ContractNumber, command.StartDate,
            command.EndDate, source.TotalAmount, source.Currency, termsResult.Value,
            [provider.Value, student.Value]);
        if (contractResult.IsFailure) return Result.Failure<TrainingContractId>(contractResult.Error);

        contractResult.Value.SetCreatedAudit(clock.UtcNow, command.ActorUserId);
        await contracts.AddAsync(contractResult.Value, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);
        return Result.Success(contractResult.Value.Id);
    }
}
