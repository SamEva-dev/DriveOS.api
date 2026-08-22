using DomainRelay.Abstractions;
using DriveOS.Application.Abstractions.Authentication;
using DriveOS.Modules.ExamsCertification.Application.Certifications;
using DriveOS.Security.Contracts;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Api.Endpoints.ExamsCertification;

internal static class ExamAttestationEndpoints
{
    internal static IEndpointRouteBuilder MapExamAttestationEndpoints(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("/api/exams").WithTags("Exams & Certification");
        group.MapPost("/results/{resultId:guid}/attestations", Issue).RequireAuthorization(DriveOsPermissionCodes.Exams.CertificationsIssue);
        group.MapGet("/results/{resultId:guid}/attestations", ListByResult).RequireAuthorization(DriveOsPermissionCodes.Exams.CertificationsRead);
        group.MapGet("/students/{studentId:guid}/attestations", ListByStudent).RequireAuthorization(DriveOsPermissionCodes.Exams.CertificationsRead);
        group.MapGet("/attestations/{attestationId:guid}", Get).RequireAuthorization(DriveOsPermissionCodes.Exams.CertificationsRead);
        group.MapPost("/attestations/{attestationId:guid}/correct", Correct).RequireAuthorization(DriveOsPermissionCodes.Exams.CertificationsIssue);
        group.MapPost("/attestations/{attestationId:guid}/sign", Sign).RequireAuthorization(DriveOsPermissionCodes.Exams.CertificationsIssue);
        group.MapPost("/attestations/{attestationId:guid}/deliver", Deliver).RequireAuthorization(DriveOsPermissionCodes.Exams.CertificationsIssue);
        group.MapPost("/attestations/{attestationId:guid}/revoke", Revoke).RequireAuthorization(DriveOsPermissionCodes.Exams.CertificationsRevoke);
        app.MapGet("/api/public/exams/attestations/verify/{token}", Verify).WithTags("Exams - Public verification");
        return app;
    }

    private sealed record IssueRequest(string Type,string Reference,string TemplateCode,int TemplateVersion,Guid DocumentId,string DocumentSha256,string? PublicVerificationToken,DateTimeOffset? ExpiresAtUtc,Guid? SupersedesAttestationId,Guid OperationId);
    private sealed record CorrectRequest(string TemplateCode,int TemplateVersion,Guid DocumentId,string DocumentSha256,string? PublicVerificationToken);
    private sealed record SignRequest(string SignatureProcessReference,string SignatureEvidenceHash);
    private sealed record DeliverRequest(string DeliveryChannel);
    private sealed record RevokeRequest(string ReasonCode,string? Notes);

    private static async Task<IResult> Issue(Guid resultId,IssueRequest r,IMediator m,ICurrentTenant t,ICurrentUser u,CancellationToken ct){if(t.OrganizationId is not{} org||u.UserId is not{} actor)return Results.Unauthorized();var x=await m.Send(new IssueExamAttestationCommand(org,new ExamResultId(resultId),r.Type,r.Reference,r.TemplateCode,r.TemplateVersion,new DocumentId(r.DocumentId),r.DocumentSha256,r.PublicVerificationToken,r.ExpiresAtUtc,r.SupersedesAttestationId.HasValue?new ExamAttestationId(r.SupersedesAttestationId.Value):null,r.OperationId,actor),ct);return x.IsSuccess?Results.Ok(x.Value):Failure(x.Error);}
    private static async Task<IResult> Correct(Guid attestationId,CorrectRequest r,IMediator m,ICurrentTenant t,ICurrentUser u,CancellationToken ct){if(t.OrganizationId is not{} org||u.UserId is not{} actor)return Results.Unauthorized();var x=await m.Send(new CorrectExamAttestationDocumentCommand(org,new ExamAttestationId(attestationId),r.TemplateCode,r.TemplateVersion,new DocumentId(r.DocumentId),r.DocumentSha256,r.PublicVerificationToken,actor),ct);return x.IsSuccess?Results.Ok(x.Value):Failure(x.Error);}
    private static async Task<IResult> Sign(Guid attestationId,SignRequest r,IMediator m,ICurrentTenant t,ICurrentUser u,CancellationToken ct){if(t.OrganizationId is not{} org||u.UserId is not{} actor)return Results.Unauthorized();var x=await m.Send(new SignExamAttestationCommand(org,new ExamAttestationId(attestationId),r.SignatureProcessReference,r.SignatureEvidenceHash,actor),ct);return x.IsSuccess?Results.Ok(x.Value):Failure(x.Error);}
    private static async Task<IResult> Deliver(Guid attestationId,DeliverRequest r,IMediator m,ICurrentTenant t,ICurrentUser u,CancellationToken ct){if(t.OrganizationId is not{} org||u.UserId is not{} actor)return Results.Unauthorized();var x=await m.Send(new DeliverExamAttestationCommand(org,new ExamAttestationId(attestationId),r.DeliveryChannel,actor),ct);return x.IsSuccess?Results.Ok(x.Value):Failure(x.Error);}
    private static async Task<IResult> Revoke(Guid attestationId,RevokeRequest r,IMediator m,ICurrentTenant t,ICurrentUser u,CancellationToken ct){if(t.OrganizationId is not{} org||u.UserId is not{} actor)return Results.Unauthorized();var x=await m.Send(new RevokeExamAttestationCommand(org,new ExamAttestationId(attestationId),r.ReasonCode,r.Notes,actor),ct);return x.IsSuccess?Results.Ok(x.Value):Failure(x.Error);}
    private static async Task<IResult> Get(Guid attestationId,IMediator m,ICurrentTenant t,CancellationToken ct){if(t.OrganizationId is not{} org)return Results.Unauthorized();var x=await m.Send(new GetExamAttestationQuery(org,new ExamAttestationId(attestationId)),ct);return x.IsSuccess?Results.Ok(x.Value):Failure(x.Error);}
    private static async Task<IResult> ListByResult(Guid resultId,IMediator m,ICurrentTenant t,CancellationToken ct){if(t.OrganizationId is not{} org)return Results.Unauthorized();var x=await m.Send(new GetExamResultAttestationsQuery(org,new ExamResultId(resultId)),ct);return x.IsSuccess?Results.Ok(x.Value):Failure(x.Error);}
    private static async Task<IResult> ListByStudent(Guid studentId,IMediator m,ICurrentTenant t,CancellationToken ct){if(t.OrganizationId is not{} org)return Results.Unauthorized();var x=await m.Send(new GetStudentExamAttestationsQuery(org,new PersonId(studentId)),ct);return x.IsSuccess?Results.Ok(x.Value):Failure(x.Error);}
    private static async Task<IResult> Verify(string token,IMediator m,CancellationToken ct){var x=await m.Send(new VerifyExamAttestationQuery(token),ct);return x.IsSuccess?Results.Ok(x.Value):Results.NotFound();}
    private static IResult Failure(Error e)=>Results.Problem(statusCode:e.Type switch{ErrorType.NotFound=>404,ErrorType.Conflict=>409,ErrorType.Forbidden=>403,_=>400},title:e.Code,extensions:new Dictionary<string,object?>{{"code",e.Code},{"messageKey",e.MessageKey}});
}
