using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Modules.FundingBilling.Domain.BillingAccounts;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.FundingBilling.Application.StudentFinance.Read;

public sealed record StudentFinancialOverviewResponse(
    Guid BillingAccountId,
    Guid StudentId,
    string Currency,
    string AccountStatus,
    StudentFinancialTotalsResponse Totals,
    StudentFinancialAlertsResponse Alerts,
    StudentFinancialNextInstallmentResponse? NextInstallment,
    IReadOnlyCollection<StudentFinancialInvoiceSummaryResponse> RecentInvoices,
    IReadOnlyCollection<StudentFinancialPaymentSummaryResponse> RecentPayments,
    IReadOnlyCollection<StudentFinancialFundingPlanSummaryResponse> FundingPlans,
    IReadOnlyCollection<StudentFinancialBillingPartySummaryResponse> BillingParties,
    IReadOnlyCollection<StudentFinancialCreditSummaryResponse> TrainingCredits,
    IReadOnlyCollection<StudentFinancialRefundSummaryResponse> RecentRefunds,
    IReadOnlyCollection<StudentFinancialCreditNoteSummaryResponse> RecentCreditNotes);

public sealed record StudentFinancialTotalsResponse(
    decimal TotalInvoiced,
    decimal TotalPaid,
    decimal TotalRefunded,
    decimal TotalCredited,
    decimal CreditBalance,
    decimal OutstandingBalance,
    decimal OverdueAmount,
    decimal UnallocatedPayments,
    decimal ApprovedFunding,
    decimal PlannedFunding,
    decimal AvailableTrainingCredits);

public sealed record StudentFinancialAlertsResponse(
    int OverdueInvoiceCount,
    int OverdueInstallmentCount,
    int PendingReminderCount,
    int FailedPaymentCount,
    int PendingFundingDecisionCount,
    int ExpiringCreditAccountCount,
    int PendingRefundCount,
    bool HasFinancialBlock);

public sealed record StudentFinancialNextInstallmentResponse(Guid Id, DateOnly DueDate, decimal ExpectedAmount, decimal PaidAmount, decimal RemainingAmount, string Status);
public sealed record StudentFinancialInvoiceSummaryResponse(Guid Id, string? Number, DateOnly? IssueDate, DateOnly? DueDate, string Status, decimal TotalAmount, decimal PaidAmount, decimal CreditedAmount, decimal RemainingAmount);
public sealed record StudentFinancialPaymentSummaryResponse(Guid Id, decimal Amount, decimal AllocatedAmount, decimal UnallocatedAmount, decimal RefundedAmount, string Status, string PaymentMethod, DateTimeOffset? PaidAtUtc);
public sealed record StudentFinancialFundingPlanSummaryResponse(Guid Id, Guid ContractId, decimal TotalCost, decimal StudentContribution, decimal RequestedFundingAmount, decimal ApprovedFundingAmount, string Status);
public sealed record StudentFinancialBillingPartySummaryResponse(Guid Id, Guid? PersonId, Guid? OrganizationId, string Role, decimal? MaximumAmount, int Priority, bool IsPrimary, string Status, DateOnly EffectiveFrom, DateOnly? EffectiveTo);
public sealed record StudentFinancialCreditSummaryResponse(Guid Id, string CreditType, decimal Purchased, decimal Reserved, decimal Consumed, decimal Adjustments, decimal Available, DateOnly? ExpirationDate, string Status);
public sealed record StudentFinancialRefundSummaryResponse(Guid Id, Guid PaymentId, decimal Amount, string Status, DateTimeOffset RequestedAtUtc, DateTimeOffset? CompletedAtUtc);
public sealed record StudentFinancialCreditNoteSummaryResponse(Guid Id, Guid InvoiceId, string? Number, DateOnly? IssueDate, decimal Amount, string Status);

public interface IStudentFinancialOverviewReadService
{
    Task<StudentFinancialOverviewResponse?> GetAsync(OrganizationId organizationId, PersonId studentId, DateOnly businessDate, CancellationToken cancellationToken = default);
}

public sealed record GetStudentFinancialOverviewQuery(OrganizationId OrganizationId, PersonId StudentId, DateOnly BusinessDate)
    : IQuery<StudentFinancialOverviewResponse>;

internal sealed class GetStudentFinancialOverviewQueryHandler(IStudentFinancialOverviewReadService readService)
    : IQueryHandler<GetStudentFinancialOverviewQuery, StudentFinancialOverviewResponse>
{
    public async Task<Result<StudentFinancialOverviewResponse>> Handle(GetStudentFinancialOverviewQuery query, CancellationToken cancellationToken)
    {
        StudentFinancialOverviewResponse? result = await readService.GetAsync(query.OrganizationId, query.StudentId, query.BusinessDate, cancellationToken);
        return result is null
            ? Result.Failure<StudentFinancialOverviewResponse>(BillingAccountErrors.NotFound)
            : Result.Success(result);
    }
}
