using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.FundingBilling.Application.SupplierPayments;

public sealed record SupplierFinanceNotificationRequest(
    Guid SupplierOrganizationId,
    OrganizationId ClientOrganizationId,
    string TemplateKey,
    string DeduplicationKey,
    IReadOnlyDictionary<string,string?> Parameters,
    Guid SupplierInvoiceId,
    UserId ActorUserId);

public interface ISupplierFinanceNotificationGateway
{
    Task TryEnqueueAsync(SupplierFinanceNotificationRequest request,CancellationToken ct=default);
}
