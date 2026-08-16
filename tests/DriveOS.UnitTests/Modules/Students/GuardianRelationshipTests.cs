using DriveOS.Modules.Students.Domain.Guardians;
using DriveOS.SharedKernel.Identifiers;
using FluentAssertions;

namespace DriveOS.UnitTests.Modules.Students;

public sealed class GuardianRelationshipTests
{
    [Fact]
    public void Create_ShouldKeepIndependentPermissionsAndRights()
    {
        var item = Create(
            "parent@example.test",
            GuardianPermissions.ProfileRead | GuardianPermissions.ScheduleRead,
            true,
            false
        );
        item.Permissions.Should()
            .Be(GuardianPermissions.ProfileRead | GuardianPermissions.ScheduleRead);
        item.FinancialRights.Should().BeTrue();
        item.SignatureRights.Should().BeFalse();
    }

    [Fact]
    public void RevokedRelationship_ShouldNotBeEditable()
    {
        var item = Create("parent@example.test", GuardianPermissions.ProfileRead, false, false);
        var actor = UserId.New();
        item.Revoke("Court decision", actor, DateTimeOffset.UtcNow).IsSuccess.Should().BeTrue();
        item.Update(
                GuardianRelationshipType.Parent,
                "Court order",
                ParentalAuthorityStatus.Restricted,
                GuardianPermissions.None,
                DateOnly.FromDateTime(DateTime.UtcNow),
                null,
                false,
                false,
                "none",
                actor,
                DateTimeOffset.UtcNow
            )
            .Error.Should()
            .Be(GuardianErrors.Revoked);
    }

    [Fact]
    public void Invite_ShouldRequireAnEmailAddress()
    {
        Create(null, GuardianPermissions.ProfileRead, false, false)
            .Invite(UserId.New(), DateTimeOffset.UtcNow)
            .Error.Should()
            .Be(GuardianErrors.InvitationContactRequired);
    }

    private static GuardianRelationship Create(
        string? email,
        GuardianPermissions permissions,
        bool financial,
        bool signature
    ) =>
        GuardianRelationship
            .Create(
                OrganizationId.New(),
                PersonId.New(),
                PersonId.New(),
                "Alex",
                "Martin",
                email,
                "0102030405",
                GuardianRelationshipType.Parent,
                "Birth certificate",
                ParentalAuthorityStatus.Full,
                permissions,
                DateOnly.FromDateTime(DateTime.UtcNow),
                null,
                financial,
                signature,
                "email",
                UserId.New(),
                DateTimeOffset.UtcNow
            )
            .Value;
}
