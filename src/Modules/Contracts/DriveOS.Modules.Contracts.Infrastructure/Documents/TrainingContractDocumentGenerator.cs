using System.Net;
using System.Security.Cryptography;
using System.Text;
using DriveOS.Modules.Contracts.Application.TrainingContracts.Generate;
using DriveOS.Modules.Contracts.Domain.TrainingContracts;

namespace DriveOS.Modules.Contracts.Infrastructure.Documents;

internal sealed class TrainingContractDocumentGenerator : ITrainingContractDocumentGenerator
{
    public Task<TrainingContractGeneratedPayload> GenerateAsync(TrainingContract contract, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        static string H(string? value) => WebUtility.HtmlEncode(value ?? string.Empty);
        TrainingContractParty provider = contract.Parties.Single(x => x.Kind == TrainingContractPartyKind.TrainingProvider);
        TrainingContractParty student = contract.Parties.Single(x => x.Kind == TrainingContractPartyKind.Student);
        var t = contract.TermsSnapshot;

        string html = $$"""
            <!doctype html>
            <html lang="fr">
            <head>
                <meta charset="utf-8">
                <title>{{H(contract.ContractNumber)}}</title>
                <style>
                    body { font-family: Arial, sans-serif; max-width: 900px; margin: 40px auto; color: #172033; line-height: 1.5; }
                    h1 { color: #1e40af; }
                    h2 { margin-top: 28px; border-bottom: 1px solid #dbe2ea; padding-bottom: 6px; }
                    table { width: 100%; border-collapse: collapse; }
                    td { padding: 7px; border-bottom: 1px solid #edf0f4; }
                    .meta { color: #64748b; }
                </style>
            </head>
            <body>
                <h1>Contrat de formation</h1>
                <p class="meta">Contrat {{H(contract.ContractNumber)}} · Version {{contract.CurrentVersionNumber}}</p>

                <h2>Parties</h2>
                <table>
                    <tr><td>Établissement</td><td>{{H(provider.DisplayName)}}</td></tr>
                    <tr><td>Élève</td><td>{{H(student.DisplayName)}}</td></tr>
                </table>

                <h2>Formation</h2>
                <table>
                    <tr><td>Code</td><td>{{H(t.TrainingCode)}}</td></tr>
                    <tr><td>Heures pratiques</td><td>{{t.PracticalHours}}</td></tr>
                    <tr><td>Période</td><td>{{contract.StartDate:yyyy-MM-dd}} - {{contract.EndDate?.ToString("yyyy-MM-dd") ?? string.Empty}}</td></tr>
                    <tr><td>Montant</td><td>{{contract.TotalAmount:0.00}} {{H(contract.Currency)}}</td></tr>
                </table>

                <h2>Prestations</h2><p>{{H(t.ServicesSnapshot)}}</p>
                <h2>Échéancier</h2><p>{{H(t.PaymentScheduleSnapshot)}}</p>
                <h2>Annulation</h2><p>{{H(t.CancellationTerms)}}</p>
                <h2>Réservation</h2><p>{{H(t.BookingRules)}}</p>
                <h2>Obligations de l'élève</h2><p>{{H(t.StudentObligations)}}</p>
                <h2>Obligations de l'établissement</h2><p>{{H(t.ProviderObligations)}}</p>
                <h2>Présentation à l'examen</h2><p>{{H(t.ExamPresentationTerms)}}</p>
                <h2>Données personnelles</h2><p>{{H(t.DataProcessingTerms)}}</p>
            </body>
            </html>
            """;
        byte[] content = Encoding.UTF8.GetBytes(html);
        string sha = Convert.ToHexString(SHA256.HashData(content));
        string fileName = $"{contract.ContractNumber}-v{contract.CurrentVersionNumber}.html";
        return Task.FromResult(new TrainingContractGeneratedPayload(fileName, "text/html; charset=utf-8", content, sha));
    }
}
