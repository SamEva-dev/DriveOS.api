using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.CRM.Domain.Leads;

public sealed class SavedLeadView
{
    private SavedLeadView() { }

    public SavedLeadView(Guid id, OrganizationId organizationId, UserId ownerUserId,
        string name, string filtersJson, string sortJson, string columnsJson,
        SavedLeadViewScope scope, Guid? branchId, bool isDefault, DateTimeOffset nowUtc)
    {
        Id = id; OrganizationId = organizationId; OwnerUserId = ownerUserId;
        Name = name.Trim(); FiltersJson = filtersJson; SortJson = sortJson;
        ColumnsJson = columnsJson; Scope = scope; BranchId = branchId;
        IsDefault = isDefault; CreatedAtUtc = nowUtc; LastModifiedAtUtc = nowUtc;
    }

    public Guid Id { get; private set; }
    public OrganizationId OrganizationId { get; private set; }
    public UserId OwnerUserId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string FiltersJson { get; private set; } = "{}";
    public string SortJson { get; private set; } = "{}";
    public string ColumnsJson { get; private set; } = "[]";
    public SavedLeadViewScope Scope { get; private set; }
    public Guid? BranchId { get; private set; }
    public bool IsDefault { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public DateTimeOffset LastModifiedAtUtc { get; private set; }

    public void Update(string name, string filtersJson, string sortJson, string columnsJson,
        SavedLeadViewScope scope, Guid? branchId, bool isDefault, DateTimeOffset nowUtc)
    {
        Name = name.Trim(); FiltersJson = filtersJson; SortJson = sortJson;
        ColumnsJson = columnsJson; Scope = scope; BranchId = branchId;
        IsDefault = isDefault; LastModifiedAtUtc = nowUtc;
    }

    public void ClearDefault() => IsDefault = false;
}

public enum SavedLeadViewScope { Private = 0, Branch = 1, Organization = 2 }
