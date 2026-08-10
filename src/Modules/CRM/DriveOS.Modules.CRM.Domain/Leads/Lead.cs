using DriveOS.Modules.CRM.Domain.Leads.Events;
using DriveOS.SharedKernel.Auditing;
using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.CRM.Domain.Leads;

public sealed class Lead : AggregateRoot<LeadId>, IAuditableEntity
{
    private Lead()
    {
    }

    private Lead(
        LeadId id,
        OrganizationId organizationId,
        BranchId? branchId,
        LeadIdentity identity,
        RequestedTraining requestedTraining,
        LeadSource source,
        UserId? assignedAdvisorId)
        : base(id)
    {
        OrganizationId = organizationId;
        BranchId = branchId;
        Identity = identity;
        RequestedTraining = requestedTraining;
        Source = source;
        AssignedAdvisorId = assignedAdvisorId;
        Status = LeadStatus.New;
    }

    public OrganizationId OrganizationId { get; private set; }

    public BranchId? BranchId { get; private set; }

    public LeadIdentity Identity { get; private set; } = null!;

    public RequestedTraining RequestedTraining { get; private set; } = null!;

    public LeadSource Source { get; private set; } = null!;

    public UserId? AssignedAdvisorId { get; private set; }

    public LeadStatus Status { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public UserId? CreatedByUserId { get; private set; }

    public DateTimeOffset? LastModifiedAtUtc { get; private set; }

    public UserId? LastModifiedByUserId { get; private set; }

    public static Result<Lead> Create(
        LeadId id,
        OrganizationId organizationId,
        BranchId? branchId,
        LeadIdentity identity,
        RequestedTraining requestedTraining,
        LeadSource source,
        UserId? assignedAdvisorId = null)
    {
        if (id.IsEmpty)
        {
            return Result.Failure<Lead>(LeadErrors.EmptyId);
        }

        if (organizationId.IsEmpty)
        {
            return Result.Failure<Lead>(LeadErrors.EmptyOrganizationId);
        }

        ArgumentNullException.ThrowIfNull(identity);
        ArgumentNullException.ThrowIfNull(requestedTraining);
        ArgumentNullException.ThrowIfNull(source);

        var lead = new Lead(
            id,
            organizationId,
            branchId,
            identity,
            requestedTraining,
            source,
            assignedAdvisorId);

        lead.RaiseDomainEvent(
            new LeadCreatedDomainEvent(
                lead.Id,
                lead.OrganizationId,
                lead.BranchId,
                lead.Identity.FirstName,
                lead.Identity.LastName,
                lead.RequestedTraining.LicenseCategory,
                lead.Source.Type));

        return Result.Success(lead);
    }

    public Result UpdateInformation(
        BranchId? branchId,
        LeadIdentity identity,
        RequestedTraining requestedTraining,
        LeadSource source)
    {
        ArgumentNullException.ThrowIfNull(identity);
        ArgumentNullException.ThrowIfNull(requestedTraining);
        ArgumentNullException.ThrowIfNull(source);

        BranchId = branchId;
        Identity = identity;
        RequestedTraining = requestedTraining;
        Source = source;

        RaiseDomainEvent(
            new LeadInformationUpdatedDomainEvent(
                Id,
                OrganizationId,
                BranchId));

        return Result.Success();
    }

    public Result ChangeStatus(LeadStatus targetStatus, string? reason = null)
    {
        if (!Enum.IsDefined(targetStatus))
        {
            return Result.Failure(LeadErrors.InvalidStatus);
        }

        if (Status == targetStatus)
        {
            return Result.Failure(LeadErrors.StatusAlreadyApplied);
        }

        if (!CanTransition(Status, targetStatus))
        {
            return Result.Failure(LeadErrors.InvalidStatusTransition);
        }

        string? normalizedReason = string.IsNullOrWhiteSpace(reason)
            ? null
            : reason.Trim();

        if (targetStatus == LeadStatus.Lost && normalizedReason is null)
        {
            return Result.Failure(LeadErrors.LossReasonRequired);
        }

        if (normalizedReason?.Length > 500)
        {
            return Result.Failure(LeadErrors.StatusReasonTooLong);
        }

        LeadStatus previousStatus = Status;
        Status = targetStatus;
        RaiseDomainEvent(new LeadStatusChangedDomainEvent(
            Id,
            OrganizationId,
            previousStatus,
            targetStatus,
            normalizedReason));

        return Result.Success();
    }

    private static bool CanTransition(LeadStatus current, LeadStatus target) =>
        current switch
        {
            LeadStatus.New => target is LeadStatus.Contacted or LeadStatus.Lost or LeadStatus.Dormant,
            LeadStatus.Contacted => target is LeadStatus.Qualified or LeadStatus.Lost or LeadStatus.Dormant,
            LeadStatus.Qualified => target is LeadStatus.AssessmentScheduled or LeadStatus.OfferSent or LeadStatus.Lost or LeadStatus.Dormant,
            LeadStatus.AssessmentScheduled => target is LeadStatus.Qualified or LeadStatus.OfferSent or LeadStatus.Lost or LeadStatus.Dormant,
            LeadStatus.OfferSent => target is LeadStatus.Negotiation or LeadStatus.Won or LeadStatus.Lost or LeadStatus.Dormant,
            LeadStatus.Negotiation => target is LeadStatus.Won or LeadStatus.Lost or LeadStatus.Dormant,
            LeadStatus.Lost or LeadStatus.Dormant => target == LeadStatus.New,
            LeadStatus.Won => false,
            _ => false
        };

    public void SetCreatedAudit(
        DateTimeOffset createdAtUtc,
        UserId? createdByUserId)
    {
        if (CreatedAtUtc != default)
        {
            return;
        }

        CreatedAtUtc = createdAtUtc;
        CreatedByUserId = createdByUserId;
    }

    public void SetModifiedAudit(
        DateTimeOffset modifiedAtUtc,
        UserId? modifiedByUserId)
    {
        LastModifiedAtUtc = modifiedAtUtc;
        LastModifiedByUserId = modifiedByUserId;
    }
}
