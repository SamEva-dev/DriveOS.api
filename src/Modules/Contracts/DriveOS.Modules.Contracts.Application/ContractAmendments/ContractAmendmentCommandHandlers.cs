using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Application.Abstractions.Time;
using DriveOS.Modules.Contracts.Application.Persistence;
using DriveOS.Modules.Contracts.Domain.ContractAmendments;
using DriveOS.Modules.Contracts.Domain.TrainingContracts;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Contracts.Application.ContractAmendments;

public sealed class CreateContractAmendmentCommandHandler(
    ITrainingContractRepository contracts,
    IContractAmendmentRepository amendments,
    IContractsUnitOfWork uow,
    IClock clock) : ICommandHandler<CreateContractAmendmentCommand, CreateContractAmendmentResponse>
{
    public async Task<Result<CreateContractAmendmentResponse>> Handle(CreateContractAmendmentCommand command, CancellationToken ct)
    {
        TrainingContract? contract = await contracts.GetByIdAsync(command.ContractId, ct);
        if (contract is null || contract.OrganizationId != command.OrganizationId)
            return Result.Failure<CreateContractAmendmentResponse>(TrainingContractErrors.NotFound);
        if (!contract.CanAmend)
            return Result.Failure<CreateContractAmendmentResponse>(TrainingContractErrors.AmendmentNotAllowed);

        Result<TrainingContractTermsSnapshot> terms = TrainingContractTermsSnapshot.Create(
            contract.TermsSnapshot.TrainingCode,
            command.PracticalHours,
            command.ServicesSnapshot,
            command.PaymentScheduleSnapshot,
            command.CancellationTerms,
            command.BookingRules,
            command.StudentObligations,
            command.ProviderObligations,
            command.ExamPresentationTerms,
            command.DataProcessingTerms);
        if (terms.IsFailure)
            return Result.Failure<CreateContractAmendmentResponse>(terms.Error);

        int amendmentNumber = await amendments.GetNextNumberAsync(command.OrganizationId, command.ContractId, ct);
        Result<ContractAmendment> created = ContractAmendment.CreateDraft(
            ContractAmendmentId.New(), command.OrganizationId, command.ContractId, amendmentNumber,
            contract.CurrentVersionNumber, command.Reason, command.EffectiveDate, command.StartDate, command.EndDate,
            command.TotalAmount, command.Currency, terms.Value);
        if (created.IsFailure)
            return Result.Failure<CreateContractAmendmentResponse>(created.Error);

        DateTimeOffset now = clock.UtcNow;
        created.Value.SetCreatedAudit(now, command.ActorUserId);
        await amendments.AddAsync(created.Value, ct);
        await uow.CommitAsync(ct);
        return Result.Success(new CreateContractAmendmentResponse(created.Value.Id.Value, amendmentNumber, created.Value.Status.ToString()));
    }
}

public sealed class RecordContractAmendmentSignedProofCommandHandler(
    ITrainingContractRepository contracts,
    IContractAmendmentRepository amendments,
    IContractsUnitOfWork uow,
    IClock clock) : ICommandHandler<RecordContractAmendmentSignedProofCommand>
{
    public async Task<Result> Handle(RecordContractAmendmentSignedProofCommand command, CancellationToken ct)
    {
        TrainingContract? contract = await contracts.GetByIdAsync(command.ContractId, ct);
        if (contract is null || contract.OrganizationId != command.OrganizationId) return Result.Failure(TrainingContractErrors.NotFound);
        ContractAmendment? amendment = await amendments.GetByIdAsync(command.AmendmentId, ct);
        if (amendment is null || amendment.OrganizationId != command.OrganizationId || amendment.ContractId != command.ContractId)
            return Result.Failure(ContractAmendmentErrors.NotFound);
        Result result = amendment.MarkSigned(command.SignedDocumentReference, command.DocumentSha256, command.ActorUserId, command.SignedAtUtc);
        if (result.IsFailure) return result;
        amendment.SetModifiedAudit(clock.UtcNow, command.ActorUserId);
        await uow.CommitAsync(ct);
        return Result.Success();
    }
}

public sealed class ApplyContractAmendmentCommandHandler(
    ITrainingContractRepository contracts,
    IContractAmendmentRepository amendments,
    IContractsUnitOfWork uow,
    IClock clock) : ICommandHandler<ApplyContractAmendmentCommand, ApplyContractAmendmentResponse>
{
    public async Task<Result<ApplyContractAmendmentResponse>> Handle(ApplyContractAmendmentCommand command, CancellationToken ct)
    {
        TrainingContract? contract = await contracts.GetByIdAsync(command.ContractId, ct);
        if (contract is null || contract.OrganizationId != command.OrganizationId)
            return Result.Failure<ApplyContractAmendmentResponse>(TrainingContractErrors.NotFound);
        ContractAmendment? amendment = await amendments.GetByIdAsync(command.AmendmentId, ct);
        if (amendment is null || amendment.OrganizationId != command.OrganizationId || amendment.ContractId != command.ContractId)
            return Result.Failure<ApplyContractAmendmentResponse>(ContractAmendmentErrors.NotFound);

        DateTimeOffset now = clock.UtcNow;
        Result<int> applied = contract.ApplySignedAmendment(amendment, command.ActorUserId, now);
        if (applied.IsFailure) return Result.Failure<ApplyContractAmendmentResponse>(applied.Error);
        Result amendmentApplied = amendment.MarkApplied(applied.Value, command.ActorUserId, now);
        if (amendmentApplied.IsFailure) return Result.Failure<ApplyContractAmendmentResponse>(amendmentApplied.Error);
        contract.SetModifiedAudit(now, command.ActorUserId);
        amendment.SetModifiedAudit(now, command.ActorUserId);
        await uow.CommitAsync(ct);
        return Result.Success(new ApplyContractAmendmentResponse(amendment.Id.Value, applied.Value, contract.Status.ToString(), amendment.Status.ToString()));
    }
}

public sealed class CancelContractAmendmentCommandHandler(
    ITrainingContractRepository contracts,
    IContractAmendmentRepository amendments,
    IContractsUnitOfWork uow,
    IClock clock) : ICommandHandler<CancelContractAmendmentCommand>
{
    public async Task<Result> Handle(CancelContractAmendmentCommand command, CancellationToken ct)
    {
        TrainingContract? contract = await contracts.GetByIdAsync(command.ContractId, ct);
        if (contract is null || contract.OrganizationId != command.OrganizationId) return Result.Failure(TrainingContractErrors.NotFound);
        ContractAmendment? amendment = await amendments.GetByIdAsync(command.AmendmentId, ct);
        if (amendment is null || amendment.OrganizationId != command.OrganizationId || amendment.ContractId != command.ContractId)
            return Result.Failure(ContractAmendmentErrors.NotFound);
        Result result = amendment.Cancel(command.Reason, command.ActorUserId, clock.UtcNow);
        if (result.IsFailure) return result;
        amendment.SetModifiedAudit(clock.UtcNow, command.ActorUserId);
        await uow.CommitAsync(ct);
        return Result.Success();
    }
}
