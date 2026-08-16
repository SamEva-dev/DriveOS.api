using DriveOS.Modules.CRM.Domain.Leads;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.CRM.Application.Leads.SavedViews;

public sealed record SavedLeadViewDto(
    Guid Id,
    string Name,
    string FiltersJson,
    string SortJson,
    string ColumnsJson,
    SavedLeadViewScope Scope,
    Guid? BranchId,
    bool IsDefault,
    Guid OwnerUserId,
    bool CanEdit
);

public sealed record SaveLeadViewInput(
    Guid? Id,
    string Name,
    string FiltersJson,
    string SortJson,
    string ColumnsJson,
    SavedLeadViewScope Scope,
    Guid? BranchId,
    bool IsDefault
);

public interface ISavedLeadViewService
{
    Task<IReadOnlyList<SavedLeadViewDto>> ListAsync(
        OrganizationId organizationId,
        UserId userId,
        IReadOnlySet<Guid> branchIds,
        CancellationToken cancellationToken
    );
    Task<SavedLeadViewDto?> SaveAsync(
        OrganizationId organizationId,
        UserId userId,
        SaveLeadViewInput input,
        bool canShare,
        CancellationToken cancellationToken
    );
    Task<bool> DeleteAsync(
        OrganizationId organizationId,
        UserId userId,
        Guid id,
        CancellationToken cancellationToken
    );
}
