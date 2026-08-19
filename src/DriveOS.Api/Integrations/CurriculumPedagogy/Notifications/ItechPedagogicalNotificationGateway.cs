using System.Net;
using System.Text.RegularExpressions;
using DriveOS.Modules.CurriculumPedagogy.Application.Notifications;
using DriveOS.Modules.CurriculumPedagogy.Domain.Readiness;
using DriveOS.Modules.Students.Application.Students.Identity;
using DriveOS.SharedKernel.Identifiers;
using Itech.Emailing.Abstractions;

namespace DriveOS.Api.Integrations.CurriculumPedagogy.Notifications;

internal sealed class ItechPedagogicalNotificationGateway(
    IStudentIdentityService students,
    IEmailingService emailing,
    ILogger<ItechPedagogicalNotificationGateway> logger) : IPedagogicalNotificationGateway
{
    public Task<Guid?> QueueTrainingPathSuspendedAsync(OrganizationId org, PersonId student, string reason, CancellationToken ct = default) => QueueAsync(org, student,
        r => Fr(r) ? "Votre parcours de formation est suspendu" : "Your training path has been suspended",
        r => Fr(r) ? $"<p>Bonjour {Name(r)},</p><p>Votre parcours de formation a été suspendu.</p><p>Motif : {E(reason)}</p>" : $"<p>Hello {Name(r)},</p><p>Your training path has been suspended.</p><p>Reason: {E(reason)}</p>", ct);

    public Task<Guid?> QueueTrainingPathReactivatedAsync(OrganizationId org, PersonId student, CancellationToken ct = default) => QueueAsync(org, student,
        r => Fr(r) ? "Votre parcours de formation est réactivé" : "Your training path has been reactivated",
        r => Fr(r) ? $"<p>Bonjour {Name(r)},</p><p>Votre parcours de formation est de nouveau actif.</p>" : $"<p>Hello {Name(r)},</p><p>Your training path is active again.</p>", ct);

    public Task<Guid?> QueuePedagogicalReviewCompletedAsync(OrganizationId org, PersonId student, string recommendations, decimal? remainingHours, CancellationToken ct = default) => QueueAsync(org, student,
        r => Fr(r) ? "Votre bilan pédagogique est disponible" : "Your pedagogical review is available",
        r => Fr(r) ? $"<p>Bonjour {Name(r)},</p><p>Votre bilan pédagogique a été finalisé.</p><p>Recommandations : {E(recommendations)}</p>{Hours(remainingHours, true)}" : $"<p>Hello {Name(r)},</p><p>Your pedagogical review has been completed.</p><p>Recommendations: {E(recommendations)}</p>{Hours(remainingHours, false)}", ct);

    public Task<Guid?> QueueRemediationActivatedAsync(OrganizationId org, PersonId student, string recommendation, DateOnly reviewDate, CancellationToken ct = default) => QueueAsync(org, student,
        r => Fr(r) ? "Plan de remédiation activé" : "Remediation plan activated",
        r => Fr(r) ? $"<p>Bonjour {Name(r)},</p><p>Un plan de remédiation a été activé pour votre parcours.</p><p>{E(recommendation)}</p><p>Réévaluation prévue : {reviewDate:dd/MM/yyyy}.</p>" : $"<p>Hello {Name(r)},</p><p>A remediation plan has been activated for your training path.</p><p>{E(recommendation)}</p><p>Review date: {reviewDate:yyyy-MM-dd}.</p>", ct);

    public Task<Guid?> QueueRemediationCompletedAsync(OrganizationId org, PersonId student, CancellationToken ct = default) => QueueAsync(org, student,
        r => Fr(r) ? "Plan de remédiation terminé" : "Remediation plan completed",
        r => Fr(r) ? $"<p>Bonjour {Name(r)},</p><p>Votre plan de remédiation a été clôturé. Votre progression reste disponible dans DriveOS.</p>" : $"<p>Hello {Name(r)},</p><p>Your remediation plan has been completed. Your progress remains available in DriveOS.</p>", ct);

    public Task<Guid?> QueueReadinessDecisionAsync(OrganizationId org, PersonId student, PedagogicalReadinessDecisionStatus decision, string rationale, string? conditions, CancellationToken ct = default) => QueueAsync(org, student,
        r => Fr(r) ? "Décision de préparation à l’examen" : "Exam readiness decision",
        r => Fr(r) ? $"<p>Bonjour {Name(r)},</p><p>Une décision pédagogique a été enregistrée : <strong>{E(decision.ToString())}</strong>.</p><p>{E(rationale)}</p>{Conditions(conditions, true)}" : $"<p>Hello {Name(r)},</p><p>A pedagogical readiness decision has been recorded: <strong>{E(decision.ToString())}</strong>.</p><p>{E(rationale)}</p>{Conditions(conditions, false)}", ct);

    private async Task<Guid?> QueueAsync(OrganizationId org, PersonId studentId, Func<StudentIdentityResponse,string> subject, Func<StudentIdentityResponse,string> html, CancellationToken ct)
    {
        var recipient = await students.GetAsync(org, studentId, ct);
        if (recipient is null || !recipient.AllowEmail || string.IsNullOrWhiteSpace(recipient.Email)) return null;
        try
        {
            string body = html(recipient);
            string text = WebUtility.HtmlDecode(Regex.Replace(body, "<[^>]+>", " "));
            return await emailing.QueueHtmlAsync(recipient.Email.Trim(), subject(recipient), body, text, null, EmailUseCaseTags.NotificationSystem, ct);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unable to queue pedagogical email for student {StudentId}.", studentId.Value);
            return null;
        }
    }
    private static bool Fr(StudentIdentityResponse r) => string.IsNullOrWhiteSpace(r.PreferredLanguage) || r.PreferredLanguage.StartsWith("fr", StringComparison.OrdinalIgnoreCase);
    private static string Name(StudentIdentityResponse r) => E(string.IsNullOrWhiteSpace(r.PreferredName) ? r.LegalFirstName : r.PreferredName);
    private static string E(string? v) => WebUtility.HtmlEncode(v ?? string.Empty);
    private static string Hours(decimal? h, bool fr) => h.HasValue ? (fr ? $"<p>Estimation restante : {h:0.##} h.</p>" : $"<p>Estimated remaining: {h:0.##} h.</p>") : string.Empty;
    private static string Conditions(string? c, bool fr) => string.IsNullOrWhiteSpace(c) ? string.Empty : (fr ? $"<p>Conditions : {E(c)}</p>" : $"<p>Conditions: {E(c)}</p>");
}
