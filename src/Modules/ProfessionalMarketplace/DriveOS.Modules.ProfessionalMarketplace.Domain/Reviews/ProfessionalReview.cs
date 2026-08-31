using DriveOS.Modules.ProfessionalMarketplace.Domain.Engagements;
using DriveOS.SharedKernel.Auditing;
using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.ProfessionalMarketplace.Domain.Reviews;

/// <summary>
/// Verified organization review backed by a real ProfessionalEngagement.
/// Reviews are never anonymous: author, tenant and source engagement remain auditable.
/// Moderation hides content without deleting the review record or its source evidence.
/// </summary>
public sealed class ProfessionalReview:AggregateRoot<ProfessionalReviewId>,IAuditableEntity
{
    private ProfessionalReview(){}
    private ProfessionalReview(ProfessionalReviewId id,ProfessionalEngagement engagement,UserId author,ProfessionalReviewRatings ratings,string? comment):base(id)
    {
        OrganizationId=engagement.OrganizationId;
        ProfessionalProfileId=engagement.ProfessionalProfileId;
        EngagementId=engagement.Id;
        AuthorUserId=author;
        Ratings=ratings;
        Comment=NormalizeText(comment,2000);
        Status=ProfessionalReviewStatus.Published;
    }

    public OrganizationId OrganizationId{get;private set;}
    public ProfessionalProfileId ProfessionalProfileId{get;private set;}
    public ProfessionalEngagementId EngagementId{get;private set;}
    public UserId AuthorUserId{get;private set;}
    public ProfessionalReviewRatings Ratings{get;private set;}=default!;
    public string? Comment{get;private set;}
    public ProfessionalReviewStatus Status{get;private set;}
    public string? ProfessionalResponse{get;private set;}
    public DateTimeOffset? RespondedAtUtc{get;private set;}
    public UserId? RespondedByUserId{get;private set;}
    public DateTimeOffset? HiddenAtUtc{get;private set;}
    public UserId? HiddenByUserId{get;private set;}
    public string? ModerationReason{get;private set;}
    public DateTimeOffset CreatedAtUtc{get;private set;}
    public UserId? CreatedByUserId{get;private set;}
    public DateTimeOffset? LastModifiedAtUtc{get;private set;}
    public UserId? LastModifiedByUserId{get;private set;}

    public decimal OverallScore=>Ratings.Average;
    public bool CountsTowardReputation=>Status==ProfessionalReviewStatus.Published;

    public static Result<ProfessionalReview> Create(ProfessionalReviewId id,ProfessionalEngagement engagement,UserId author,ProfessionalReviewRatings ratings,string? comment,DateTimeOffset now)
    {
        if(id.IsEmpty||engagement.Id.IsEmpty||author.IsEmpty)
            return Result.Failure<ProfessionalReview>(ProfessionalReviewErrors.InvalidIdentifier);
        if(engagement.ActivatedAtUtc is null||engagement.Status is not ProfessionalEngagementStatus.Ended and not ProfessionalEngagementStatus.Terminated)
            return Result.Failure<ProfessionalReview>(ProfessionalReviewErrors.CompletedCollaborationRequired);
        Result validation=ratings.Validate();if(validation.IsFailure)return Result.Failure<ProfessionalReview>(validation.Error);
        if(comment?.Trim().Length>2000)return Result.Failure<ProfessionalReview>(ProfessionalReviewErrors.InvalidComment);
        var x=new ProfessionalReview(id,engagement,author,ratings,comment);x.SetCreatedAudit(now,author);return Result.Success(x);
    }

    public Result Respond(string response,DateTimeOffset now,UserId actor)
    {
        if(Status!=ProfessionalReviewStatus.Published)return Result.Failure(ProfessionalReviewErrors.ReviewNotRespondable);
        response=(response??string.Empty).Trim();if(response.Length is <2 or >2000)return Result.Failure(ProfessionalReviewErrors.InvalidResponse);
        ProfessionalResponse=response;RespondedAtUtc=now.ToUniversalTime();RespondedByUserId=actor;SetModifiedAudit(now,actor);return Result.Success();
    }

    public Result Hide(string reason,DateTimeOffset now,UserId actor)
    {
        if(Status==ProfessionalReviewStatus.Hidden)return Result.Failure(ProfessionalReviewErrors.InvalidTransition);
        reason=(reason??string.Empty).Trim();if(reason.Length is <2 or >512)return Result.Failure(ProfessionalReviewErrors.ModerationReasonRequired);
        Status=ProfessionalReviewStatus.Hidden;HiddenAtUtc=now.ToUniversalTime();HiddenByUserId=actor;ModerationReason=reason;SetModifiedAudit(now,actor);return Result.Success();
    }

    public Result Restore(DateTimeOffset now,UserId actor)
    {
        if(Status!=ProfessionalReviewStatus.Hidden)return Result.Failure(ProfessionalReviewErrors.InvalidTransition);
        Status=ProfessionalReviewStatus.Published;HiddenAtUtc=null;HiddenByUserId=null;ModerationReason=null;SetModifiedAudit(now,actor);return Result.Success();
    }

    public void SetCreatedAudit(DateTimeOffset at,UserId? actor){CreatedAtUtc=at.ToUniversalTime();CreatedByUserId=actor;}
    public void SetModifiedAudit(DateTimeOffset at,UserId? actor){LastModifiedAtUtc=at.ToUniversalTime();LastModifiedByUserId=actor;}
    private static string? NormalizeText(string? value,int max){if(string.IsNullOrWhiteSpace(value))return null;string x=value.Trim();return x[..Math.Min(x.Length,max)];}
}

public sealed record ProfessionalReviewRatings(int Overall,int Reliability,int Pedagogy,int Communication,int Punctuality)
{
    public decimal Average=>decimal.Round((Overall+Reliability+Pedagogy+Communication+Punctuality)/5m,2);
    public Result Validate()=>new[]{Overall,Reliability,Pedagogy,Communication,Punctuality}.Any(x=>x is <1 or >5)
        ?Result.Failure(ProfessionalReviewErrors.InvalidRatings):Result.Success();
}

public enum ProfessionalReviewStatus{Published=1,Hidden=2}
