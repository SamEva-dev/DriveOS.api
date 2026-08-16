using DriveOS.Modules.Students.Domain.ExternalTransfers;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.UnitTests.Modules.Students;

public sealed class ExternalTransferCaseTests
{
    private static readonly OrganizationId Source = new(
        Guid.Parse("11111111-1111-1111-1111-111111111111")
    );
    private static readonly OrganizationId Target = new(
        Guid.Parse("22222222-2222-2222-2222-222222222222")
    );
    private static readonly PersonId Student = new(
        Guid.Parse("33333333-3333-3333-3333-333333333333")
    );
    private static readonly UserId Actor = new(Guid.Parse("44444444-4444-4444-4444-444444444444"));
    private static readonly DateTimeOffset Now = new(2026, 8, 15, 8, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Create_rejects_same_organization()
    {
        var r = ExternalTransferCase.Create(
            Source,
            Source,
            Student,
            ExternalTransferType.FullTransfer,
            ExternalTransferDataScope.Identity,
            DateOnly.FromDateTime(Now.UtcDateTime),
            null,
            "FR",
            "Requested",
            "Source retains finance",
            Actor,
            Now
        );
        Assert.True(r.IsFailure);
        Assert.Equal(ExternalTransferErrors.SameOrganization, r.Error);
    }

    [Fact]
    public void Create_requires_explicit_data_scope()
    {
        var r = ExternalTransferCase.Create(
            Source,
            Target,
            Student,
            ExternalTransferType.FullTransfer,
            ExternalTransferDataScope.None,
            DateOnly.FromDateTime(Now.UtcDateTime),
            null,
            "FR",
            "Requested",
            "Responsibilities",
            Actor,
            Now
        );
        Assert.True(r.IsFailure);
    }

    [Fact]
    public void Submit_requires_verified_consent()
    {
        var transfer = Create();
        transfer.ReviewFinance(TransferFinancialStatus.Cleared, null, Actor, Now);
        var r = transfer.Submit(TargetRelationshipStatus.Active, Actor, Now);
        Assert.True(r.IsFailure);
        Assert.Equal(ExternalTransferErrors.ConsentRequired, r.Error);
    }

    [Fact]
    public void Submit_requires_financial_resolution()
    {
        var transfer = Create();
        transfer.VerifyConsent("consent://signed", Actor, Now);
        var r = transfer.Submit(TargetRelationshipStatus.Active, Actor, Now);
        Assert.True(r.IsFailure);
        Assert.Equal(ExternalTransferErrors.FinancialReviewRequired, r.Error);
    }

    [Fact]
    public void Acceptance_creates_only_authorized_grant_and_audit()
    {
        var transfer = Create(
            ExternalTransferType.TemporaryTransfer,
            DateOnly.FromDateTime(Now.UtcDateTime).AddDays(30)
        );
        transfer.VerifyConsent("consent://signed", Actor, Now);
        transfer.ReviewFinance(TransferFinancialStatus.Cleared, null, Actor, Now);
        transfer.Submit(TargetRelationshipStatus.Active, Actor, Now);
        var r = transfer.Decide(true, "Partner accepted", Actor, Now);
        Assert.True(r.IsSuccess);
        var grant = Assert.Single(transfer.DataGrants);
        Assert.Equal(
            ExternalTransferDataScope.Identity | ExternalTransferDataScope.CompletedHours,
            grant.Scope
        );
        Assert.Equal(Target, grant.GranteeOrganizationId);
        Assert.Contains(transfer.Audit, x => x.Action == "Accepted");
        Assert.Equal(Source, transfer.SourceOrganizationId);
    }

    [Fact]
    public void Temporary_grant_expires_automatically()
    {
        var until = DateOnly.FromDateTime(Now.UtcDateTime).AddDays(2);
        var transfer = Create(ExternalTransferType.TemporaryTransfer, until);
        transfer.VerifyConsent("consent://signed", Actor, Now);
        transfer.ReviewFinance(TransferFinancialStatus.Cleared, null, Actor, Now);
        transfer.Submit(TargetRelationshipStatus.Active, Actor, Now);
        transfer.Decide(true, "Accepted", Actor, Now);
        var grant = Assert.Single(transfer.DataGrants);
        Assert.True(grant.IsActive(until));
        Assert.False(grant.IsActive(until.AddDays(1)));
    }

    private static ExternalTransferCase Create(
        ExternalTransferType type = ExternalTransferType.FullTransfer,
        DateOnly? until = null
    ) =>
        ExternalTransferCase
            .Create(
                Source,
                Target,
                Student,
                type,
                ExternalTransferDataScope.Identity | ExternalTransferDataScope.CompletedHours,
                DateOnly.FromDateTime(Now.UtcDateTime),
                until,
                "FR",
                "Requested",
                "Source finance; target pedagogy",
                Actor,
                Now
            )
            .Value;
}
