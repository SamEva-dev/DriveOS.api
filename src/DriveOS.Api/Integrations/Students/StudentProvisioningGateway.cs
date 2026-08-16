using DriveOS.Modules.CRM.Application.Leads.ConvertLead;
using DriveOS.Modules.Students.Application.Provisioning;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Api.Integrations.Students;

internal sealed class StudentProvisioningGateway(IStudentProvisioningService service)
    : IStudentProvisioningGateway
{
    public async Task<Result<StudentProvisioningResult>> ProvisionAsync(
        StudentProvisioningRequest request,
        CancellationToken cancellationToken = default
    )
    {
        Result<ProvisionStudentResult> result = await service.ProvisionAsync(
            new ProvisionStudentRequest(
                request.OrganizationId,
                request.SourceLeadId,
                request.BranchId,
                request.FirstName,
                request.LastName,
                request.Email,
                request.Phone,
                request.TrainingCode
            ),
            cancellationToken
        );
        return result.IsFailure
            ? Result.Failure<StudentProvisioningResult>(result.Error)
            : Result.Success(
                new StudentProvisioningResult(
                    result.Value.StudentId,
                    result.Value.EnrollmentId,
                    result.Value.AlreadyProvisioned
                )
            );
    }
}
