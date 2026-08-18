using DriveOS.Application.Abstractions.Messaging;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.Contracts.Application.TrainingContracts.Generate;

public sealed record GenerateTrainingContractCommand(
    OrganizationId OrganizationId,
    TrainingContractId ContractId,
    UserId ActorUserId) : ICommand<GeneratedTrainingContractResponse>;

public sealed record GeneratedTrainingContractResponse(
    Guid ContractId,
    int VersionNumber,
    string FileName,
    string ContentType,
    string Sha256,
    DateTimeOffset GeneratedAtUtc);

public sealed record TrainingContractGeneratedPayload(
    string FileName,
    string ContentType,
    byte[] Content,
    string Sha256);

public interface ITrainingContractDocumentGenerator
{
    Task<TrainingContractGeneratedPayload> GenerateAsync(
        Domain.TrainingContracts.TrainingContract contract,
        CancellationToken cancellationToken = default);
}

public interface ITrainingContractDocumentStorage
{
    Task<string> StoreAsync(
        OrganizationId organizationId,
        TrainingContractId contractId,
        int versionNumber,
        string fileName,
        ReadOnlyMemory<byte> content,
        CancellationToken cancellationToken = default);
}
