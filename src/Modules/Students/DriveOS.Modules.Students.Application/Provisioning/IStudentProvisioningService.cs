using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Students.Application.Provisioning;

public sealed record ProvisionStudentRequest(
    OrganizationId OrganizationId,
    LeadId SourceLeadId,
    BranchId BranchId,
    string FirstName,
    string LastName,
    string? Email,
    string? Phone,
    string TrainingCode
);

public sealed record ProvisionStudentResult(
    PersonId StudentId,
    DraftEnrollmentId EnrollmentId,
    bool AlreadyProvisioned
);

public interface IStudentProvisioningService
{
    Task<Result<ProvisionStudentResult>> ProvisionAsync(
        ProvisionStudentRequest request,
        CancellationToken cancellationToken = default
    );
}
