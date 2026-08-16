using DriveOS.Modules.CRM.Domain.Leads;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.CRM.Application.Dashboard.GetDashboard;

public sealed record CrmDashboardResponse(
    DateTimeOffset GeneratedAtUtc,
    string Scope,
    Guid? BranchId,
    CrmDashboardKpis Kpis,
    IReadOnlyList<CrmDashboardPriority> Priorities,
    IReadOnlyList<CrmDashboardPipelineStage> Pipeline,
    IReadOnlyList<CrmDashboardActivity> RecentActivities,
    IReadOnlyList<CrmDashboardTask> UpcomingTasks,
    IReadOnlyList<CrmDashboardAppointment> UpcomingAppointments,
    IReadOnlyList<CrmDashboardSource> Sources,
    IReadOnlyList<CrmDashboardBranchConversion> ConversionsByBranch,
    IReadOnlyList<CrmDashboardInactiveLead> InactiveLeads,
    IReadOnlyList<string> UnavailableWidgets
)
{
    public IReadOnlyList<CrmDashboardBranchScope> AvailableBranches { get; init; } = [];
    public IReadOnlyList<CrmDashboardOrganizationScope> IncludedOrganizations { get; init; } = [];
}

public sealed record CrmDashboardKpis(
    int NewLeads,
    int ToContact,
    int OverdueFollowUps,
    int? UpcomingAppointments,
    int PendingOffers,
    decimal ConversionRate,
    double? FirstContactDelayHours,
    decimal? PipelineValue,
    string? PipelineCurrency,
    int UnassignedLeads,
    int? ExpiringOpportunities
);

public sealed record CrmDashboardPriority(
    Guid LeadId,
    string FirstName,
    string LastName,
    string Kind,
    string Label,
    DateTimeOffset? DueAtUtc
);

public sealed record CrmDashboardPipelineStage(string Status, int Count);

public sealed record CrmDashboardActivity(
    Guid Id,
    Guid LeadId,
    string FirstName,
    string LastName,
    string Type,
    string Direction,
    string Subject,
    DateTimeOffset OccurredAtUtc
);

public sealed record CrmDashboardTask(
    Guid Id,
    Guid LeadId,
    string FirstName,
    string LastName,
    string Type,
    string Title,
    DateTimeOffset DueAtUtc,
    bool IsOverdue
);

public sealed record CrmDashboardAppointment(
    Guid Id,
    Guid LeadId,
    string FirstName,
    string LastName,
    string Type,
    string DeliveryMode,
    string Status,
    DateTimeOffset StartsAtUtc,
    DateTimeOffset EndsAtUtc,
    string? LocationDetails
);

public sealed record CrmDashboardSource(string Source, int Count);

public sealed record CrmDashboardBranchConversion(Guid? BranchId, int Converted, int Total);

public sealed record CrmDashboardBranchScope(Guid Id, string Name, string Code, bool IsPrimary);

public sealed record CrmDashboardOrganizationScope(Guid Id, string Name, bool IsNetwork);

public sealed record CrmDashboardInactiveLead(
    Guid LeadId,
    string FirstName,
    string LastName,
    string Status,
    DateTimeOffset LastInteractionAtUtc,
    int InactiveDays
);

public sealed record CrmDashboardFilters(
    DateTimeOffset? FromUtc,
    DateTimeOffset? ToUtc,
    UserId? AssignedAdvisorId,
    LeadSourceType? Source,
    LeadStatus? Status
);
