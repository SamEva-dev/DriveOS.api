using DriveOS.Modules.Students.Domain.Relationships;
using DriveOS.SharedKernel.Identifiers;
using FluentAssertions;

namespace DriveOS.UnitTests.Modules.Students;

public sealed class StudentRelationshipTests
{
    [Fact]
    public void NonPayer_CannotBePrimary()
    {
        var r = Create(StudentRelationshipType.EmergencyContact, true);
        r.Error.Should().Be(StudentRelationshipErrors.InvalidPrimaryPayer);
    }

    [Fact]
    public void PayerRights_RemainLimitedToConfiguredScopes()
    {
        var x = Create(StudentRelationshipType.Payer, true).Value;
        x.Permissions.Should()
            .Be(StudentRelationshipPermissions.Pay | StudentRelationshipPermissions.ViewInvoices);
        x.CommunicationScope.Should().Be(CommunicationScope.Financial);
    }

    [Fact]
    public void Suspension_RemovesPrimaryPayerFlag()
    {
        var x = Create(StudentRelationshipType.Payer, true).Value;
        x.Suspend("Temporarily unavailable", UserId.New(), DateTimeOffset.UtcNow);
        x.Status.Should().Be(StudentRelationshipStatus.Suspended);
        x.IsPrimaryPayer.Should().BeFalse();
    }

    private static DriveOS.SharedKernel.Results.Result<StudentRelationship> Create(
        StudentRelationshipType type,
        bool primary
    ) =>
        StudentRelationship.Create(
            OrganizationId.New(),
            PersonId.New(),
            Guid.NewGuid(),
            RelatedPartyKind.Person,
            "Alex Martin",
            "alex@example.test",
            null,
            type,
            StudentRelationshipPermissions.Pay | StudentRelationshipPermissions.ViewInvoices,
            FinancialScope.Invoices | FinancialScope.Payments,
            CommunicationScope.Financial,
            DateOnly.FromDateTime(DateTime.UtcNow),
            null,
            primary,
            UserId.New(),
            DateTimeOffset.UtcNow
        );
}
