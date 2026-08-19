using DriveOS.SharedKernel.Results;
namespace DriveOS.Modules.CurriculumPedagogy.Domain.RemediationPlans;
public static class RemediationPlanErrors
{
 public static readonly Error InvalidIdentifier=Error.Validation("CurriculumPedagogy.RemediationPlan.InvalidIdentifier","errors.curriculumPedagogy.remediationPlan.invalidIdentifier");
 public static readonly Error InvalidContext=Error.Validation("CurriculumPedagogy.RemediationPlan.InvalidContext","errors.curriculumPedagogy.remediationPlan.invalidContext");
 public static readonly Error InvalidRecommendation=Error.Validation("CurriculumPedagogy.RemediationPlan.InvalidRecommendation","errors.curriculumPedagogy.remediationPlan.invalidRecommendation");
 public static readonly Error InvalidHours=Error.Validation("CurriculumPedagogy.RemediationPlan.InvalidHours","errors.curriculumPedagogy.remediationPlan.invalidHours");
 public static readonly Error InvalidSessions=Error.Validation("CurriculumPedagogy.RemediationPlan.InvalidSessions","errors.curriculumPedagogy.remediationPlan.invalidSessions");
 public static readonly Error InvalidReviewDate=Error.Validation("CurriculumPedagogy.RemediationPlan.InvalidReviewDate","errors.curriculumPedagogy.remediationPlan.invalidReviewDate");
 public static readonly Error InvalidTarget=Error.Validation("CurriculumPedagogy.RemediationPlan.InvalidTarget","errors.curriculumPedagogy.remediationPlan.invalidTarget");
 public static readonly Error DuplicateTarget=Error.Conflict("CurriculumPedagogy.RemediationPlan.DuplicateTarget","errors.curriculumPedagogy.remediationPlan.duplicateTarget");
 public static readonly Error ActivationNotAllowed=Error.Conflict("CurriculumPedagogy.RemediationPlan.ActivationNotAllowed","errors.curriculumPedagogy.remediationPlan.activationNotAllowed");
 public static readonly Error CompletionNotAllowed=Error.Conflict("CurriculumPedagogy.RemediationPlan.CompletionNotAllowed","errors.curriculumPedagogy.remediationPlan.completionNotAllowed");
 public static readonly Error CancellationNotAllowed=Error.Conflict("CurriculumPedagogy.RemediationPlan.CancellationNotAllowed","errors.curriculumPedagogy.remediationPlan.cancellationNotAllowed");
}
