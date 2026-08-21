using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Modules.TrainingDelivery.Application.Persistence;
using DriveOS.Modules.TrainingDelivery.Domain.GroupSessions;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.TrainingDelivery.Application.GroupSessions;

public sealed class MaterializeGroupTrainingSessionCommandHandler(IGroupTrainingSessionRepository repository, IConfirmedGroupBookingSourceGateway sourceGateway, ITrainingDeliveryUnitOfWork uow) : ICommandHandler<MaterializeGroupTrainingSessionCommand, GroupTrainingSessionId>
{
    public async Task<Result<GroupTrainingSessionId>> Handle(MaterializeGroupTrainingSessionCommand c, CancellationToken ct)
    {
        GroupTrainingSession? existing = await repository.GetBySourceBookingAsync(c.OrganizationId, c.BookingId, ct);
        if (existing is not null) return Result.Success(existing.Id);
        Result<ConfirmedGroupBookingSource> sourceResult = await sourceGateway.GetAsync(c.OrganizationId, c.BookingId, ct);
        if (sourceResult.IsFailure) return Result.Failure<GroupTrainingSessionId>(sourceResult.Error);
        ConfirmedGroupBookingSource s = sourceResult.Value;
        Result<GroupTrainingSession> result = GroupTrainingSession.Materialize(new GroupTrainingSessionMaterialization(s.OrganizationId,s.BookingId,s.Program,s.Capacity,s.TrainerId,s.BranchId,s.RoomResourceId,s.RoomName,s.PlannedStartAtUtc,s.PlannedEndAtUtc,s.SharedObjectives,s.ParticipantStudentIds));
        if (result.IsFailure) return Result.Failure<GroupTrainingSessionId>(result.Error);
        repository.Add(result.Value); await uow.CommitAsync(ct); return Result.Success(result.Value.Id);
    }
}

public sealed class GetGroupTrainingSessionQueryHandler(IGroupTrainingSessionRepository repository) : IQueryHandler<GetGroupTrainingSessionQuery, GroupTrainingSessionResponse>
{
    public async Task<Result<GroupTrainingSessionResponse>> Handle(GetGroupTrainingSessionQuery q, CancellationToken ct)
    {
        GroupTrainingSession? session = await repository.GetByIdAsync(q.OrganizationId, q.SessionId, ct);
        return session is null ? Result.Failure<GroupTrainingSessionResponse>(GroupTrainingSessionErrors.NotFound) : Result.Success(Map(session));
    }
    internal static GroupTrainingSessionResponse Map(GroupTrainingSession s) => new(s.Id.Value,s.SourceBookingId.Value,s.Program,s.Capacity,s.TrainerId.Value,s.BranchId?.Value,s.RoomResourceId,s.RoomName,s.PlannedStartAtUtc,s.PlannedEndAtUtc,s.SharedObjectives,s.CollectiveReport,s.Participants.Count,s.Participants.Count(x=>x.AttendanceStatus is GroupSessionAttendanceStatus.Present or GroupSessionAttendanceStatus.Late),s.Participants.Count(x=>x.AttendanceStatus==GroupSessionAttendanceStatus.Absent),s.Participants.Select(x=>new GroupTrainingSessionParticipantResponse(x.Id.Value,x.StudentId.Value,x.AddedOutsideOriginalList,(int)x.AttendanceStatus,x.AttendanceMethod.HasValue?(int)x.AttendanceMethod.Value:null,x.CheckInAtUtc,x.CheckOutAtUtc,x.CompetencyId,x.AssessmentLevel,x.QuizScore,x.IndividualObservation,(int)x.CertificateStatus)).ToArray());
}

public abstract class GroupMutationHandlerBase(IGroupTrainingSessionRepository repository, ITrainingDeliveryUnitOfWork uow)
{
    protected async Task<Result<GroupTrainingSessionResponse>> Mutate(OrganizationId organizationId, GroupTrainingSessionId sessionId, Func<GroupTrainingSession,Result> action, CancellationToken ct)
    {
        GroupTrainingSession? session=await repository.GetByIdForUpdateAsync(organizationId, sessionId, ct); if(session is null)return Result.Failure<GroupTrainingSessionResponse>(GroupTrainingSessionErrors.NotFound); Result r=action(session); if(r.IsFailure)return Result.Failure<GroupTrainingSessionResponse>(r.Error); await uow.CommitAsync(ct); return Result.Success(GetGroupTrainingSessionQueryHandler.Map(session));
    }
}
public sealed class AddGroupParticipantCommandHandler(IGroupTrainingSessionRepository r,ITrainingDeliveryUnitOfWork u):GroupMutationHandlerBase(r,u),ICommandHandler<AddGroupParticipantCommand,GroupTrainingSessionResponse>{public Task<Result<GroupTrainingSessionResponse>> Handle(AddGroupParticipantCommand c,CancellationToken ct)=>Mutate(c.OrganizationId,c.SessionId,s=>{Result<GroupTrainingSessionParticipant> x=s.AddAuthorizedParticipant(c.StudentId,c.OperationId);return x.IsSuccess?Result.Success():Result.Failure(x.Error);},ct);}
public sealed class RecordGroupAttendanceCommandHandler(IGroupTrainingSessionRepository r,ITrainingDeliveryUnitOfWork u):GroupMutationHandlerBase(r,u),ICommandHandler<RecordGroupAttendanceCommand,GroupTrainingSessionResponse>{public Task<Result<GroupTrainingSessionResponse>> Handle(RecordGroupAttendanceCommand c,CancellationToken ct)=>Mutate(c.OrganizationId,c.SessionId,s=>s.RecordAttendance(c.StudentId,c.Status,c.Method,c.CheckInAtUtc,c.CheckOutAtUtc,c.ActorUserId.Value,c.OperationId),ct);}
public sealed class RecordGroupAssessmentCommandHandler(IGroupTrainingSessionRepository r,ITrainingDeliveryUnitOfWork u):GroupMutationHandlerBase(r,u),ICommandHandler<RecordGroupAssessmentCommand,GroupTrainingSessionResponse>{public Task<Result<GroupTrainingSessionResponse>> Handle(RecordGroupAssessmentCommand c,CancellationToken ct)=>Mutate(c.OrganizationId,c.SessionId,s=>s.RecordIndividualAssessment(c.StudentId,c.CompetencyId,c.Level,c.QuizScore,c.Observation,c.ActorUserId.Value,c.OperationId),ct);}
public sealed class SaveGroupReportCommandHandler(IGroupTrainingSessionRepository r,ITrainingDeliveryUnitOfWork u):GroupMutationHandlerBase(r,u),ICommandHandler<SaveGroupReportCommand,GroupTrainingSessionResponse>{public Task<Result<GroupTrainingSessionResponse>> Handle(SaveGroupReportCommand c,CancellationToken ct)=>Mutate(c.OrganizationId,c.SessionId,s=>s.SaveCollectiveReport(c.Report,c.SharedObjectives,c.OperationId),ct);}
public sealed class PrepareGroupCertificateCommandHandler(IGroupTrainingSessionRepository r,ITrainingDeliveryUnitOfWork u):GroupMutationHandlerBase(r,u),ICommandHandler<PrepareGroupCertificateCommand,GroupTrainingSessionResponse>{public Task<Result<GroupTrainingSessionResponse>> Handle(PrepareGroupCertificateCommand c,CancellationToken ct)=>Mutate(c.OrganizationId,c.SessionId,s=>s.MarkCertificateReady(c.StudentId,c.OperationId),ct);}
