using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.ProfessionalMarketplace.Domain.Compliance;

public interface IProfessionalDocumentRepository
{
    Task<ProfessionalDocument?> GetAsync(ProfessionalDocumentId id,bool tracking,CancellationToken ct=default);
    Task<IReadOnlyList<ProfessionalDocument>> ListByProfileAsync(ProfessionalProfileId profileId,CancellationToken ct=default);
    Task<bool> DocumentReferenceExistsAsync(ProfessionalProfileId profileId,Guid documentReferenceId,CancellationToken ct=default);
    Task<IReadOnlyList<ProfessionalDocument>> ListExpirationCandidatesAsync(DateOnly today,DateOnly warningDate,bool tracking,CancellationToken ct=default);
    void Add(ProfessionalDocument document);
}
public interface IProfessionalCredentialRepository
{
    Task<ProfessionalCredential?> GetAsync(ProfessionalCredentialId id,bool tracking,CancellationToken ct=default);
    Task<IReadOnlyList<ProfessionalCredential>> ListByProfileAsync(ProfessionalProfileId profileId,CancellationToken ct=default);
    Task<bool> DuplicateExistsAsync(ProfessionalProfileId profileId,string type,string country,string? reference,CancellationToken ct=default);
    Task<IReadOnlyList<ProfessionalCredential>> ListExpirationCandidatesAsync(DateOnly today,DateOnly warningDate,bool tracking,CancellationToken ct=default);
    void Add(ProfessionalCredential credential);
}
