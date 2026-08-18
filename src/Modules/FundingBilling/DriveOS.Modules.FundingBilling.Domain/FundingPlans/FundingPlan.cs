using DriveOS.Modules.FundingBilling.Domain.FundingPlans.Events;
using DriveOS.SharedKernel.Auditing;
using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.FundingBilling.Domain.FundingPlans;

public sealed class FundingPlan : AggregateRoot<FundingPlanId>, IAuditableEntity
{
    private readonly List<FundingAllocation> _allocations = [];
    private FundingPlan() { }

    private FundingPlan(FundingPlanId id, OrganizationId organizationId, BillingAccountId billingAccountId, PersonId studentId, Guid contractId, decimal totalCost, decimal studentContribution, string currency) : base(id)
    {
        OrganizationId = organizationId; BillingAccountId = billingAccountId; StudentId = studentId; ContractId = contractId;
        TotalCost = Round(totalCost); StudentContribution = Round(studentContribution); Currency = currency; Status = FundingPlanStatus.Draft;
    }

    public OrganizationId OrganizationId { get; private set; }
    public BillingAccountId BillingAccountId { get; private set; }
    public PersonId StudentId { get; private set; }
    public Guid ContractId { get; private set; }
    public decimal TotalCost { get; private set; }
    public decimal StudentContribution { get; private set; }
    public string Currency { get; private set; } = string.Empty;
    public FundingPlanStatus Status { get; private set; }
    public IReadOnlyCollection<FundingAllocation> Allocations => _allocations.AsReadOnly();
    public decimal RequestedFundingAmount => Round(_allocations.Sum(x => x.RequestedAmount));
    public decimal ApprovedFundingAmount => Round(_allocations.Where(x => x.Status is FundingAllocationStatus.Approved or FundingAllocationStatus.Exhausted).Sum(x => x.ApprovedAmount));
    public decimal PlannedAmount => Round(StudentContribution + RequestedFundingAmount);
    public decimal ApprovedCoverageAmount => Round(StudentContribution + ApprovedFundingAmount);
    public decimal RemainingToPlan => decimal.Max(0m, Round(TotalCost - PlannedAmount));
    public decimal RemainingToApprove => decimal.Max(0m, Round(TotalCost - ApprovedCoverageAmount));
    public DateTimeOffset? SubmittedAtUtc { get; private set; }
    public UserId? SubmittedByUserId { get; private set; }
    public DateTimeOffset? ApprovedAtUtc { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public UserId? CreatedByUserId { get; private set; }
    public DateTimeOffset? LastModifiedAtUtc { get; private set; }
    public UserId? LastModifiedByUserId { get; private set; }

    public static Result<FundingPlan> Create(FundingPlanId id, OrganizationId organizationId, BillingAccountId billingAccountId, PersonId studentId, Guid contractId, decimal totalCost, decimal studentContribution, string currency)
    {
        string normalizedCurrency = NormalizeCurrency(currency); decimal roundedCost = Round(totalCost); decimal roundedContribution = Round(studentContribution);
        if (id.IsEmpty) return Result.Failure<FundingPlan>(FundingPlanErrors.InvalidIdentifier);
        if (organizationId.IsEmpty || billingAccountId.IsEmpty || studentId.IsEmpty) return Result.Failure<FundingPlan>(FundingPlanErrors.InvalidOwner);
        if (contractId == Guid.Empty) return Result.Failure<FundingPlan>(FundingPlanErrors.InvalidContract);
        if (roundedCost <= 0m || roundedContribution < 0m || roundedContribution > roundedCost) return Result.Failure<FundingPlan>(FundingPlanErrors.InvalidAmount);
        if (!IsValidCurrency(normalizedCurrency)) return Result.Failure<FundingPlan>(FundingPlanErrors.InvalidCurrency);
        var plan = new FundingPlan(id, organizationId, billingAccountId, studentId, contractId, roundedCost, roundedContribution, normalizedCurrency);
        plan.RaiseDomainEvent(new FundingPlanCreatedDomainEvent(plan.Id, plan.BillingAccountId, plan.ContractId, plan.TotalCost, plan.Currency));
        return Result.Success(plan);
    }

    public Result<FundingAllocationId> AddAllocation(FundingAllocationId allocationId, PersonId? financingPersonId, OrganizationId? financingOrganizationId, decimal requestedAmount, string? externalReference)
    {
        if (Status != FundingPlanStatus.Draft) return Result.Failure<FundingAllocationId>(FundingPlanErrors.ModificationNotAllowed);
        Result<FundingAllocation> created = FundingAllocation.Create(allocationId, Id, financingPersonId, financingOrganizationId, requestedAmount, externalReference);
        if (created.IsFailure) return Result.Failure<FundingAllocationId>(created.Error);
        if (Round(PlannedAmount + created.Value.RequestedAmount) > TotalCost) return Result.Failure<FundingAllocationId>(FundingPlanErrors.AllocationExceeded);
        _allocations.Add(created.Value);
        return Result.Success(created.Value.Id);
    }

    public Result Submit(UserId actorUserId, DateTimeOffset occurredAtUtc)
    {
        if (Status != FundingPlanStatus.Draft) return Result.Failure(FundingPlanErrors.ModificationNotAllowed);
        if (actorUserId.IsEmpty || occurredAtUtc == default) return Result.Failure(FundingPlanErrors.InvalidActor);
        if (PlannedAmount != TotalCost) return Result.Failure(FundingPlanErrors.CoverageIncomplete);
        SubmittedAtUtc = occurredAtUtc.ToUniversalTime(); SubmittedByUserId = actorUserId;
        if (_allocations.Count == 0) { Status = FundingPlanStatus.Approved; ApprovedAtUtc = SubmittedAtUtc; RaiseDomainEvent(new FundingPlanApprovedDomainEvent(Id, TotalCost, Currency, ApprovedAtUtc.Value)); }
        else Status = FundingPlanStatus.PendingApproval;
        RaiseDomainEvent(new FundingPlanSubmittedDomainEvent(Id, actorUserId, SubmittedAtUtc.Value));
        return Result.Success();
    }

    public Result ApproveAllocation(FundingAllocationId allocationId, decimal approvedAmount, UserId actorUserId, DateTimeOffset occurredAtUtc)
    {
        if (Status is not (FundingPlanStatus.PendingApproval or FundingPlanStatus.PartiallyApproved)) return Result.Failure(FundingPlanErrors.ApprovalNotAllowed);
        FundingAllocation? allocation = _allocations.SingleOrDefault(x => x.Id == allocationId);
        if (allocation is null) return Result.Failure(FundingPlanErrors.AllocationNotFound);
        Result result = allocation.Approve(approvedAmount, actorUserId, occurredAtUtc); if (result.IsFailure) return result;
        RaiseDomainEvent(new FundingAllocationApprovedDomainEvent(Id, allocation.Id, allocation.ApprovedAmount, Currency, actorUserId, occurredAtUtc.ToUniversalTime()));
        RecalculateStatus(occurredAtUtc); return Result.Success();
    }

    public Result RejectAllocation(FundingAllocationId allocationId, string reason, UserId actorUserId, DateTimeOffset occurredAtUtc)
    {
        if (Status is not (FundingPlanStatus.PendingApproval or FundingPlanStatus.PartiallyApproved)) return Result.Failure(FundingPlanErrors.ApprovalNotAllowed);
        FundingAllocation? allocation = _allocations.SingleOrDefault(x => x.Id == allocationId);
        if (allocation is null) return Result.Failure(FundingPlanErrors.AllocationNotFound);
        Result result = allocation.Reject(reason, actorUserId, occurredAtUtc); if (result.IsFailure) return result;
        RaiseDomainEvent(new FundingAllocationRejectedDomainEvent(Id, allocation.Id, allocation.DecisionReason!, actorUserId, occurredAtUtc.ToUniversalTime()));
        RecalculateStatus(occurredAtUtc); return Result.Success();
    }

    public void SetCreatedAudit(DateTimeOffset atUtc, UserId? userId) { if (CreatedAtUtc != default) return; CreatedAtUtc = atUtc.ToUniversalTime(); CreatedByUserId = userId; }
    public void SetModifiedAudit(DateTimeOffset atUtc, UserId? userId) { LastModifiedAtUtc = atUtc.ToUniversalTime(); LastModifiedByUserId = userId; }

    private void RecalculateStatus(DateTimeOffset occurredAtUtc)
    {
        if (ApprovedCoverageAmount == TotalCost) { Status = FundingPlanStatus.Approved; ApprovedAtUtc = occurredAtUtc.ToUniversalTime(); RaiseDomainEvent(new FundingPlanApprovedDomainEvent(Id, TotalCost, Currency, ApprovedAtUtc.Value)); return; }
        if (_allocations.All(x => x.Status != FundingAllocationStatus.Pending) && ApprovedCoverageAmount < TotalCost) { Status = ApprovedFundingAmount > 0m ? FundingPlanStatus.PartiallyApproved : FundingPlanStatus.Rejected; return; }
        Status = ApprovedFundingAmount > 0m ? FundingPlanStatus.PartiallyApproved : FundingPlanStatus.PendingApproval;
    }

    private static decimal Round(decimal value) => decimal.Round(value, 2, MidpointRounding.AwayFromZero);
    private static string NormalizeCurrency(string currency) => currency?.Trim().ToUpperInvariant() ?? string.Empty;
    private static bool IsValidCurrency(string currency) => currency.Length == 3 && currency.All(c => c is >= 'A' and <= 'Z');
}
