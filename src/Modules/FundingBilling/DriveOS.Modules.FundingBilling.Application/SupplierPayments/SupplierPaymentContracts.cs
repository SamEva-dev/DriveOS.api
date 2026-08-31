using DriveOS.Application.Abstractions.Messaging;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.FundingBilling.Application.SupplierPayments;

public sealed record ScheduleSupplierPaymentCommand(
    SupplierPaymentAttemptId Id,
    SupplierInvoiceId SupplierInvoiceId,
    OrganizationId ClientOrganizationId,
    decimal? Amount,
    DateOnly ScheduledDate,
    string PaymentMethod,
    string? BankReference,
    UserId ActorUserId,
    SupplierPaymentBatchId? BatchId=null):ICommand<SupplierPaymentAttemptId>;

public sealed record MarkSupplierPaymentProcessingCommand(
    SupplierPaymentAttemptId Id,
    OrganizationId ClientOrganizationId,
    UserId ActorUserId):ICommand;

public sealed record MarkSupplierPaymentPaidCommand(
    SupplierPaymentAttemptId Id,
    OrganizationId ClientOrganizationId,
    decimal? SettledAmount,
    DateOnly? SettledOn,
    string? ProviderReference,
    UserId ActorUserId):ICommand;

public sealed record MarkSupplierPaymentFailedCommand(
    SupplierPaymentAttemptId Id,
    OrganizationId ClientOrganizationId,
    string Reason,
    UserId ActorUserId):ICommand;

public sealed record CancelSupplierPaymentAttemptCommand(
    SupplierPaymentAttemptId Id,
    OrganizationId ClientOrganizationId,
    UserId ActorUserId):ICommand;

public sealed record RecordManualSupplierPaymentCommand(
    SupplierPaymentAttemptId Id,
    SupplierInvoiceId SupplierInvoiceId,
    OrganizationId ClientOrganizationId,
    decimal Amount,
    DateOnly PaidOn,
    string PaymentMethod,
    string? BankReference,
    string? ProviderReference,
    UserId ActorUserId):ICommand<SupplierPaymentAttemptId>;

public sealed record RecordSupplierPaymentRefundCommand(
    SupplierPaymentRefundId Id,
    SupplierInvoiceId SupplierInvoiceId,
    OrganizationId ClientOrganizationId,
    decimal Amount,
    string Reason,
    string Method,
    string? ProviderReference,
    UserId ActorUserId):ICommand<SupplierPaymentRefundId>;

public sealed record ScheduleSupplierPaymentBatchItem(
    SupplierInvoiceId SupplierInvoiceId,
    decimal? Amount);

public sealed record ScheduleSupplierPaymentBatchCommand(
    SupplierPaymentBatchId Id,
    OrganizationId ClientOrganizationId,
    DateOnly ScheduledDate,
    string PaymentMethod,
    string? BankReference,
    ScheduleSupplierPaymentBatchItem[] Items,
    UserId ActorUserId):ICommand<SupplierPaymentBatchId>;

public sealed record SupplierPaymentAttemptSnapshot(
    Guid AttemptId,
    string Status,
    decimal Amount,
    decimal? SettledAmount,
    string Currency,
    string PaymentMethod,
    DateOnly ScheduledDate,
    DateOnly? SettledOn,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? ProcessingAtUtc,
    DateTimeOffset? PaidAtUtc,
    DateTimeOffset? FailedAtUtc,
    DateTimeOffset? CancelledAtUtc,
    string? ProviderReference,
    string? FailureReason,
    string ReconciliationStatus,
    decimal? ReconciliationDifference,
    Guid? BatchId,
    bool IsManual);

public interface ISupplierPaymentTimelineReadService
{
    Task<IReadOnlyList<SupplierPaymentAttemptSnapshot>> ListAsync(
        SupplierInvoiceId supplierInvoiceId,
        CancellationToken ct=default);
}

public interface ISupplierSettlementOverdueAutomation
{
    Task<int> RunAsync(CancellationToken cancellationToken=default);
}
