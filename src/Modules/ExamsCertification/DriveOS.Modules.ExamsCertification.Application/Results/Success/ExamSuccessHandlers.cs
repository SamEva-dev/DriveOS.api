using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Application.Abstractions.Time;
using DriveOS.Modules.ExamsCertification.Application.Persistence;
using DriveOS.Modules.ExamsCertification.Domain.Results.Success;
using DriveOS.SharedKernel.Results;
namespace DriveOS.Modules.ExamsCertification.Application.Results.Success;
internal static class ExamSuccessProcessMapper
{
    internal static ExamSuccessProcessResponse Map(ExamSuccessProcess x) => new(x.Id.Value, x.ExamResultId.Value, x.ResultRevision, x.AttemptId.Value, x.RegistrationId.Value, x.StudentId.Value, x.AttemptNumber, x.Status.ToString(), x.Actions.OrderBy(a => a.Code).Select(a => new ExamSuccessActionResponse(a.Code.ToString(), a.Blocking, a.Status.ToString(), a.EvidenceReference, a.ReasonCode, a.Detail, a.UpdatedAtUtc)).ToArray(), x.CreatedAtUtc, x.CompletedAtUtc, x.SupersededAtUtc, x.ArchivedAtUtc);
}
public sealed class GetExamSuccessProcessQueryHandler(IExamSuccessProcessRepository repository) : IQueryHandler<GetExamSuccessProcessQuery, ExamSuccessProcessResponse>
{
    public async Task<Result<ExamSuccessProcessResponse>> Handle(GetExamSuccessProcessQuery q, CancellationToken ct) { var p = await repository.GetLatestByResultAsync(q.OrganizationId, q.ResultId, ct); return p is null ? Result.Failure<ExamSuccessProcessResponse>(ExamSuccessProcessErrors.NotFound) : Result.Success(ExamSuccessProcessMapper.Map(p)); }
}
public sealed class CompleteExamSuccessProcessCommandHandler(IExamSuccessProcessRepository repository, IExamsCertificationUnitOfWork uow, IClock clock) : ICommandHandler<CompleteExamSuccessProcessCommand, ExamSuccessProcessResponse>
{
    public async Task<Result<ExamSuccessProcessResponse>> Handle(CompleteExamSuccessProcessCommand c, CancellationToken ct) { var p=await repository.GetByResultForUpdateAsync(c.OrganizationId,c.ResultId,c.ResultRevision,ct); if(p is null)return Result.Failure<ExamSuccessProcessResponse>(ExamSuccessProcessErrors.NotFound); var r=p.Complete(c.ActorUserId,clock.UtcNow); if(r.IsFailure)return Result.Failure<ExamSuccessProcessResponse>(r.Error); await uow.CommitAsync(ct); return Result.Success(ExamSuccessProcessMapper.Map(p)); }
}
public sealed class ArchiveExamSuccessProcessCommandHandler(IExamSuccessProcessRepository repository, IExamsCertificationUnitOfWork uow, IClock clock) : ICommandHandler<ArchiveExamSuccessProcessCommand, ExamSuccessProcessResponse>
{
    public async Task<Result<ExamSuccessProcessResponse>> Handle(ArchiveExamSuccessProcessCommand c, CancellationToken ct) { var p=await repository.GetByResultForUpdateAsync(c.OrganizationId,c.ResultId,c.ResultRevision,ct); if(p is null)return Result.Failure<ExamSuccessProcessResponse>(ExamSuccessProcessErrors.NotFound); var r=p.Archive(c.ActorUserId,clock.UtcNow); if(r.IsFailure)return Result.Failure<ExamSuccessProcessResponse>(r.Error); await uow.CommitAsync(ct); return Result.Success(ExamSuccessProcessMapper.Map(p)); }
}
