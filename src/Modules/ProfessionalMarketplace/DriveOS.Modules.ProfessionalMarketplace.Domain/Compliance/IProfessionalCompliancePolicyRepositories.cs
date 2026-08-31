using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.ProfessionalMarketplace.Domain.Compliance;

public interface IProfessionalCompliancePolicyRepository
{
    Task<ProfessionalComplianceCriticalityPolicy?> GetAsync(
        ProfessionalCompliancePolicyId id,bool tracking,CancellationToken ct=default);
    Task<ProfessionalComplianceCriticalityPolicy?> GetApplicableAsync(
        string countryCode,string requirementCode,DateOnly date,CancellationToken ct=default);
    Task<bool> ExistsVersionAsync(
        string countryCode,string requirementCode,int version,CancellationToken ct=default);
    Task<IReadOnlyList<ProfessionalComplianceCriticalityPolicy>> ListAsync(
        string? countryCode,CancellationToken ct=default);
    void Add(ProfessionalComplianceCriticalityPolicy policy);
}

public interface IProfessionalComplianceWaiverRepository
{
    Task<ProfessionalComplianceWaiver?> GetAsync(
        ProfessionalComplianceWaiverId id,bool tracking,CancellationToken ct=default);
    Task<ProfessionalComplianceWaiver?> GetEffectiveAsync(
        ProfessionalProfileId profileId,string countryCode,string requirementCode,DateOnly date,CancellationToken ct=default);
    Task<bool> ExistsOverlappingAsync(
        ProfessionalProfileId profileId,string requirementCode,DateOnly from,DateOnly until,CancellationToken ct=default);
    Task<IReadOnlyList<ProfessionalComplianceWaiver>> ListByProfileAsync(
        ProfessionalProfileId profileId,CancellationToken ct=default);
    Task<IReadOnlyList<ProfessionalComplianceWaiver>> ListExpiredActiveAsync(
        DateOnly today,bool tracking,CancellationToken ct=default);
    void Add(ProfessionalComplianceWaiver waiver);
}
