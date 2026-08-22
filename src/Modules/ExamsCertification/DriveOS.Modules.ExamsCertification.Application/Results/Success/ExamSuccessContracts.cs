using DriveOS.Application.Abstractions.Messaging;
using DriveOS.SharedKernel.Identifiers;
namespace DriveOS.Modules.ExamsCertification.Application.Results.Success;
public sealed record GetExamSuccessProcessQuery(OrganizationId OrganizationId, ExamResultId ResultId) : IQuery<ExamSuccessProcessResponse>;
public sealed record CompleteExamSuccessProcessCommand(OrganizationId OrganizationId, ExamResultId ResultId, int ResultRevision, UserId ActorUserId) : ICommand<ExamSuccessProcessResponse>;
public sealed record ArchiveExamSuccessProcessCommand(OrganizationId OrganizationId, ExamResultId ResultId, int ResultRevision, UserId ActorUserId) : ICommand<ExamSuccessProcessResponse>;
public sealed record ExamSuccessActionResponse(string Code, bool Blocking, string Status, string? EvidenceReference, string? ReasonCode, string? Detail, DateTimeOffset? UpdatedAtUtc);
public sealed record ExamSuccessProcessResponse(Guid Id, Guid ResultId, int ResultRevision, Guid AttemptId, Guid RegistrationId, Guid StudentId, int AttemptNumber, string Status, IReadOnlyCollection<ExamSuccessActionResponse> Actions, DateTimeOffset CreatedAtUtc, DateTimeOffset? CompletedAtUtc, DateTimeOffset? SupersededAtUtc, DateTimeOffset? ArchivedAtUtc);
