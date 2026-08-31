using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Modules.ProfessionalMarketplace.Domain.Compliance;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.ProfessionalMarketplace.Application.Compliance;

public sealed record CreateComplianceCriticalityPolicyCommand(
    ProfessionalCompliancePolicyId Id,
    string CountryCode,
    string RequirementCode,
    ProfessionalComplianceCriticality Criticality,
    ProfessionalComplianceEnforcementAction Action,
    int GracePeriodDays,
    DateOnly EffectiveFrom,
    DateOnly? EffectiveTo,
    int Version,
    UserId ActorUserId):ICommand<ProfessionalCompliancePolicyId>;

public sealed record RetireComplianceCriticalityPolicyCommand(
    ProfessionalCompliancePolicyId Id,
    UserId ActorUserId):ICommand;

public sealed record CreateProfessionalComplianceWaiverCommand(
    ProfessionalComplianceWaiverId Id,
    ProfessionalProfileId ProfessionalProfileId,
    string RequirementCode,
    DateOnly ValidFrom,
    DateOnly ValidUntil,
    string Reason,
    UserId ActorUserId):ICommand<ProfessionalComplianceWaiverId>;

public sealed record RevokeProfessionalComplianceWaiverCommand(
    ProfessionalComplianceWaiverId Id,
    ProfessionalProfileId ProfessionalProfileId,
    string Reason,
    UserId ActorUserId):ICommand;

public sealed record GetComplianceCriticalityPoliciesQuery(
    string? CountryCode):IQuery<IReadOnlyList<ComplianceCriticalityPolicyResponse>>;

public sealed record GetProfessionalComplianceWaiversQuery(
    ProfessionalProfileId ProfessionalProfileId):IQuery<IReadOnlyList<ProfessionalComplianceWaiverResponse>>;

public sealed record ComplianceCriticalityPolicyResponse(
    Guid Id,string CountryCode,string RequirementCode,string Criticality,string Action,
    int GracePeriodDays,DateOnly EffectiveFrom,DateOnly? EffectiveTo,int Version,string Status);

public sealed record ProfessionalComplianceWaiverResponse(
    Guid Id,Guid ProfessionalProfileId,string RequirementCode,string CountryCode,
    DateOnly ValidFrom,DateOnly ValidUntil,string Reason,string Status,Guid ApprovedByUserId);

public sealed record ProfessionalComplianceOperationalRequest(
    UserId ProfessionalUserId,
    OrganizationId[] OrganizationIds,
    bool BlockNewSessions,
    string Reason);

public interface IProfessionalComplianceOperationalGateway
{
    Task ApplyAsync(
        ProfessionalComplianceOperationalRequest request,
        CancellationToken cancellationToken=default);
}
