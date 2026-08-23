using DriveOS.SharedKernel.Auditing;
using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;
namespace DriveOS.Modules.Workforce.Domain.PerformanceReviews;

public enum PerformanceReviewStatus { Draft=1, InProgress=2, Submitted=3, Acknowledged=4, Completed=5, Cancelled=6 }

/// <summary>
/// Durable HR evaluation of an employee for a bounded review period. Criteria are snapshotted in this aggregate
/// so later changes to evaluation templates cannot rewrite historical reviews. This aggregate evaluates employees,
/// never learners: pedagogical learner assessments remain owned by Curriculum/Training Delivery.
/// </summary>
public sealed class PerformanceReview : AggregateRoot<PerformanceReviewId>, IAuditableEntity
{
    private readonly List<PerformanceReviewCriterion> _criteria=[];
    private PerformanceReview(){}
    private PerformanceReview(PerformanceReviewId id,OrganizationId org,EmployeeId employee,UserId evaluator,DateOnly from,DateOnly to,string title):base(id){OrganizationId=org;EmployeeId=employee;EvaluatorUserId=evaluator;PeriodFrom=from;PeriodTo=to;Title=title;Status=PerformanceReviewStatus.Draft;}
    public OrganizationId OrganizationId{get;private set;}
    public EmployeeId EmployeeId{get;private set;}
    public UserId EvaluatorUserId{get;private set;}
    public DateOnly PeriodFrom{get;private set;}
    public DateOnly PeriodTo{get;private set;}
    public string Title{get;private set;}=string.Empty;
    public PerformanceReviewStatus Status{get;private set;}
    public string? OverallAssessment{get;private set;}
    public string? Objectives{get;private set;}
    public DateTimeOffset? SubmittedAtUtc{get;private set;}
    public UserId? SubmittedByUserId{get;private set;}
    public DateTimeOffset? AcknowledgedAtUtc{get;private set;}
    public UserId? AcknowledgedByUserId{get;private set;}
    public string? EmployeeComment{get;private set;}
    public DateTimeOffset? CompletedAtUtc{get;private set;}
    public UserId? CompletedByUserId{get;private set;}
    public string? CancellationReason{get;private set;}
    public IReadOnlyCollection<PerformanceReviewCriterion> Criteria=>_criteria;
    public DateTimeOffset CreatedAtUtc{get;private set;}
    public UserId? CreatedByUserId{get;private set;}
    public DateTimeOffset? LastModifiedAtUtc{get;private set;}
    public UserId? LastModifiedByUserId{get;private set;}
    public static Result<PerformanceReview> Create(PerformanceReviewId id,OrganizationId org,EmployeeId employee,UserId evaluator,DateOnly from,DateOnly to,string title,DateTimeOffset now,UserId actor){if(id.IsEmpty||org.IsEmpty||employee.IsEmpty||evaluator.IsEmpty)return Result.Failure<PerformanceReview>(PerformanceReviewErrors.InvalidIdentifier);if(to<from)return Result.Failure<PerformanceReview>(PerformanceReviewErrors.InvalidPeriod);title=(title??string.Empty).Trim();if(title.Length is <1 or >160)return Result.Failure<PerformanceReview>(PerformanceReviewErrors.InvalidText);var x=new PerformanceReview(id,org,employee,evaluator,from,to,title);x.SetCreatedAudit(now,actor);return Result.Success(x);}
    public Result Start(DateTimeOffset now,UserId actor){if(Status!=PerformanceReviewStatus.Draft)return Result.Failure(PerformanceReviewErrors.InvalidTransition);Status=PerformanceReviewStatus.InProgress;SetModifiedAudit(now,actor);return Result.Success();}
    public Result AddCriterion(PerformanceReviewCriterionId id,string code,string label,int weight,string? comment,DateTimeOffset now,UserId actor){if(Status is not (PerformanceReviewStatus.Draft or PerformanceReviewStatus.InProgress))return Result.Failure(PerformanceReviewErrors.NotEditable);var r=PerformanceReviewCriterion.Create(id,code,label,weight,comment);if(r.IsFailure)return Result.Failure(r.Error);if(_criteria.Any(x=>x.Code.Equals(r.Value.Code,StringComparison.OrdinalIgnoreCase)))return Result.Failure(PerformanceReviewErrors.DuplicateCriterion);_criteria.Add(r.Value);SetModifiedAudit(now,actor);return Result.Success();}
    public Result RateCriterion(PerformanceReviewCriterionId id,int rating,string? comment,DateTimeOffset now,UserId actor){if(Status is not (PerformanceReviewStatus.Draft or PerformanceReviewStatus.InProgress))return Result.Failure(PerformanceReviewErrors.NotEditable);var c=_criteria.SingleOrDefault(x=>x.Id==id);if(c is null)return Result.Failure(PerformanceReviewErrors.CriterionNotFound);var r=c.Rate(rating,comment);if(r.IsFailure)return r;SetModifiedAudit(now,actor);return Result.Success();}
    public Result SetSummary(string overallAssessment,string? objectives,DateTimeOffset now,UserId actor){if(Status is not (PerformanceReviewStatus.Draft or PerformanceReviewStatus.InProgress))return Result.Failure(PerformanceReviewErrors.NotEditable);if(string.IsNullOrWhiteSpace(overallAssessment)||overallAssessment.Trim().Length>4000||(objectives?.Trim().Length??0)>4000)return Result.Failure(PerformanceReviewErrors.InvalidText);OverallAssessment=overallAssessment.Trim();Objectives=string.IsNullOrWhiteSpace(objectives)?null:objectives.Trim();SetModifiedAudit(now,actor);return Result.Success();}
    public Result Submit(DateTimeOffset now,UserId actor){if(Status is not (PerformanceReviewStatus.Draft or PerformanceReviewStatus.InProgress))return Result.Failure(PerformanceReviewErrors.InvalidTransition);if(_criteria.Count==0||_criteria.Any(x=>x.Rating is null)||string.IsNullOrWhiteSpace(OverallAssessment))return Result.Failure(PerformanceReviewErrors.IncompleteReview);Status=PerformanceReviewStatus.Submitted;SubmittedAtUtc=now.ToUniversalTime();SubmittedByUserId=actor;SetModifiedAudit(now,actor);return Result.Success();}
    public Result Acknowledge(string? employeeComment,DateTimeOffset now,UserId actor){if(Status!=PerformanceReviewStatus.Submitted)return Result.Failure(PerformanceReviewErrors.InvalidTransition);if((employeeComment?.Trim().Length??0)>4000)return Result.Failure(PerformanceReviewErrors.InvalidText);Status=PerformanceReviewStatus.Acknowledged;AcknowledgedAtUtc=now.ToUniversalTime();AcknowledgedByUserId=actor;EmployeeComment=string.IsNullOrWhiteSpace(employeeComment)?null:employeeComment.Trim();SetModifiedAudit(now,actor);return Result.Success();}
    public Result Complete(DateTimeOffset now,UserId actor){if(Status!=PerformanceReviewStatus.Acknowledged)return Result.Failure(PerformanceReviewErrors.InvalidTransition);Status=PerformanceReviewStatus.Completed;CompletedAtUtc=now.ToUniversalTime();CompletedByUserId=actor;SetModifiedAudit(now,actor);return Result.Success();}
    public Result Cancel(string reason,DateTimeOffset now,UserId actor){if(Status is PerformanceReviewStatus.Completed or PerformanceReviewStatus.Cancelled)return Result.Failure(PerformanceReviewErrors.InvalidTransition);if(string.IsNullOrWhiteSpace(reason)||reason.Trim().Length>512)return Result.Failure(PerformanceReviewErrors.CancellationReasonRequired);Status=PerformanceReviewStatus.Cancelled;CancellationReason=reason.Trim();SetModifiedAudit(now,actor);return Result.Success();}
    public void SetCreatedAudit(DateTimeOffset at,UserId? actor){CreatedAtUtc=at.ToUniversalTime();CreatedByUserId=actor;}
    public void SetModifiedAudit(DateTimeOffset at,UserId? actor){LastModifiedAtUtc=at.ToUniversalTime();LastModifiedByUserId=actor;}
}
public sealed class PerformanceReviewCriterion
{
    private PerformanceReviewCriterion(){}
    private PerformanceReviewCriterion(PerformanceReviewCriterionId id,string code,string label,int weight,string? comment){Id=id;Code=code;Label=label;Weight=weight;Comment=Normalize(comment,2000);}
    public PerformanceReviewCriterionId Id{get;private set;}
    public string Code{get;private set;}=string.Empty;
    public string Label{get;private set;}=string.Empty;
    public int Weight{get;private set;}
    public int? Rating{get;private set;}
    public string? Comment{get;private set;}
    public static Result<PerformanceReviewCriterion> Create(PerformanceReviewCriterionId id,string code,string label,int weight,string? comment){code=(code??string.Empty).Trim().ToUpperInvariant();label=(label??string.Empty).Trim();if(id.IsEmpty)return Result.Failure<PerformanceReviewCriterion>(PerformanceReviewErrors.InvalidIdentifier);if(code.Length is <1 or >64||label.Length is <1 or >256||(comment?.Trim().Length??0)>2000)return Result.Failure<PerformanceReviewCriterion>(PerformanceReviewErrors.InvalidText);if(weight is <1 or >100)return Result.Failure<PerformanceReviewCriterion>(PerformanceReviewErrors.InvalidWeight);return Result.Success(new PerformanceReviewCriterion(id,code,label,weight,comment));}
    internal Result Rate(int rating,string? comment){if(rating is <1 or >5)return Result.Failure(PerformanceReviewErrors.InvalidRating);if((comment?.Trim().Length??0)>2000)return Result.Failure(PerformanceReviewErrors.InvalidText);Rating=rating;Comment=Normalize(comment,2000);return Result.Success();}
    private static string? Normalize(string? v,int max)=>string.IsNullOrWhiteSpace(v)?null:v.Trim()[..Math.Min(v.Trim().Length,max)];
}
public static class PerformanceReviewErrors
{
    public static readonly Error InvalidIdentifier=Error.Validation("Workforce.PerformanceReview.InvalidIdentifier","errors.workforce.performanceReview.invalidIdentifier");
    public static readonly Error InvalidPeriod=Error.Validation("Workforce.PerformanceReview.InvalidPeriod","errors.workforce.performanceReview.invalidPeriod");
    public static readonly Error InvalidText=Error.Validation("Workforce.PerformanceReview.InvalidText","errors.workforce.performanceReview.invalidText");
    public static readonly Error InvalidWeight=Error.Validation("Workforce.PerformanceReview.InvalidWeight","errors.workforce.performanceReview.invalidWeight");
    public static readonly Error InvalidRating=Error.Validation("Workforce.PerformanceReview.InvalidRating","errors.workforce.performanceReview.invalidRating");
    public static readonly Error DuplicateCriterion=Error.Conflict("Workforce.PerformanceReview.DuplicateCriterion","errors.workforce.performanceReview.duplicateCriterion");
    public static readonly Error CriterionNotFound=Error.NotFound("Workforce.PerformanceReview.CriterionNotFound","errors.workforce.performanceReview.criterionNotFound");
    public static readonly Error NotEditable=Error.Conflict("Workforce.PerformanceReview.NotEditable","errors.workforce.performanceReview.notEditable");
    public static readonly Error InvalidTransition=Error.Conflict("Workforce.PerformanceReview.InvalidTransition","errors.workforce.performanceReview.invalidTransition");
    public static readonly Error IncompleteReview=Error.Validation("Workforce.PerformanceReview.Incomplete","errors.workforce.performanceReview.incomplete");
    public static readonly Error CancellationReasonRequired=Error.Validation("Workforce.PerformanceReview.CancellationReasonRequired","errors.workforce.performanceReview.cancellationReasonRequired");
    public static readonly Error OverlappingPeriod=Error.Conflict("Workforce.PerformanceReview.OverlappingPeriod","errors.workforce.performanceReview.overlappingPeriod");
    public static readonly Error NotFound=Error.NotFound("Workforce.PerformanceReview.NotFound","errors.workforce.performanceReview.notFound");
}
public interface IPerformanceReviewRepository
{
    Task<PerformanceReview?> GetAsync(OrganizationId organizationId,PerformanceReviewId id,bool tracking,CancellationToken ct=default);
    Task<IReadOnlyList<PerformanceReview>> ListAsync(OrganizationId organizationId,EmployeeId? employeeId,PerformanceReviewStatus? status,CancellationToken ct=default);
    Task<bool> HasOverlapAsync(OrganizationId organizationId,EmployeeId employeeId,DateOnly from,DateOnly to,PerformanceReviewId? excluding,CancellationToken ct=default);
    void Add(PerformanceReview review);
}
