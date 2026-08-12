using DriveOS.Modules.CRM.Domain.Leads;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.CRM.Application.Leads.BulkActions;

public enum LeadBulkActionType { AssignAdvisor = 0, ChangeStatus = 1 }
public sealed record LeadBulkActionInput(IReadOnlyCollection<Guid> LeadIds, LeadBulkActionType Action,
    Guid? AdvisorId, LeadStatus? TargetStatus, string? Reason);
public sealed record LeadBulkActionItem(Guid LeadId, bool Succeeded, string? ErrorKey);
public sealed record LeadBulkActionResult(int Requested, int Succeeded, int Failed,
    IReadOnlyList<LeadBulkActionItem> Items);

public interface ILeadBulkActionService
{
    Task<LeadBulkActionResult> ExecuteAsync(OrganizationId organizationId, LeadBulkActionInput input,
        CancellationToken cancellationToken);
}
