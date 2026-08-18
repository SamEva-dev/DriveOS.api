using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Application.Abstractions.Time;
using DriveOS.Modules.Contracts.Application.Persistence;
using DriveOS.Modules.Contracts.Domain.TrainingContracts;
using DriveOS.Modules.Contracts.Domain.ContractDocuments;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Contracts.Application.TrainingContracts.Generate;

public sealed class GenerateTrainingContractCommandHandler(
    ITrainingContractRepository contracts,
    ITrainingContractDocumentGenerator generator,
    ITrainingContractDocumentStorage storage,
    IContractDocumentRepository documents,
    IContractsUnitOfWork unitOfWork,
    IClock clock) : ICommandHandler<GenerateTrainingContractCommand, GeneratedTrainingContractResponse>
{
    public async Task<Result<GeneratedTrainingContractResponse>> Handle(
        GenerateTrainingContractCommand command,
        CancellationToken cancellationToken)
    {
        TrainingContract? contract = await contracts.GetByIdAsync(command.ContractId, cancellationToken);
        if (contract is null || contract.OrganizationId != command.OrganizationId)
            return Result.Failure<GeneratedTrainingContractResponse>(TrainingContractErrors.NotFound);

        if (!contract.CanGenerate)
            return Result.Failure<GeneratedTrainingContractResponse>(TrainingContractErrors.GenerationNotAllowed);

        TrainingContractGeneratedPayload payload = await generator.GenerateAsync(contract, cancellationToken);
        string reference = await storage.StoreAsync(
            contract.OrganizationId,
            contract.Id,
            contract.CurrentVersionNumber,
            payload.FileName,
            payload.Content,
            cancellationToken);

        DateTimeOffset generatedAt = clock.UtcNow;
        Result mark = contract.MarkGenerated(
            reference,
            payload.FileName,
            payload.ContentType,
            payload.Sha256,
            command.ActorUserId,
            generatedAt);
        if (mark.IsFailure)
            return Result.Failure<GeneratedTrainingContractResponse>(mark.Error);

        contract.SetModifiedAudit(generatedAt, command.ActorUserId);

        ContractDocumentId documentId = ContractDocumentId.New();
        Result<ContractDocumentVersion> documentVersion = ContractDocumentVersion.Create(
            ContractDocumentVersionId.New(), documentId, 1, payload.FileName, payload.ContentType,
            payload.Content.LongLength, reference, payload.Sha256, command.ActorUserId, generatedAt);
        if (documentVersion.IsFailure)
            return Result.Failure<GeneratedTrainingContractResponse>(documentVersion.Error);

        Result<ContractDocument> contractDocument = ContractDocument.Create(
            documentId, contract.OrganizationId, contract.Id, contract.CurrentVersionNumber,
            ContractDocumentType.MainContract, $"{contract.ContractNumber} - V{contract.CurrentVersionNumber}",
            ContractDocumentVisibility.Signatories, retainUntil: null, legalBasis: "Contractual evidence", documentVersion.Value);
        if (contractDocument.IsFailure)
            return Result.Failure<GeneratedTrainingContractResponse>(contractDocument.Error);
        contractDocument.Value.SetCreatedAudit(generatedAt, command.ActorUserId);
        await documents.AddAsync(contractDocument.Value, cancellationToken);

        await unitOfWork.CommitAsync(cancellationToken);

        return Result.Success(new GeneratedTrainingContractResponse(
            contract.Id.Value,
            contract.GeneratedDocumentVersionNumber!.Value,
            contract.GeneratedDocumentFileName!,
            contract.GeneratedDocumentContentType!,
            contract.GeneratedDocumentSha256!,
            contract.GeneratedAtUtc!.Value));
    }
}
