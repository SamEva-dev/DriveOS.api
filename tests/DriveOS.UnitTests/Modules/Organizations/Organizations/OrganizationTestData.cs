using DriveOS.Modules.Organizations.Domain.Organizations;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.UnitTests.Organizations;

internal static class OrganizationTestData
{
    private static readonly DateTimeOffset DefaultChangedAtUtc = new(
        2026,
        7,
        29,
        8,
        0,
        0,
        TimeSpan.Zero
    );

    private static readonly Guid DefaultChangedByUserId = Guid.Parse(
        "11111111-1111-1111-1111-111111111111"
    );

    public static Organization CreateDraft(
        OrganizationId? organizationId = null,
        string legalName = "Auto-école Horizon",
        string countryCode = "FR",
        OrganizationType organizationType = OrganizationType.DrivingSchool
    )
    {
        Result<Organization> result = Organization.Create(
            organizationId ?? OrganizationId.New(),
            legalName,
            countryCode,
            organizationType
        );

        if (result.IsFailure)
        {
            throw new InvalidOperationException(
                $"Unable to create test organization. " + $"Error: {result.Error.Code}"
            );
        }

        return result.Value;
    }

    public static Organization CreatePendingActivation(
        OrganizationId? organizationId = null,
        Guid? changedByUserId = null,
        DateTimeOffset? changedAtUtc = null
    )
    {
        Organization organization = CreateDraft(organizationId);

        organization.SubmitForActivation(
            OrganizationStatusChangeReason.Create("Organization submitted for activation."),
            changedByUserId ?? DefaultChangedByUserId,
            changedAtUtc ?? DefaultChangedAtUtc
        );

        return organization;
    }

    public static Organization CreateActive(
        OrganizationId? organizationId = null,
        Guid? changedByUserId = null,
        DateTimeOffset? changedAtUtc = null
    )
    {
        Guid userId = changedByUserId ?? DefaultChangedByUserId;

        DateTimeOffset initialDate = changedAtUtc ?? DefaultChangedAtUtc;

        Organization organization = CreatePendingActivation(organizationId, userId, initialDate);

        organization.Activate(
            OrganizationStatusChangeReason.Create("Administrative checks completed."),
            userId,
            initialDate.AddMinutes(1)
        );

        return organization;
    }

    public static Organization CreateRestricted(
        OrganizationId? organizationId = null,
        Guid? changedByUserId = null,
        DateTimeOffset? changedAtUtc = null
    )
    {
        Guid userId = changedByUserId ?? DefaultChangedByUserId;

        DateTimeOffset initialDate = changedAtUtc ?? DefaultChangedAtUtc;

        Organization organization = CreateActive(organizationId, userId, initialDate);

        organization.Restrict(
            OrganizationStatusChangeReason.Create("Temporary operational restriction."),
            userId,
            initialDate.AddMinutes(2)
        );

        return organization;
    }

    public static Organization CreateSuspended(
        OrganizationId? organizationId = null,
        Guid? changedByUserId = null,
        DateTimeOffset? changedAtUtc = null
    )
    {
        Guid userId = changedByUserId ?? DefaultChangedByUserId;

        DateTimeOffset initialDate = changedAtUtc ?? DefaultChangedAtUtc;

        Organization organization = CreateActive(organizationId, userId, initialDate);

        organization.Suspend(
            OrganizationStatusChangeReason.Create("Organization temporarily suspended."),
            userId,
            initialDate.AddMinutes(2)
        );

        return organization;
    }

    public static Organization CreateClosed(
        OrganizationId? organizationId = null,
        Guid? changedByUserId = null,
        DateTimeOffset? changedAtUtc = null
    )
    {
        Guid userId = changedByUserId ?? DefaultChangedByUserId;

        DateTimeOffset initialDate = changedAtUtc ?? DefaultChangedAtUtc;

        Organization organization = CreateActive(organizationId, userId, initialDate);

        organization.Close(
            OrganizationStatusChangeReason.Create("Organization permanently closed."),
            userId,
            initialDate.AddMinutes(2)
        );

        return organization;
    }
}
