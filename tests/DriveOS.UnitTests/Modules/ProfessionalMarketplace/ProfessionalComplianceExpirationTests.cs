using DriveOS.Modules.ProfessionalMarketplace.Domain.Compliance;
using DriveOS.SharedKernel.Identifiers;
using Xunit;

namespace DriveOS.UnitTests.Modules.ProfessionalMarketplace;

public sealed class ProfessionalComplianceExpirationTests
{
    private static readonly UserId Actor=new(Guid.NewGuid());

    [Fact]
    public void Valid_document_can_be_marked_expiring_soon()
    {
        var doc=ProfessionalDocument.Create(
            new(Guid.NewGuid()),new(Guid.NewGuid()),Guid.NewGuid(),"INSURANCE","FR",true,
            new DateOnly(2026,1,1),new DateOnly(2026,9,15),DateTimeOffset.UtcNow,Actor).Value;

        doc.SubmitForReview(DateTimeOffset.UtcNow,Actor);
        doc.Approve(ProfessionalVerificationMethod.Manual,new DateOnly(2026,8,25),DateTimeOffset.UtcNow,Actor);

        Assert.True(doc.MarkExpiringSoon(DateTimeOffset.UtcNow).IsSuccess);
        Assert.Equal(ProfessionalDocumentStatus.ExpiringSoon,doc.Status);
    }

    [Fact]
    public void Verified_credential_can_expire_automatically()
    {
        var credential=ProfessionalCredential.Create(
            new(Guid.NewGuid()),new(Guid.NewGuid()),"TEACHING_AUTH","FR","Préfecture",null,
            new DateOnly(2025,1,1),new DateOnly(2026,8,24),["B"],null,DateTimeOffset.UtcNow,Actor).Value;

        credential.Verify(ProfessionalVerificationMethod.Manual,new DateOnly(2026,8,23),DateTimeOffset.UtcNow,Actor);

        Assert.True(credential.MarkExpired(DateTimeOffset.UtcNow).IsSuccess);
        Assert.Equal(ProfessionalCredentialStatus.Expired,credential.Status);
    }
}
