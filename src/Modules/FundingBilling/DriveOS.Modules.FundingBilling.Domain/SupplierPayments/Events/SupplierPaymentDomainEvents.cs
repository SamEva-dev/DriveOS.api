using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.FundingBilling.Domain.SupplierPayments.Events;

public sealed record SupplierPaymentScheduledDomainEvent(
    SupplierPaymentAttemptId SupplierPaymentAttemptId,
    SupplierInvoiceId SupplierInvoiceId,
    OrganizationId ClientOrganizationId,
    decimal Amount,
    string Currency,
    DateOnly ScheduledDate,
    DateTimeOffset SupplierPaymentScheduledAtUtc,
    UserId ActorUserId):DomainEvent;

public sealed record SupplierPaymentSucceededDomainEvent(
    SupplierPaymentAttemptId SupplierPaymentAttemptId,
    SupplierInvoiceId SupplierInvoiceId,
    OrganizationId ClientOrganizationId,
    decimal Amount,
    string Currency,
    string? ProviderReference,
    DateTimeOffset SupplierPaymentSucceededAtUtc,
    UserId ActorUserId):DomainEvent;

public sealed record SupplierPaymentFailedDomainEvent(
    SupplierPaymentAttemptId SupplierPaymentAttemptId,
    SupplierInvoiceId SupplierInvoiceId,
    OrganizationId ClientOrganizationId,
    decimal Amount,
    string Currency,
    string FailureReason,
    DateTimeOffset SupplierPaymentFailedAtUtc,
    UserId ActorUserId):DomainEvent;
