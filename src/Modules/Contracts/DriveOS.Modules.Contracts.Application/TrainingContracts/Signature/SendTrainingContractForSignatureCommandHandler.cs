using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Application.Abstractions.Time;
using DriveOS.Modules.Contracts.Application.Persistence;
using DriveOS.Modules.Contracts.Domain.SignatureProcesses;
using DriveOS.Modules.Contracts.Domain.TrainingContracts;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Contracts.Application.TrainingContracts.Signature;

public sealed class SendTrainingContractForSignatureCommandHandler(
    ITrainingContractRepository contracts, ISignatureProcessRepository signatureProcesses,
    IContractsUnitOfWork uow, IClock clock) : ICommandHandler<SendTrainingContractForSignatureCommand, SendTrainingContractForSignatureResponse>
{
    public async Task<Result<SendTrainingContractForSignatureResponse>> Handle(SendTrainingContractForSignatureCommand command, CancellationToken ct)
    {
        TrainingContract? contract = await contracts.GetByIdAsync(command.ContractId, ct);
        if (contract is null || contract.OrganizationId != command.OrganizationId)
            return Result.Failure<SendTrainingContractForSignatureResponse>(TrainingContractErrors.NotFound);
        if (!contract.CanSendForSignature)
            return Result.Failure<SendTrainingContractForSignatureResponse>(TrainingContractErrors.SendForSignatureNotAllowed);
        if (await signatureProcesses.ExistsForContractVersionAsync(contract.Id, contract.CurrentVersionNumber, ct))
            return Result.Failure<SendTrainingContractForSignatureResponse>(TrainingContractErrors.SendForSignatureNotAllowed);

        var recipients = contract.Signatories.Select(x => new SignatureProcessRecipientSnapshot(
            x.Id, x.Kind.ToString(), x.PersonId, x.RepresentedOrganizationId, x.DisplayName, x.SigningOrder, x.IsRequired));
        DateTimeOffset now = clock.UtcNow;
        Result<SignatureProcess> created = SignatureProcess.Create(SignatureProcessId.New(), contract.OrganizationId, contract.Id,
            contract.CurrentVersionNumber, contract.GeneratedDocumentReference!, contract.GeneratedDocumentSha256!, recipients,
            command.ActorUserId, now);
        if (created.IsFailure) return Result.Failure<SendTrainingContractForSignatureResponse>(created.Error);

        Result marked = contract.MarkSentForSignature(created.Value.Id, command.ActorUserId, now);
        if (marked.IsFailure) return Result.Failure<SendTrainingContractForSignatureResponse>(marked.Error);
        await signatureProcesses.AddAsync(created.Value, ct);
        contract.SetModifiedAudit(now, command.ActorUserId);
        await uow.CommitAsync(ct);
        return Result.Success(new SendTrainingContractForSignatureResponse(created.Value.Id.Value, created.Value.Status.ToString(), created.Value.RequestedAtUtc));
    }
}
