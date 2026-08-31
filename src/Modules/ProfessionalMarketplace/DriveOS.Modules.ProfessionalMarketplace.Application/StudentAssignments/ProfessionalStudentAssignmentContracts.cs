using DriveOS.Application.Abstractions.Messaging;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.ProfessionalMarketplace.Application.StudentAssignments;

public sealed record AssignStudentToProfessionalMissionCommand(
    ProfessionalStudentAssignmentId Id,
    OrganizationId OrganizationId,
    ProfessionalMissionId MissionId,
    PersonId StudentId,
    DateOnly StartsOn,
    DateOnly EndsOn,
    string ScopeCode,
    string AssignmentReason,
    UserId ActorUserId):ICommand<ProfessionalStudentAssignmentId>;

public sealed record RevokeProfessionalStudentAssignmentCommand(
    ProfessionalStudentAssignmentId Id,
    OrganizationId OrganizationId,
    string Reason,
    UserId ActorUserId):ICommand;

public sealed record GetProfessionalMissionStudentAssignmentsQuery(
    OrganizationId OrganizationId,
    ProfessionalMissionId MissionId):IQuery<IReadOnlyList<ProfessionalStudentAssignmentListItem>>;

public sealed record ProfessionalStudentAssignmentListItem(
    Guid Id,
    Guid MissionId,
    Guid EngagementId,
    Guid ProfessionalProfileId,
    Guid StudentId,
    string StudentDisplayName,
    string? StudentEmail,
    string? StudentPhone,
    DateOnly StartsOn,
    DateOnly EndsOn,
    string ScopeCode,
    string AssignmentReason,
    string Status,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? RevokedAtUtc,
    string? RevocationReason);

public sealed record ProfessionalStudentScopeStudent(
    Guid StudentId,
    string DisplayName,
    string? Email,
    string? Phone);

public interface IProfessionalStudentScopeGateway
{
    Task<bool> ExistsAsync(
        OrganizationId organizationId,
        PersonId studentId,
        CancellationToken ct=default);

    Task<ProfessionalStudentScopeStudent?> GetAsync(
        OrganizationId organizationId,
        PersonId studentId,
        CancellationToken ct=default);
}


public sealed record GetCurrentProfessionalStudentAssignmentsQuery(
    UserId UserId):IQuery<IReadOnlyList<ProfessionalStudentAssignmentListItem>>;

public sealed record GetCurrentProfessionalMissionStudentAssignmentsQuery(
    UserId UserId,
    ProfessionalMissionId MissionId):IQuery<IReadOnlyList<ProfessionalStudentAssignmentListItem>>;
