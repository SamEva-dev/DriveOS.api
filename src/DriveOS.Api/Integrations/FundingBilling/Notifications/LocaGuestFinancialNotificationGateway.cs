using System.Globalization;
using System.Net;
using DriveOS.Modules.FundingBilling.Application.BillingAccounts.Read;
using DriveOS.Modules.FundingBilling.Application.Notifications;
using DriveOS.Modules.Students.Application.Students.Identity;
using DriveOS.SharedKernel.Identifiers;
using Itech.Emailing.Abstractions;

namespace DriveOS.Api.Integrations.FundingBilling.Notifications;

internal sealed class LocaGuestFinancialNotificationGateway(
    IBillingAccountReadService billingAccounts,
    IStudentIdentityService students,
    IEmailingService emailing,
    ILogger<LocaGuestFinancialNotificationGateway> logger) : IFinancialNotificationGateway
{
    public Task<Guid?> QueueInvoiceIssuedAsync(
        OrganizationId organizationId,
        BillingAccountId billingAccountId,
        string invoiceNumber,
        decimal amount,
        string currency,
        DateOnly dueDate,
        CancellationToken cancellationToken = default) => QueueAsync(
            organizationId,
            billingAccountId,
            recipient => IsFrench(recipient)
                ? $"Votre facture {invoiceNumber} est disponible"
                : $"Your invoice {invoiceNumber} is available",
            recipient => IsFrench(recipient)
                ? $"<p>Bonjour {Name(recipient)},</p><p>Votre facture <strong>{E(invoiceNumber)}</strong> d'un montant de <strong>{Money(amount, currency, recipient)}</strong> est disponible. Échéance : <strong>{dueDate:dd/MM/yyyy}</strong>.</p><p>Vous pouvez la consulter depuis votre espace DriveOS.</p>"
                : $"<p>Hello {Name(recipient)},</p><p>Your invoice <strong>{E(invoiceNumber)}</strong> for <strong>{Money(amount, currency, recipient)}</strong> is available. Due date: <strong>{dueDate:yyyy-MM-dd}</strong>.</p><p>You can view it from your DriveOS portal.</p>",
            EmailUseCaseTags.BillingInvoiceSent,
            cancellationToken);

    public Task<Guid?> QueuePaymentReminderAsync(
        OrganizationId organizationId,
        BillingAccountId billingAccountId,
        string targetType,
        decimal outstandingAmount,
        string currency,
        DateOnly dueDate,
        int sequenceNumber,
        CancellationToken cancellationToken = default) => QueueAsync(
            organizationId,
            billingAccountId,
            recipient => IsFrench(recipient)
                ? $"Rappel de paiement DriveOS — échéance du {dueDate:dd/MM/yyyy}"
                : $"DriveOS payment reminder — due {dueDate:yyyy-MM-dd}",
            recipient => IsFrench(recipient)
                ? $"<p>Bonjour {Name(recipient)},</p><p>Un montant de <strong>{Money(outstandingAmount, currency, recipient)}</strong> reste dû depuis le <strong>{dueDate:dd/MM/yyyy}</strong>.</p><p>Il s'agit de votre relance n°{sequenceNumber}. Consultez votre espace DriveOS pour régulariser la situation.</p>"
                : $"<p>Hello {Name(recipient)},</p><p><strong>{Money(outstandingAmount, currency, recipient)}</strong> remains outstanding since <strong>{dueDate:yyyy-MM-dd}</strong>.</p><p>This is reminder #{sequenceNumber}. Please open your DriveOS portal to review the balance.</p>",
            EmailUseCaseTags.BillingPaymentReminder,
            cancellationToken);

    public Task<Guid?> QueuePaymentReceivedAsync(
        OrganizationId organizationId,
        BillingAccountId billingAccountId,
        decimal amount,
        string currency,
        CancellationToken cancellationToken = default) => QueueAsync(
            organizationId,
            billingAccountId,
            recipient => IsFrench(recipient) ? "Paiement reçu" : "Payment received",
            recipient => IsFrench(recipient)
                ? $"<p>Bonjour {Name(recipient)},</p><p>Nous confirmons la réception d'un paiement de <strong>{Money(amount, currency, recipient)}</strong>.</p>"
                : $"<p>Hello {Name(recipient)},</p><p>We confirm receipt of your payment of <strong>{Money(amount, currency, recipient)}</strong>.</p>",
            EmailUseCaseTags.BillingReceiptSent,
            cancellationToken);

    public Task<Guid?> QueuePaymentFailedAsync(
        OrganizationId organizationId,
        BillingAccountId billingAccountId,
        decimal amount,
        string currency,
        string reason,
        CancellationToken cancellationToken = default) => QueueAsync(
            organizationId,
            billingAccountId,
            recipient => IsFrench(recipient) ? "Échec de paiement" : "Payment failed",
            recipient => IsFrench(recipient)
                ? $"<p>Bonjour {Name(recipient)},</p><p>Le paiement de <strong>{Money(amount, currency, recipient)}</strong> n'a pas abouti.</p><p>{E(reason)}</p>"
                : $"<p>Hello {Name(recipient)},</p><p>Your payment of <strong>{Money(amount, currency, recipient)}</strong> could not be completed.</p><p>{E(reason)}</p>",
            EmailUseCaseTags.BillingPaymentFailed,
            cancellationToken);

    public Task<Guid?> QueueRefundCompletedAsync(
        OrganizationId organizationId,
        BillingAccountId billingAccountId,
        decimal amount,
        string currency,
        CancellationToken cancellationToken = default) => QueueAsync(
            organizationId,
            billingAccountId,
            recipient => IsFrench(recipient) ? "Remboursement effectué" : "Refund completed",
            recipient => IsFrench(recipient)
                ? $"<p>Bonjour {Name(recipient)},</p><p>Un remboursement de <strong>{Money(amount, currency, recipient)}</strong> a été effectué.</p>"
                : $"<p>Hello {Name(recipient)},</p><p>A refund of <strong>{Money(amount, currency, recipient)}</strong> has been completed.</p>",
            EmailUseCaseTags.NotificationSystem,
            cancellationToken);

    public Task<Guid?> QueueFundingDecisionAsync(
        OrganizationId organizationId,
        BillingAccountId billingAccountId,
        string status,
        decimal approvedAmount,
        decimal totalCost,
        string currency,
        CancellationToken cancellationToken = default) => QueueAsync(
            organizationId,
            billingAccountId,
            recipient => IsFrench(recipient) ? "Mise à jour de votre financement" : "Funding update",
            recipient => IsFrench(recipient)
                ? $"<p>Bonjour {Name(recipient)},</p><p>Votre plan de financement est maintenant <strong>{E(status)}</strong>.</p><p>Montant approuvé : <strong>{Money(approvedAmount, currency, recipient)}</strong> sur {Money(totalCost, currency, recipient)}.</p>"
                : $"<p>Hello {Name(recipient)},</p><p>Your funding plan is now <strong>{E(status)}</strong>.</p><p>Approved amount: <strong>{Money(approvedAmount, currency, recipient)}</strong> of {Money(totalCost, currency, recipient)}.</p>",
            EmailUseCaseTags.NotificationSystem,
            cancellationToken);

    private async Task<Guid?> QueueAsync(
        OrganizationId organizationId,
        BillingAccountId billingAccountId,
        Func<StudentIdentityResponse, string> subjectFactory,
        Func<StudentIdentityResponse, string> htmlFactory,
        EmailUseCaseTags tags,
        CancellationToken cancellationToken)
    {
        BillingAccountResponse? account = await billingAccounts.GetByIdAsync(organizationId, billingAccountId, cancellationToken);
        if (account is null)
        {
            logger.LogWarning("Financial email skipped: billing account {BillingAccountId} was not found for organization {OrganizationId}.", billingAccountId.Value, organizationId.Value);
            return null;
        }

        StudentIdentityResponse? recipient = await students.GetAsync(organizationId, new PersonId(account.StudentId), cancellationToken);
        if (recipient is null || !recipient.AllowEmail || string.IsNullOrWhiteSpace(recipient.Email))
        {
            logger.LogInformation("Financial email skipped for billing account {BillingAccountId}: student has no authorized email recipient.", billingAccountId.Value);
            return null;
        }

        try
        {
            string subject = subjectFactory(recipient);
            string html = htmlFactory(recipient);
            string text = WebUtility.HtmlDecode(System.Text.RegularExpressions.Regex.Replace(html, "<[^>]+>", " "));
            return await emailing.QueueHtmlAsync(recipient.Email.Trim(), subject, html, text, null, tags, cancellationToken);
        }
        catch (Exception ex)
        {
            // The financial transaction is already committed when this adapter is called.
            // Never turn a successful financial operation into an HTTP failure because the email queue is unavailable.
            logger.LogError(ex, "Unable to queue financial email for billing account {BillingAccountId}.", billingAccountId.Value);
            return null;
        }
    }

    private static bool IsFrench(StudentIdentityResponse recipient) =>
        string.Equals(recipient.PreferredLanguage, "fr", StringComparison.OrdinalIgnoreCase) ||
        string.IsNullOrWhiteSpace(recipient.PreferredLanguage);

    private static string Name(StudentIdentityResponse recipient) =>
        E(string.IsNullOrWhiteSpace(recipient.PreferredName) ? recipient.LegalFirstName : recipient.PreferredName!);

    private static string Money(decimal amount, string currency, StudentIdentityResponse recipient)
    {
        CultureInfo culture = IsFrench(recipient) ? CultureInfo.GetCultureInfo("fr-FR") : CultureInfo.GetCultureInfo("en-US");
        return $"{amount.ToString("N2", culture)} {E(currency)}";
    }

    private static string E(string? value) => WebUtility.HtmlEncode(value ?? string.Empty);
}
