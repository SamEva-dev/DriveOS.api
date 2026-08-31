using DomainRelay.Abstractions;
using DriveOS.Application.Abstractions.Authentication;
using DriveOS.Modules.ProfessionalMarketplace.Application.Reviews;
using DriveOS.Modules.ProfessionalMarketplace.Domain.Reviews;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;
namespace DriveOS.Api.Endpoints.ProfessionalMarketplace;
internal static class ProfessionalReviewEndpoints
{
 internal static IEndpointRouteBuilder MapProfessionalReviewEndpoints(this IEndpointRouteBuilder app)
 {
  var g=app.MapGroup("/api/professional-marketplace").WithTags("Professional Marketplace - Reviews");
  g.MapPost("/organizations/{organizationId:guid}/engagements/{engagementId:guid}/reviews",Create).RequireAuthorization("ProfessionalMarketplace.Reviews.Create");
  g.MapGet("/profiles/{profileId:guid}/reviews",GetReputation).RequireAuthorization("ProfessionalMarketplace.Reviews.Read");
  g.MapPost("/profiles/{profileId:guid}/reviews/{reviewId:guid}/respond",Respond).RequireAuthorization("ProfessionalMarketplace.Reviews.Respond");
  g.MapPost("/organizations/{organizationId:guid}/reviews/{reviewId:guid}/report",Report).RequireAuthorization("ProfessionalMarketplace.Reviews.Report");
  g.MapGet("/moderation/profiles/{profileId:guid}/reviews",GetModeration).RequireAuthorization("ProfessionalMarketplace.Moderation.Read");
  g.MapPost("/moderation/reviews/{reviewId:guid}/hide",Hide).RequireAuthorization("ProfessionalMarketplace.Moderation.HideContent");
  g.MapPost("/moderation/reviews/{reviewId:guid}/restore",Restore).RequireAuthorization("ProfessionalMarketplace.Moderation.Manage");
  g.MapPost("/moderation/review-reports/{reportId:guid}/resolve",ResolveReport).RequireAuthorization("ProfessionalMarketplace.Moderation.ResolveReport");
  return app;
 }
 private static async Task<IResult> Create(Guid organizationId,Guid engagementId,CreateReviewRequest q,IMediator m,ICurrentUser u,CancellationToken ct){if(u.UserId is not{} actor)return Results.Unauthorized();var id=new ProfessionalReviewId(Guid.NewGuid());var r=await m.Send(new CreateProfessionalReviewCommand(id,new(organizationId),new(engagementId),new(q.Overall,q.Reliability,q.Pedagogy,q.Communication,q.Punctuality),q.Comment,actor),ct);return r.IsSuccess?Results.Created($"/api/professional-marketplace/reviews/{id.Value}",new{id=id.Value}):Problem(r.Error);}
 private static async Task<IResult> GetReputation(Guid profileId,IMediator m,CancellationToken ct){var r=await m.Send(new GetProfessionalReputationQuery(new(profileId)),ct);return r.IsSuccess?Results.Ok(r.Value):Problem(r.Error);}
 private static async Task<IResult> Respond(Guid profileId,Guid reviewId,ReviewResponseRequest q,IMediator m,ICurrentUser u,CancellationToken ct){if(u.UserId is not{} actor)return Results.Unauthorized();var r=await m.Send(new RespondProfessionalReviewCommand(new(reviewId),new(profileId),q.Response,actor),ct);return r.IsSuccess?Results.NoContent():Problem(r.Error);}
 private static async Task<IResult> Report(Guid organizationId,Guid reviewId,ReportReviewRequest q,IMediator m,ICurrentUser u,CancellationToken ct){if(u.UserId is not{} actor)return Results.Unauthorized();var id=new ProfessionalReviewReportId(Guid.NewGuid());var r=await m.Send(new ReportProfessionalReviewCommand(id,new(reviewId),new(organizationId),q.ReasonCode,q.Details,actor),ct);return r.IsSuccess?Results.Created($"/api/professional-marketplace/review-reports/{id.Value}",new{id=id.Value}):Problem(r.Error);}
 private static async Task<IResult> GetModeration(Guid profileId,IMediator m,CancellationToken ct){var r=await m.Send(new GetProfessionalReviewModerationQuery(new(profileId)),ct);return r.IsSuccess?Results.Ok(r.Value):Problem(r.Error);}
 private static async Task<IResult> Hide(Guid reviewId,ModerateReviewRequest q,IMediator m,ICurrentUser u,CancellationToken ct){if(u.UserId is not{} actor)return Results.Unauthorized();var r=await m.Send(new HideProfessionalReviewCommand(new(reviewId),q.Reason,actor),ct);return r.IsSuccess?Results.NoContent():Problem(r.Error);}
 private static async Task<IResult> Restore(Guid reviewId,IMediator m,ICurrentUser u,CancellationToken ct){if(u.UserId is not{} actor)return Results.Unauthorized();var r=await m.Send(new RestoreProfessionalReviewCommand(new(reviewId),actor),ct);return r.IsSuccess?Results.NoContent():Problem(r.Error);}
 private static async Task<IResult> ResolveReport(Guid reportId,ResolveReviewReportRequest q,IMediator m,ICurrentUser u,CancellationToken ct){if(u.UserId is not{} actor)return Results.Unauthorized();var r=await m.Send(new ResolveProfessionalReviewReportCommand(new(reportId),q.Resolution,actor),ct);return r.IsSuccess?Results.NoContent():Problem(r.Error);}
 private static IResult Problem(Error e)=>e.Type switch{ErrorType.NotFound=>Results.NotFound(new{code=e.Code,messageKey=e.Message}),ErrorType.Conflict=>Results.Conflict(new{code=e.Code,messageKey=e.Message}),ErrorType.Forbidden=>Results.Json(new{code=e.Code,messageKey=e.Message},statusCode:403),_=>Results.BadRequest(new{code=e.Code,messageKey=e.Message})};
}
internal sealed record CreateReviewRequest(int Overall,int Reliability,int Pedagogy,int Communication,int Punctuality,string? Comment);
internal sealed record ReviewResponseRequest(string Response);
internal sealed record ReportReviewRequest(string ReasonCode,string? Details);
internal sealed record ModerateReviewRequest(string Reason);
internal sealed record ResolveReviewReportRequest(string Resolution);
