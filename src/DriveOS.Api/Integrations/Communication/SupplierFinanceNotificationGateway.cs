using DriveOS.Modules.CommunicationEngagement.Application.Notifications;
using DriveOS.Modules.FundingBilling.Application.SupplierPayments;

namespace DriveOS.Api.Integrations.Communication;

internal sealed class SupplierFinanceNotificationGateway(
    ICommunicationNotificationWriter writer,
    ILogger<SupplierFinanceNotificationGateway> logger):ISupplierFinanceNotificationGateway
{
    public async Task TryEnqueueAsync(SupplierFinanceNotificationRequest request,CancellationToken ct=default)
    {
        try
        {
            await writer.EnqueueAsync(new(
                "Organization",
                request.SupplierOrganizationId,
                request.ClientOrganizationId,
                "SUPPLIER_PAYMENT",
                request.TemplateKey,
                request.DeduplicationKey,
                request.Parameters,
                "SUPPLIER_INVOICE",
                request.SupplierInvoiceId,
                null,
                null,
                request.ActorUserId),ct);
        }
        catch(Exception ex)
        {
            logger.LogError(ex,"Unable to enqueue supplier payment notification {DeduplicationKey}",request.DeduplicationKey);
        }
    }
}
