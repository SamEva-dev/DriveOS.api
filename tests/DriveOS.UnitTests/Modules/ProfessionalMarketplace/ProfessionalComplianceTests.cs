using DriveOS.Modules.ProfessionalMarketplace.Domain.Compliance;
using DriveOS.SharedKernel.Identifiers;
using Xunit;

namespace DriveOS.UnitTests.Modules.ProfessionalMarketplace;
public sealed class ProfessionalComplianceTests
{
    private static readonly UserId Actor=new(Guid.NewGuid());
    [Fact] public void Expired_document_cannot_be_approved(){var x=ProfessionalDocument.Create(new(Guid.NewGuid()),new(Guid.NewGuid()),Guid.NewGuid(),"TEACHING_AUTHORIZATION","FR",true,new(2024,1,1),new(2025,1,1),DateTimeOffset.UtcNow,Actor).Value;x.SubmitForReview(DateTimeOffset.UtcNow,Actor);var r=x.Approve(ProfessionalVerificationMethod.Manual,new DateOnly(2026,8,24),DateTimeOffset.UtcNow,Actor);Assert.True(r.IsFailure);}
    [Fact] public void Credential_requires_current_validity_to_be_verified(){var x=ProfessionalCredential.Create(new(Guid.NewGuid()),new(Guid.NewGuid()),"TEACHING_AUTHORIZATION","FR","Authority","ABC",new(2027,1,1),null,["B"],null,DateTimeOffset.UtcNow,Actor).Value;var r=x.Verify(ProfessionalVerificationMethod.Manual,new DateOnly(2026,8,24),DateTimeOffset.UtcNow,Actor);Assert.True(r.IsFailure);}
    [Fact] public void Credential_and_document_are_distinct_objects(){var documentId=new ProfessionalDocumentId(Guid.NewGuid());var x=ProfessionalCredential.Create(new(Guid.NewGuid()),new(Guid.NewGuid()),"TEACHING_AUTHORIZATION","FR","Authority","ABC",new(2026,1,1),null,["B"],documentId,DateTimeOffset.UtcNow,Actor).Value;Assert.Equal(documentId,x.EvidenceDocumentId);Assert.Equal(ProfessionalCredentialStatus.PendingVerification,x.Status);}
}
