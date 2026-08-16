namespace DriveOS.Api.Endpoints.Organization.Networks;

public sealed record AddNetworkMemberRequest(Guid MemberOrganizationId);

public sealed record NetworkMemberResponse(
    Guid MembershipId,
    Guid OrganizationId,
    string LegalName,
    string CountryCode,
    string Status,
    DateTimeOffset JoinedAtUtc
);

public sealed record NetworkMemberCandidateResponse(
    Guid OrganizationId,
    string LegalName,
    string CountryCode,
    string Status,
    bool AlreadyAssignedToNetwork
);
