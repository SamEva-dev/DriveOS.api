using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.CRM.Application.Leads.ConvertLead;

public sealed record StudentProvisioningRequest(
    OrganizationId OrganizationId,
    LeadId SourceLeadId,
    BranchId BranchId,
    string FirstName,
    string LastName,
    string? Email,
    string? Phone,
    string TrainingCode
);

public sealed record StudentProvisioningResult(
    PersonId StudentId,
    DraftEnrollmentId EnrollmentId,
    bool AlreadyProvisioned
);

public interface IStudentProvisioningGateway
{
    Task<Result<StudentProvisioningResult>> ProvisionAsync(
        StudentProvisioningRequest request,
        CancellationToken cancellationToken = default
    );
}
