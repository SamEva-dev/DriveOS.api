using DomainRelay.Abstractions;
using DriveOS.Application.Abstractions.Authentication;
using DriveOS.Modules.TrainingDelivery.Application.GroupSessions;
using DriveOS.Modules.TrainingDelivery.Domain.GroupSessions;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Api.Endpoints.TrainingDelivery;

internal static class GroupTrainingSessionEndpoints
{
    internal static IEndpointRouteBuilder MapGroupTrainingSessionEndpoints(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder g=app.MapGroup("/api/training-delivery/group-sessions").WithTags("Training Delivery - Group sessions");
        g.MapPost("/from-booking/{bookingId:guid}",Materialize).RequireAuthorization("TrainingDelivery.GroupSessions.Materialize");
        g.MapGet("/{sessionId:guid}",Get).RequireAuthorization("TrainingDelivery.GroupSessions.Read");
        g.MapPost("/{sessionId:guid}/participants",AddParticipant).RequireAuthorization("TrainingDelivery.GroupSessions.ManageParticipants");
        g.MapPost("/{sessionId:guid}/attendance",Attendance).RequireAuthorization("TrainingDelivery.GroupSessions.Attendance.Record");
        g.MapPost("/{sessionId:guid}/assessments",Assessment).RequireAuthorization("TrainingDelivery.GroupSessions.Assessments.Record");
        g.MapPut("/{sessionId:guid}/report",Report).RequireAuthorization("TrainingDelivery.GroupSessions.Report.Write");
        g.MapPost("/{sessionId:guid}/certificates/prepare",Certificate).RequireAuthorization("TrainingDelivery.GroupSessions.Certificates.Prepare");
        return app;
    }
    static async Task<IResult> Materialize(Guid bookingId,IMediator m,ICurrentTenant t,CancellationToken ct){if(t.OrganizationId is not{} o)return Results.Unauthorized();var r=await m.Send(new MaterializeGroupTrainingSessionCommand(o,new BookingId(bookingId)),ct);return Out(r,x=>new{id=x.Value});}
    static async Task<IResult> Get(Guid sessionId,IMediator m,ICurrentTenant t,CancellationToken ct){if(t.OrganizationId is not{} o)return Results.Unauthorized();var r=await m.Send(new GetGroupTrainingSessionQuery(o,new GroupTrainingSessionId(sessionId)),ct);return Out(r,x=>x);}
    static async Task<IResult> AddParticipant(Guid sessionId,AddParticipantRequest req,IMediator m,ICurrentTenant t,CancellationToken ct){if(t.OrganizationId is not{} o)return Results.Unauthorized();var r=await m.Send(new AddGroupParticipantCommand(o,new GroupTrainingSessionId(sessionId),new PersonId(req.StudentId),req.OperationId),ct);return Out(r,x=>x);}
    static async Task<IResult> Attendance(Guid sessionId,AttendanceRequest req,IMediator m,ICurrentTenant t,ICurrentUser u,CancellationToken ct){if(t.OrganizationId is not{} o||u.UserId is not{} a)return Results.Unauthorized();if(!Enum.IsDefined(typeof(GroupSessionAttendanceStatus),req.Status)||!Enum.IsDefined(typeof(GroupSessionAttendanceMethod),req.Method))return Results.BadRequest(new{code="TrainingDelivery.GroupSession.Attendance.Invalid",messageKey="errors.trainingDelivery.groupSession.attendance.invalid"});var r=await m.Send(new RecordGroupAttendanceCommand(o,new GroupTrainingSessionId(sessionId),new PersonId(req.StudentId),(GroupSessionAttendanceStatus)req.Status,(GroupSessionAttendanceMethod)req.Method,req.CheckInAtUtc,req.CheckOutAtUtc,a,req.OperationId),ct);return Out(r,x=>x);}
    static async Task<IResult> Assessment(Guid sessionId,AssessmentRequest req,IMediator m,ICurrentTenant t,ICurrentUser u,CancellationToken ct){if(t.OrganizationId is not{} o||u.UserId is not{} a)return Results.Unauthorized();var r=await m.Send(new RecordGroupAssessmentCommand(o,new GroupTrainingSessionId(sessionId),new PersonId(req.StudentId),req.CompetencyId,req.Level,req.QuizScore,req.Observation,a,req.OperationId),ct);return Out(r,x=>x);}
    static async Task<IResult> Report(Guid sessionId,ReportRequest req,IMediator m,ICurrentTenant t,CancellationToken ct){if(t.OrganizationId is not{} o)return Results.Unauthorized();var r=await m.Send(new SaveGroupReportCommand(o,new GroupTrainingSessionId(sessionId),req.Report,req.SharedObjectives,req.OperationId),ct);return Out(r,x=>x);}
    static async Task<IResult> Certificate(Guid sessionId,CertificateRequest req,IMediator m,ICurrentTenant t,CancellationToken ct){if(t.OrganizationId is not{} o)return Results.Unauthorized();var r=await m.Send(new PrepareGroupCertificateCommand(o,new GroupTrainingSessionId(sessionId),new PersonId(req.StudentId),req.OperationId),ct);return Out(r,x=>x);}
    static IResult Out<T,TOut>(Result<T> r,Func<T,TOut> map)=>r.IsSuccess?Results.Ok(map(r.Value)):TrainingDeliveryEndpointFailure.Map(r.Error);
    public sealed record AddParticipantRequest(Guid StudentId,Guid OperationId);
    public sealed record AttendanceRequest(Guid StudentId,int Status,int Method,DateTimeOffset? CheckInAtUtc,DateTimeOffset? CheckOutAtUtc,Guid OperationId);
    public sealed record AssessmentRequest(Guid StudentId,Guid? CompetencyId,int? Level,decimal? QuizScore,string? Observation,Guid OperationId);
    public sealed record ReportRequest(string Report,string? SharedObjectives,Guid OperationId);
    public sealed record CertificateRequest(Guid StudentId,Guid OperationId);
}

internal static class TrainingDeliveryEndpointFailure
{
    internal static IResult Map(Error e)=>e.Type switch{ErrorType.NotFound=>Results.NotFound(new{code=e.Code,messageKey=e.MessageKey}),ErrorType.Conflict=>Results.Conflict(new{code=e.Code,messageKey=e.MessageKey}),ErrorType.Validation=>Results.BadRequest(new{code=e.Code,messageKey=e.MessageKey}),_=>Results.BadRequest(new{code=e.Code,messageKey=e.MessageKey})};
}
