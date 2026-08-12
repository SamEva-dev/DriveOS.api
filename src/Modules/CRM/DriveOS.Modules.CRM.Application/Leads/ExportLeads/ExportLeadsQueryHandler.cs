using System.Globalization;
using System.Text;
using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Modules.CRM.Application.Abstractions.Persistence;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.CRM.Application.Leads.ExportLeads;

public sealed class ExportLeadsQueryHandler(ILeadReadService leadReadService)
    : IQueryHandler<ExportLeadsQuery, LeadExportFile>
{
    private const int MaximumRows = 50_000;

    public async Task<Result<LeadExportFile>> Handle(ExportLeadsQuery query, CancellationToken cancellationToken)
    {
        IReadOnlyList<LeadExportRow> rows = await leadReadService.GetForExportAsync(
            query.OrganizationId, query.Search, query.BranchId, query.Status, query.SourceType,
            query.AssignedAdvisorId, query.UnassignedOnly, MaximumRows, cancellationToken);

        var csv = new StringBuilder("Id,FirstName,LastName,Email,Phone,LicenseCategory,Transmission,Source,BranchId,AdvisorId,Status,CreatedAtUtc,LastModifiedAtUtc\r\n");
        foreach (LeadExportRow row in rows)
        {
            string[] values = [row.Id.ToString("D"), row.FirstName, row.LastName, row.Email ?? string.Empty,
                row.Phone ?? string.Empty, row.LicenseCategory, row.Transmission.ToString(),
                row.SourceType.ToString(), row.BranchId?.ToString("D") ?? string.Empty,
                row.AssignedAdvisorId?.ToString("D") ?? string.Empty, row.Status.ToString(),
                row.CreatedAtUtc.ToString("O", CultureInfo.InvariantCulture),
                row.LastModifiedAtUtc?.ToString("O", CultureInfo.InvariantCulture) ?? string.Empty];
            csv.AppendJoin(',', values.Select(Escape)).Append("\r\n");
        }

        byte[] content = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(csv.ToString())).ToArray();
        return Result.Success(new LeadExportFile(content, $"driveos-prospects-{DateTime.UtcNow:yyyyMMdd-HHmmss}.csv", rows.Count));
    }

    private static string Escape(string value) =>
        value.IndexOfAny([',', '"', '\r', '\n']) >= 0 ? $"\"{value.Replace("\"", "\"\"")}\"" : value;
}
