using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Modules.Workforce.Domain.Qualifications;
using DriveOS.SharedKernel.Identifiers;
namespace DriveOS.Modules.Workforce.Application.Qualifications;

public sealed record EmployeeQualificationResponse(Guid Id,string CountryCode,string QualificationType,string Title,string? Identifier,string? IssuingAuthority,DateOnly? IssuedOn,DateOnly? ExpiresOn,string Source,string Status,DateTimeOffset DeclaredAtUtc,DateTimeOffset? VerifiedAtUtc,string? VerificationMethod,string? DecisionReason,Guid? SupersededById);
public sealed record InstructorAuthorizationResponse(Guid Id,string CountryCode,string AuthorizationType,string Identifier,string IssuingAuthority,string? JurisdictionCode,string LicenseCategoryCode,DateOnly? IssuedOn,DateOnly? ExpiresOn,string Source,string Status,DateTimeOffset DeclaredAtUtc,DateTimeOffset? VerifiedAtUtc,string? VerificationMethod,string? DecisionReason,Guid? SupersededById);
public sealed record InstructorAuthorizationSnapshot(Guid EmployeeId,Guid? UserId,string CountryCode,string AuthorizationType,string Identifier,string IssuingAuthority,string? JurisdictionCode,string LicenseCategoryCode,DateOnly? IssuedOn,DateOnly? ExpiresOn,bool Verified);

public sealed record GetEmployeeQualificationsQuery(OrganizationId OrganizationId,EmployeeId EmployeeId):IQuery<IReadOnlyList<EmployeeQualificationResponse>>;
public sealed record DeclareEmployeeQualificationCommand(OrganizationId OrganizationId,EmployeeId EmployeeId,EmployeeQualificationId QualificationId,string CountryCode,string QualificationType,string Title,string? Identifier,string? IssuingAuthority,DateOnly? IssuedOn,DateOnly? ExpiresOn,QualificationSource Source,UserId ActorUserId):ICommand<EmployeeQualificationId>;
public sealed record VerifyEmployeeQualificationCommand(OrganizationId OrganizationId,EmployeeId EmployeeId,EmployeeQualificationId QualificationId,string VerificationMethod,string? Reason,UserId ActorUserId):ICommand;
public sealed record RejectEmployeeQualificationCommand(OrganizationId OrganizationId,EmployeeId EmployeeId,EmployeeQualificationId QualificationId,string Reason,UserId ActorUserId):ICommand;

public sealed record GetInstructorAuthorizationsQuery(OrganizationId OrganizationId,EmployeeId EmployeeId):IQuery<IReadOnlyList<InstructorAuthorizationResponse>>;
public sealed record DeclareInstructorAuthorizationCommand(OrganizationId OrganizationId,EmployeeId EmployeeId,InstructorAuthorizationId AuthorizationId,string CountryCode,string AuthorizationType,string Identifier,string IssuingAuthority,string? JurisdictionCode,string LicenseCategoryCode,DateOnly? IssuedOn,DateOnly? ExpiresOn,QualificationSource Source,UserId ActorUserId):ICommand<InstructorAuthorizationId>;
public sealed record VerifyInstructorAuthorizationCommand(OrganizationId OrganizationId,EmployeeId EmployeeId,InstructorAuthorizationId AuthorizationId,string VerificationMethod,string? Reason,UserId ActorUserId):ICommand;
public sealed record RejectInstructorAuthorizationCommand(OrganizationId OrganizationId,EmployeeId EmployeeId,InstructorAuthorizationId AuthorizationId,string Reason,UserId ActorUserId):ICommand;

public interface IWorkforceInstructorAuthorizationReadService
{
    Task<InstructorAuthorizationSnapshot?> ResolveCurrentAsync(OrganizationId organizationId,UserId instructorUserId,string countryCode,string authorizationType,string? licenseCategoryCode,DateOnly atDate,CancellationToken ct=default);
}
