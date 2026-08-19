using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.CurriculumPedagogy.Domain.PedagogicalReviews;

public static class PedagogicalReviewErrors
{
    public static readonly Error InvalidIdentifier = Error.Validation("CurriculumPedagogy.PedagogicalReview.Id.Invalid", "errors.curriculumPedagogy.pedagogicalReview.id.invalid");
    public static readonly Error InvalidOrganization = Error.Validation("CurriculumPedagogy.PedagogicalReview.Organization.Invalid", "errors.curriculumPedagogy.pedagogicalReview.organization.invalid");
    public static readonly Error InvalidStudent = Error.Validation("CurriculumPedagogy.PedagogicalReview.Student.Invalid", "errors.curriculumPedagogy.pedagogicalReview.student.invalid");
    public static readonly Error InvalidTrainingPath = Error.Validation("CurriculumPedagogy.PedagogicalReview.TrainingPath.Invalid", "errors.curriculumPedagogy.pedagogicalReview.trainingPath.invalid");
    public static readonly Error InvalidReviewer = Error.Validation("CurriculumPedagogy.PedagogicalReview.Reviewer.Invalid", "errors.curriculumPedagogy.pedagogicalReview.reviewer.invalid");
    public static readonly Error InvalidReason = Error.Validation("CurriculumPedagogy.PedagogicalReview.Reason.Invalid", "errors.curriculumPedagogy.pedagogicalReview.reason.invalid");
    public static readonly Error StartNotAllowed = Error.Conflict("CurriculumPedagogy.PedagogicalReview.Start.NotAllowed", "errors.curriculumPedagogy.pedagogicalReview.start.notAllowed");
    public static readonly Error CompletionNotAllowed = Error.Conflict("CurriculumPedagogy.PedagogicalReview.Complete.NotAllowed", "errors.curriculumPedagogy.pedagogicalReview.complete.notAllowed");
    public static readonly Error InvalidFindings = Error.Validation("CurriculumPedagogy.PedagogicalReview.Findings.Invalid", "errors.curriculumPedagogy.pedagogicalReview.findings.invalid");
    public static readonly Error InvalidRecommendations = Error.Validation("CurriculumPedagogy.PedagogicalReview.Recommendations.Invalid", "errors.curriculumPedagogy.pedagogicalReview.recommendations.invalid");
    public static readonly Error InvalidEstimatedRemainingNeeds = Error.Validation("CurriculumPedagogy.PedagogicalReview.EstimatedRemainingNeeds.Invalid", "errors.curriculumPedagogy.pedagogicalReview.estimatedRemainingNeeds.invalid");
    public static readonly Error CancellationNotAllowed = Error.Conflict("CurriculumPedagogy.PedagogicalReview.Cancel.NotAllowed", "errors.curriculumPedagogy.pedagogicalReview.cancel.notAllowed");
}
