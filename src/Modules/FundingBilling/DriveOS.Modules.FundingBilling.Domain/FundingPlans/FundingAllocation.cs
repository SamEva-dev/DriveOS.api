using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.FundingBilling.Domain.FundingPlans;

public sealed class FundingAllocation
{
    private FundingAllocation() { }

    private FundingAllocation(FundingAllocationId id, FundingPlanId fundingPlanId, PersonId? financingPersonId, OrganizationId? financingOrganizationId, decimal requestedAmount, string? externalReference)
    {
        Id = id;
        FundingPlanId = fundingPlanId;
        FinancingPersonId = financingPersonId;
        FinancingOrganizationId = financingOrganizationId;
        RequestedAmount = Round(requestedAmount);
        ExternalReference = NormalizeOptional(externalReference);
        Status = FundingAllocationStatus.Pending;
    }

    public FundingAllocationId Id { get; private set; }
    public FundingPlanId FundingPlanId { get; private set; }
    public PersonId? FinancingPersonId { get; private set; }
    public OrganizationId? FinancingOrganizationId { get; private set; }
    public decimal RequestedAmount { get; private set; }
    public decimal ApprovedAmount { get; private set; }
    public string? ExternalReference { get; private set; }
    public FundingAllocationStatus Status { get; private set; }
    public DateTimeOffset? DecidedAtUtc { get; private set; }
    public UserId? DecidedByUserId { get; private set; }
    public string? DecisionReason { get; private set; }

    internal static Result<FundingAllocation> Create(FundingAllocationId id, FundingPlanId planId, PersonId? personId, OrganizationId? organizationId, decimal requestedAmount, string? externalReference)
    {
        if (id.IsEmpty || planId.IsEmpty) return Result.Failure<FundingAllocation>(FundingPlanErrors.InvalidIdentifier);
        if (personId.HasValue == organizationId.HasValue) return Result.Failure<FundingAllocation>(FundingPlanErrors.InvalidFinancingParty);
        if (personId is { } person && person.IsEmpty || organizationId is { } organization && organization.IsEmpty) return Result.Failure<FundingAllocation>(FundingPlanErrors.InvalidFinancingParty);
        if (Round(requestedAmount) <= 0m) return Result.Failure<FundingAllocation>(FundingPlanErrors.InvalidAmount);
        if (NormalizeOptional(externalReference)?.Length > 250) return Result.Failure<FundingAllocation>(FundingPlanErrors.InvalidReference);
        return Result.Success(new FundingAllocation(id, planId, personId, organizationId, requestedAmount, externalReference));
    }

    internal Result Approve(decimal approvedAmount, UserId actorUserId, DateTimeOffset occurredAtUtc)
    {
        if (Status != FundingAllocationStatus.Pending) return Result.Failure(FundingPlanErrors.ApprovalNotAllowed);
        decimal rounded = Round(approvedAmount);
        if (rounded <= 0m || rounded > RequestedAmount) return Result.Failure(FundingPlanErrors.InvalidAmount);
        if (actorUserId.IsEmpty || occurredAtUtc == default) return Result.Failure(FundingPlanErrors.InvalidActor);
        ApprovedAmount = rounded; Status = FundingAllocationStatus.Approved; DecidedByUserId = actorUserId; DecidedAtUtc = occurredAtUtc.ToUniversalTime(); DecisionReason = null;
        return Result.Success();
    }

    internal Result Reject(string reason, UserId actorUserId, DateTimeOffset occurredAtUtc)
    {
        if (Status != FundingAllocationStatus.Pending) return Result.Failure(FundingPlanErrors.ApprovalNotAllowed);
        string normalized = reason?.Trim() ?? string.Empty;
        if (normalized.Length is < 3 or > 1000) return Result.Failure(FundingPlanErrors.InvalidReason);
        if (actorUserId.IsEmpty || occurredAtUtc == default) return Result.Failure(FundingPlanErrors.InvalidActor);
        ApprovedAmount = 0m; Status = FundingAllocationStatus.Rejected; DecisionReason = normalized; DecidedByUserId = actorUserId; DecidedAtUtc = occurredAtUtc.ToUniversalTime();
        return Result.Success();
    }

    private static decimal Round(decimal value) => decimal.Round(value, 2, MidpointRounding.AwayFromZero);
    private static string? NormalizeOptional(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
