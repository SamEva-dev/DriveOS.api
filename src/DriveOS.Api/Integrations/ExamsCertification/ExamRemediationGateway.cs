using DomainRelay.Abstractions;
using DriveOS.Modules.CurriculumPedagogy.Application.RemediationPlans;
using DriveOS.Modules.ExamsCertification.Application.Remediation;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Api.Integrations.ExamsCertification;

internal sealed class ExamRemediationGateway(IMediator mediator, IRemediationPlanReadService remediationPlans) : IExamRemediationGateway
{
    public async Task<ExamRemediationProvisionResult> ProvisionAsync(ExamRemediationProvisionRequest request, CancellationToken ct = default)
    {
        var targets = request.CompetencyIds.Distinct().Select(id => new RemediationTargetRequest(new CompetencyId(id),
            $"Rework competency after failed exam result {request.ResultId.Value:N} revision {request.ResultRevision}.")).ToArray();
        if (targets.Length == 0)
            return new ExamRemediationProvisionResult(false, true, null, "exams.remediation.no-target-competency");

        var result = await mediator.Send(new CreateRemediationPlanCommand(request.OrganizationId, request.TrainingPathId,
            request.ResponsibleUserId, null, request.Recommendation, request.RecommendedPracticalHours, request.RecommendedSessions,
            request.ReviewDate, targets), ct);
        if (result.IsSuccess) return new ExamRemediationProvisionResult(true, false, result.Value);
        if (result.Error.Code is "CurriculumPedagogy.RemediationPlan.OpenPlan.Exists")
            return new ExamRemediationProvisionResult(false, true, null, "exams.remediation.open-pedagogical-plan-exists", result.Error.MessageKey);
        return new ExamRemediationProvisionResult(false, false, null, result.Error.Code, result.Error.MessageKey);
    }

    public async Task<ExamRemediationPedagogicalStatus?> GetStatusAsync(OrganizationId organizationId, RemediationPlanId planId, CancellationToken ct = default)
    {
        RemediationPlanResponse? plan = await remediationPlans.GetAsync(organizationId, planId, ct);
        return plan is null ? null : new ExamRemediationPedagogicalStatus(plan.Status);
    }
}
