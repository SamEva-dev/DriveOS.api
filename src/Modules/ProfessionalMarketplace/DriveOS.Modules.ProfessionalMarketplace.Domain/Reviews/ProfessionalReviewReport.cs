using DriveOS.SharedKernel.Auditing;
using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.ProfessionalMarketplace.Domain.Reviews;

public sealed class ProfessionalReviewReport:AggregateRoot<ProfessionalReviewReportId>,IAuditableEntity
{
    private ProfessionalReviewReport(){}
    private ProfessionalReviewReport(ProfessionalReviewReportId id,ProfessionalReviewId reviewId,OrganizationId organizationId,UserId reporter,string reasonCode,string? details):base(id)
    {ReviewId=reviewId;OrganizationId=organizationId;ReportedByUserId=reporter;ReasonCode=Token(reasonCode);Details=Text(details,1000);Status=ProfessionalReviewReportStatus.Open;}
    public ProfessionalReviewId ReviewId{get;private set;}
    public OrganizationId OrganizationId{get;private set;}
    public UserId ReportedByUserId{get;private set;}
    public string ReasonCode{get;private set;}=string.Empty;
    public string? Details{get;private set;}
    public ProfessionalReviewReportStatus Status{get;private set;}
    public string? Resolution{get;private set;}
    public DateTimeOffset? ResolvedAtUtc{get;private set;}
    public UserId? ResolvedByUserId{get;private set;}
    public DateTimeOffset CreatedAtUtc{get;private set;}
    public UserId? CreatedByUserId{get;private set;}
    public DateTimeOffset? LastModifiedAtUtc{get;private set;}
    public UserId? LastModifiedByUserId{get;private set;}
    public static Result<ProfessionalReviewReport> Create(ProfessionalReviewReportId id,ProfessionalReviewId reviewId,OrganizationId org,UserId reporter,string reasonCode,string? details,DateTimeOffset now)
    {if(id.IsEmpty||reviewId.IsEmpty||org.IsEmpty||reporter.IsEmpty||string.IsNullOrWhiteSpace(reasonCode))return Result.Failure<ProfessionalReviewReport>(ProfessionalReviewErrors.InvalidReport);var x=new ProfessionalReviewReport(id,reviewId,org,reporter,reasonCode,details);x.SetCreatedAudit(now,reporter);return Result.Success(x);}
    public Result Resolve(string resolution,DateTimeOffset now,UserId actor){if(Status==ProfessionalReviewReportStatus.Resolved)return Result.Failure(ProfessionalReviewErrors.InvalidTransition);resolution=(resolution??string.Empty).Trim();if(resolution.Length is <2 or >1000)return Result.Failure(ProfessionalReviewErrors.InvalidReportResolution);Status=ProfessionalReviewReportStatus.Resolved;Resolution=resolution;ResolvedAtUtc=now.ToUniversalTime();ResolvedByUserId=actor;SetModifiedAudit(now,actor);return Result.Success();}
    public void SetCreatedAudit(DateTimeOffset at,UserId? actor){CreatedAtUtc=at.ToUniversalTime();CreatedByUserId=actor;}public void SetModifiedAudit(DateTimeOffset at,UserId? actor){LastModifiedAtUtc=at.ToUniversalTime();LastModifiedByUserId=actor;}
    private static string Token(string v)=>v.Trim().ToUpperInvariant();private static string? Text(string? v,int max)=>string.IsNullOrWhiteSpace(v)?null:v.Trim()[..Math.Min(v.Trim().Length,max)];
}
public enum ProfessionalReviewReportStatus{Open=1,Resolved=2}
