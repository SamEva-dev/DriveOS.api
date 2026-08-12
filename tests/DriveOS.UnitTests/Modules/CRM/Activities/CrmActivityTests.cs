using DriveOS.Modules.CRM.Domain.Activities;
using DriveOS.Modules.CRM.Domain.Leads;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.UnitTests.Modules.CRM.Activities;

public sealed class CrmActivityTests
{
    private static readonly OrganizationId OrganizationId = new(Guid.NewGuid());
    private static readonly LeadId LeadId = new(Guid.NewGuid());

    [Fact]
    public void Create_ShouldNormalizeContent_AndStoreUtcDate()
    {
        var result = CrmActivity.Create(CrmActivityId.New(), OrganizationId, LeadId,
            CrmActivityType.Call, CrmActivityDirection.Outbound, "  Appel de suivi  ",
            "  Le prospect est intéressé.  ", DateTimeOffset.Now);

        Assert.True(result.IsSuccess);
        Assert.Equal("Appel de suivi", result.Value.Subject);
        Assert.Equal("Le prospect est intéressé.", result.Value.Details);
        Assert.Equal(TimeSpan.Zero, result.Value.OccurredAtUtc.Offset);
    }

    [Fact]
    public void Create_NoteWithDirection_ShouldFail()
    {
        var result = CrmActivity.Create(CrmActivityId.New(), OrganizationId, LeadId,
            CrmActivityType.Note, CrmActivityDirection.Outbound, "Note", null,
            DateTimeOffset.UtcNow);

        Assert.True(result.IsFailure);
        Assert.Equal(CrmActivityErrors.DirectionNotAllowed, result.Error);
    }
}
