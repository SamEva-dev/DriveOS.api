using DriveOS.Modules.Organizations.Domain.Networks.Events;
using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Organizations.Domain.Networks;

public sealed class NetworkOrganizationMembership : AggregateRoot<NetworkOrganizationMembershipId>
{
    private NetworkOrganizationMembership() { }

    private NetworkOrganizationMembership(NetworkOrganizationMembershipId id, OrganizationId networkOrganizationId,
        OrganizationId memberOrganizationId, DateTimeOffset joinedAtUtc)
        : base(id)
    {
        NetworkOrganizationId = networkOrganizationId;
        MemberOrganizationId = memberOrganizationId;
        JoinedAtUtc = joinedAtUtc.ToUniversalTime();
    }

    public OrganizationId NetworkOrganizationId { get; private set; }
    public OrganizationId MemberOrganizationId { get; private set; }
    public DateTimeOffset JoinedAtUtc { get; private set; }
    public DateTimeOffset? EndedAtUtc { get; private set; }
    public bool IsActive => EndedAtUtc is null;

    public static Result<NetworkOrganizationMembership> Create(NetworkOrganizationMembershipId id,
        OrganizationId networkOrganizationId, OrganizationId memberOrganizationId,
        DateTimeOffset joinedAtUtc)
    {
        if (id.IsEmpty || networkOrganizationId.IsEmpty || memberOrganizationId.IsEmpty)
            return Result.Failure<NetworkOrganizationMembership>(NetworkOrganizationMembershipErrors.InvalidIdentifier);
        if (networkOrganizationId == memberOrganizationId)
            return Result.Failure<NetworkOrganizationMembership>(NetworkOrganizationMembershipErrors.SelfMembership);

        var membership = new NetworkOrganizationMembership(id, networkOrganizationId,
            memberOrganizationId, joinedAtUtc);
        membership.RaiseDomainEvent(new NetworkOrganizationMemberAddedDomainEvent(
            id, networkOrganizationId, memberOrganizationId, membership.JoinedAtUtc));
        return Result.Success(membership);
    }

    public Result End(DateTimeOffset endedAtUtc)
    {
        if (EndedAtUtc.HasValue)
            return Result.Failure(NetworkOrganizationMembershipErrors.AlreadyEnded);
        if (endedAtUtc.ToUniversalTime() < JoinedAtUtc)
            return Result.Failure(NetworkOrganizationMembershipErrors.InvalidEndDate);

        EndedAtUtc = endedAtUtc.ToUniversalTime();
        RaiseDomainEvent(new NetworkOrganizationMemberRemovedDomainEvent(
            Id, NetworkOrganizationId, MemberOrganizationId, EndedAtUtc.Value));
        return Result.Success();
    }
}
