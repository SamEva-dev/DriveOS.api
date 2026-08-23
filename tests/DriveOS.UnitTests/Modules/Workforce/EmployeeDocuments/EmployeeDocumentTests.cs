using DriveOS.Modules.Workforce.Domain.EmployeeDocuments;using DriveOS.SharedKernel.Identifiers;
namespace DriveOS.UnitTests.Modules.Workforce.EmployeeDocuments;
public sealed class EmployeeDocumentTests
{
 [Fact] public void Expiration_is_derived_from_date_not_persisted_status(){var actor=UserId.New();var x=EmployeeDocument.Create(EmployeeDocumentId.New(),OrganizationId.New(),EmployeeId.New(),Guid.NewGuid(),EmployeeDocumentCategory.Qualification,"DIPLOMA","Diploma",EmployeeDocumentConfidentiality.Internal,new DateOnly(2026,1,1),null,new DateOnly(2026,6,30),null,null,DateTimeOffset.UtcNow,actor).Value;Assert.Equal(EmployeeDocumentStatus.Registered,x.Status);Assert.True(x.IsExpired(new DateOnly(2026,7,1)));}
 [Fact] public void Superseded_document_cannot_be_edited(){var actor=UserId.New();var x=EmployeeDocument.Create(EmployeeDocumentId.New(),OrganizationId.New(),EmployeeId.New(),Guid.NewGuid(),EmployeeDocumentCategory.Identity,"ID","Identity",EmployeeDocumentConfidentiality.Restricted,null,null,null,null,null,DateTimeOffset.UtcNow,actor).Value;x.Supersede(EmployeeDocumentId.New(),DateTimeOffset.UtcNow,actor);Assert.True(x.UpdateMetadata(EmployeeDocumentCategory.Identity,"ID","Changed",EmployeeDocumentConfidentiality.Restricted,null,null,null,null,null,DateTimeOffset.UtcNow,actor).IsFailure);}
}
