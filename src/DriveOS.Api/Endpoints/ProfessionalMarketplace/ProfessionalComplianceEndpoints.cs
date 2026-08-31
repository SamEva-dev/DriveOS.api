using DomainRelay.Abstractions;
using DriveOS.Application.Abstractions.Authentication;
using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Modules.ProfessionalMarketplace.Application.Compliance;
using DriveOS.Modules.ProfessionalMarketplace.Domain.Compliance;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Api.Endpoints.ProfessionalMarketplace;

internal static class ProfessionalComplianceEndpoints
{
    internal static IEndpointRouteBuilder MapProfessionalComplianceEndpoints(this IEndpointRouteBuilder app)
    {
        var g=app.MapGroup("/api/professional-marketplace").RequireAuthorization();
        g.MapGet("/profiles/{profileId:guid}/compliance",GetCompliance).RequireAuthorization("ProfessionalMarketplace.Compliance.Read");
        g.MapPost("/profiles/{profileId:guid}/documents",RegisterDocument).RequireAuthorization("ProfessionalMarketplace.Compliance.Manage");
        g.MapPost("/documents/{documentId:guid}/submit",SubmitDocument).RequireAuthorization("ProfessionalMarketplace.Compliance.Manage");
        g.MapPost("/documents/{documentId:guid}/approve",ApproveDocument).RequireAuthorization("ProfessionalMarketplace.Compliance.Verify");
        g.MapPost("/documents/{documentId:guid}/reject",RejectDocument).RequireAuthorization("ProfessionalMarketplace.Compliance.Verify");
        g.MapPost("/profiles/{profileId:guid}/credentials",RegisterCredential).RequireAuthorization("ProfessionalMarketplace.Compliance.Manage");
        g.MapPost("/credentials/{credentialId:guid}/verify",VerifyCredential).RequireAuthorization("ProfessionalMarketplace.Compliance.Verify");
        g.MapPost("/credentials/{credentialId:guid}/reject",RejectCredential).RequireAuthorization("ProfessionalMarketplace.Compliance.Verify");
        return app;
    }
    private static async Task<IResult> GetCompliance(Guid profileId,IMediator m,CancellationToken ct){var r=await m.Send(new GetProfessionalComplianceQuery(new(profileId)),ct);return r.IsSuccess?Results.Ok(r.Value):Problem(r.Error);}
    private static async Task<IResult> RegisterDocument(Guid profileId,RegisterDocumentRequest q,IMediator m,ICurrentUser u,CancellationToken ct){if(u.UserId is not{} actor)return Results.Unauthorized();var id=new ProfessionalDocumentId(Guid.NewGuid());var r=await m.Send(new RegisterProfessionalDocumentCommand(id,new(profileId),q.DocumentReferenceId,q.DocumentTypeCode,q.CountryCode,q.Mandatory,q.IssueDate,q.ExpirationDate,actor),ct);return r.IsSuccess?Results.Created($"/api/professional-marketplace/documents/{id.Value}",new{id=id.Value}):Problem(r.Error);}
    private static async Task<IResult> SubmitDocument(Guid documentId,IMediator m,ICurrentUser u,CancellationToken ct){if(u.UserId is not{} actor)return Results.Unauthorized();var r=await m.Send(new SubmitProfessionalDocumentCommand(new(documentId),actor),ct);return r.IsSuccess?Results.NoContent():Problem(r.Error);}
    private static async Task<IResult> ApproveDocument(Guid documentId,VerificationRequest q,IMediator m,ICurrentUser u,CancellationToken ct){if(u.UserId is not{} actor)return Results.Unauthorized();var r=await m.Send(new ApproveProfessionalDocumentCommand(new(documentId),q.Method,actor),ct);return r.IsSuccess?Results.NoContent():Problem(r.Error);}
    private static async Task<IResult> RejectDocument(Guid documentId,RejectRequest q,IMediator m,ICurrentUser u,CancellationToken ct){if(u.UserId is not{} actor)return Results.Unauthorized();var r=await m.Send(new RejectProfessionalDocumentCommand(new(documentId),q.Reason,actor),ct);return r.IsSuccess?Results.NoContent():Problem(r.Error);}
    private static async Task<IResult> RegisterCredential(Guid profileId,RegisterCredentialRequest q,IMediator m,ICurrentUser u,CancellationToken ct){if(u.UserId is not{} actor)return Results.Unauthorized();var id=new ProfessionalCredentialId(Guid.NewGuid());ProfessionalDocumentId? evidence=q.EvidenceDocumentId is Guid e?new(e):null;var r=await m.Send(new RegisterProfessionalCredentialCommand(id,new(profileId),q.CredentialTypeCode,q.CountryCode,q.IssuingAuthority,q.ReferenceNumber,q.ValidFrom,q.ValidUntil,q.CategoryCodes??[],evidence,actor),ct);return r.IsSuccess?Results.Created($"/api/professional-marketplace/credentials/{id.Value}",new{id=id.Value}):Problem(r.Error);}
    private static async Task<IResult> VerifyCredential(Guid credentialId,VerificationRequest q,IMediator m,ICurrentUser u,CancellationToken ct){if(u.UserId is not{} actor)return Results.Unauthorized();var r=await m.Send(new VerifyProfessionalCredentialCommand(new(credentialId),q.Method,actor),ct);return r.IsSuccess?Results.NoContent():Problem(r.Error);}
    private static async Task<IResult> RejectCredential(Guid credentialId,RejectRequest q,IMediator m,ICurrentUser u,CancellationToken ct){if(u.UserId is not{} actor)return Results.Unauthorized();var r=await m.Send(new RejectProfessionalCredentialCommand(new(credentialId),q.Reason,actor),ct);return r.IsSuccess?Results.NoContent():Problem(r.Error);}
    private static IResult Problem(Error e)=>e.Type switch{ErrorType.NotFound=>Results.NotFound(new{code=e.Code,messageKey=e.MessageKey}),ErrorType.Conflict=>Results.Conflict(new{code=e.Code,messageKey=e.MessageKey}),_=>Results.BadRequest(new{code=e.Code,messageKey=e.MessageKey})};
}
internal sealed record RegisterDocumentRequest(Guid DocumentReferenceId,string DocumentTypeCode,string CountryCode,bool Mandatory,DateOnly? IssueDate,DateOnly? ExpirationDate);
internal sealed record RegisterCredentialRequest(string CredentialTypeCode,string CountryCode,string IssuingAuthority,string? ReferenceNumber,DateOnly ValidFrom,DateOnly? ValidUntil,string[]? CategoryCodes,Guid? EvidenceDocumentId);
internal sealed record VerificationRequest(ProfessionalVerificationMethod Method);
internal sealed record RejectRequest(string Reason);
