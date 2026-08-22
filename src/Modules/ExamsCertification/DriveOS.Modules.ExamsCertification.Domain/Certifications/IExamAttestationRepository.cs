using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.ExamsCertification.Domain.Certifications;

public interface IExamAttestationRepository
{
    Task<ExamAttestation?> GetAsync(OrganizationId organizationId, ExamAttestationId id, CancellationToken cancellationToken = default);
    Task<ExamAttestation?> GetForUpdateAsync(OrganizationId organizationId, ExamAttestationId id, CancellationToken cancellationToken = default);
    Task<ExamAttestation?> GetCurrentAsync(OrganizationId organizationId, ExamResultId resultId, ExamAttestationType type, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ExamAttestation>> ListByResultAsync(OrganizationId organizationId, ExamResultId resultId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ExamAttestation>> ListByResultRevisionForUpdateAsync(OrganizationId organizationId, ExamResultId resultId, int resultRevision, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ExamAttestation>> ListByStudentAsync(OrganizationId organizationId, PersonId studentId, CancellationToken cancellationToken = default);
    Task<ExamAttestation?> FindByOperationIdAsync(OrganizationId organizationId, Guid operationId, CancellationToken cancellationToken = default);
    Task<ExamAttestation?> FindByPublicVerificationTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default);
    void Add(ExamAttestation attestation);
}
