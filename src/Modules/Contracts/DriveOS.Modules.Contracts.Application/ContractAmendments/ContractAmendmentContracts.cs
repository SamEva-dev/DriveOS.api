using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Modules.Contracts.Domain.TrainingContracts;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.Contracts.Application.ContractAmendments;

public sealed record CreateContractAmendmentCommand(
    OrganizationId OrganizationId,
    TrainingContractId ContractId,
    string Reason,
    DateOnly EffectiveDate,
    DateOnly StartDate,
    DateOnly? EndDate,
    decimal TotalAmount,
    string Currency,
    decimal PracticalHours,
    string ServicesSnapshot,
    string PaymentScheduleSnapshot,
    string CancellationTerms,
    string BookingRules,
    string StudentObligations,
    string ProviderObligations,
    string ExamPresentationTerms,
    string DataProcessingTerms,
    UserId ActorUserId) : ICommand<CreateContractAmendmentResponse>;

public sealed record CreateContractAmendmentResponse(Guid AmendmentId, int AmendmentNumber, string Status);

public sealed record RecordContractAmendmentSignedProofCommand(
    OrganizationId OrganizationId,
    TrainingContractId ContractId,
    ContractAmendmentId AmendmentId,
    string SignedDocumentReference,
    string DocumentSha256,
    DateTimeOffset SignedAtUtc,
    UserId ActorUserId) : ICommand;

public sealed record ApplyContractAmendmentCommand(
    OrganizationId OrganizationId,
    TrainingContractId ContractId,
    ContractAmendmentId AmendmentId,
    UserId ActorUserId) : ICommand<ApplyContractAmendmentResponse>;

public sealed record ApplyContractAmendmentResponse(Guid AmendmentId, int NewContractVersionNumber, string ContractStatus, string AmendmentStatus);

public sealed record CancelContractAmendmentCommand(
    OrganizationId OrganizationId,
    TrainingContractId ContractId,
    ContractAmendmentId AmendmentId,
    string Reason,
    UserId ActorUserId) : ICommand;
