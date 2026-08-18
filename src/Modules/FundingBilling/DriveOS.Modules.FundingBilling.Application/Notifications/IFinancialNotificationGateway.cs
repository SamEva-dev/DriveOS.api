using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.FundingBilling.Application.Notifications;

public interface IFinancialNotificationGateway
{
    Task<Guid?> QueueInvoiceIssuedAsync(
        OrganizationId organizationId,
        BillingAccountId billingAccountId,
        string invoiceNumber,
        decimal amount,
        string currency,
        DateOnly dueDate,
        CancellationToken cancellationToken = default);

    Task<Guid?> QueuePaymentReminderAsync(
        OrganizationId organizationId,
        BillingAccountId billingAccountId,
        string targetType,
        decimal outstandingAmount,
        string currency,
        DateOnly dueDate,
        int sequenceNumber,
        CancellationToken cancellationToken = default);

    Task<Guid?> QueuePaymentReceivedAsync(
        OrganizationId organizationId,
        BillingAccountId billingAccountId,
        decimal amount,
        string currency,
        CancellationToken cancellationToken = default);

    Task<Guid?> QueuePaymentFailedAsync(
        OrganizationId organizationId,
        BillingAccountId billingAccountId,
        decimal amount,
        string currency,
        string reason,
        CancellationToken cancellationToken = default);

    Task<Guid?> QueueRefundCompletedAsync(
        OrganizationId organizationId,
        BillingAccountId billingAccountId,
        decimal amount,
        string currency,
        CancellationToken cancellationToken = default);

    Task<Guid?> QueueFundingDecisionAsync(
        OrganizationId organizationId,
        BillingAccountId billingAccountId,
        string status,
        decimal approvedAmount,
        decimal totalCost,
        string currency,
        CancellationToken cancellationToken = default);
}
