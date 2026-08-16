using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Organizations.Domain.Networks;

public static class NetworkOrganizationMembershipErrors
{
    public static readonly Error InvalidIdentifier = Error.Validation(
        "Organizations.NetworkMembership.InvalidIdentifier",
        "errors.organizations.networkMembership.invalidIdentifier"
    );
    public static readonly Error SelfMembership = Error.Validation(
        "Organizations.NetworkMembership.SelfMembership",
        "errors.organizations.networkMembership.selfMembership"
    );
    public static readonly Error AlreadyEnded = Error.Conflict(
        "Organizations.NetworkMembership.AlreadyEnded",
        "errors.organizations.networkMembership.alreadyEnded"
    );
    public static readonly Error InvalidEndDate = Error.Validation(
        "Organizations.NetworkMembership.InvalidEndDate",
        "errors.organizations.networkMembership.invalidEndDate"
    );
    public static readonly Error CurrentOrganizationMustBeNetwork = Error.Forbidden(
        "Networks.CurrentOrganizationMustBeNetwork",
        "errors.organizations.networkMembership.currentOrganizationMustBeNetwork"
    );
    public static readonly Error MemberOrganizationNotFound = Error.NotFound(
        "Networks.MemberOrganizationNotFound",
        "errors.organizations.networkMembership.memberOrganizationNotFound"
    );
    public static readonly Error MemberOrganizationMustBeDrivingSchool = Error.Validation(
        "Networks.MemberOrganizationMustBeDrivingSchool",
        "errors.organizations.networkMembership.memberOrganizationMustBeDrivingSchool"
    );
    public static readonly Error ActiveMembershipAlreadyExists = Error.Conflict(
        "Networks.ActiveMembershipAlreadyExists",
        "errors.organizations.networkMembership.activeMembershipAlreadyExists"
    );
    public static readonly Error ActiveMembershipNotFound = Error.NotFound(
        "Networks.ActiveMembershipNotFound",
        "errors.organizations.networkMembership.activeMembershipNotFound"
    );
}
