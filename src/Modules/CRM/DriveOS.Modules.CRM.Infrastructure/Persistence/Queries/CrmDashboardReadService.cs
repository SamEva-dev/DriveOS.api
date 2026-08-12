using DriveOS.Modules.CRM.Application.Dashboard.GetDashboard;
using DriveOS.Modules.CRM.Domain.Leads;
using DriveOS.Modules.CRM.Domain.Tasks;
using DriveOS.Modules.CRM.Domain.Assessments;
using DriveOS.Modules.CRM.Domain.Offers;
using DriveOS.Modules.CRM.Domain.Conversions;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;

namespace DriveOS.Modules.CRM.Infrastructure.Persistence.Queries;

internal sealed class CrmDashboardReadService(CrmDbContext context)
    : ICrmDashboardReadService
{
    public async Task<CrmDashboardResponse> GetAsync(IReadOnlyCollection<OrganizationId> organizationIds,
        Guid? branchId, CrmDashboardFilters filters, DateTimeOffset nowUtc, CancellationToken ct)
    {
        OrganizationId[] scopedOrganizationIds = organizationIds.Distinct().ToArray();
        IQueryable<Lead> leads = context.Leads.AsNoTracking()
            .Where(x => scopedOrganizationIds.Contains(x.OrganizationId));
        if (branchId.HasValue)
        {
            var scopedBranchId = new BranchId(branchId.Value);
            leads = leads.Where(x => x.BranchId == scopedBranchId);
        }
        if (filters.FromUtc.HasValue)
            leads = leads.Where(x => x.CreatedAtUtc >= filters.FromUtc.Value);
        if (filters.ToUtc.HasValue)
            leads = leads.Where(x => x.CreatedAtUtc < filters.ToUtc.Value);
        if (filters.AssignedAdvisorId.HasValue)
            leads = leads.Where(x => x.AssignedAdvisorId == filters.AssignedAdvisorId.Value);
        if (filters.Source.HasValue)
            leads = leads.Where(x => x.Source.Type == filters.Source.Value);
        if (filters.Status.HasValue)
            leads = leads.Where(x => x.Status == filters.Status.Value);

        var leadRows = await leads.Select(x => new
        {
            Id = x.Id.Value,
            BranchId = x.BranchId.HasValue ? x.BranchId.Value.Value : (Guid?)null,
            x.Identity.FirstName,
            x.Identity.LastName,
            x.Status,
            Source = x.Source.Type,
            x.AssignedAdvisorId,
            x.CreatedAtUtc,
            x.LastModifiedAtUtc,
            x.ConvertedAtUtc,
            x.ResumeAtUtc
        }).ToListAsync(ct);

        LeadId[] scopedLeadIds = leadRows.Select(x => new LeadId(x.Id)).ToArray();
        var activities = await context.Activities.AsNoTracking()
            .Where(x => scopedOrganizationIds.Contains(x.OrganizationId) && scopedLeadIds.Contains(x.LeadId))
            .OrderByDescending(x => x.OccurredAtUtc)
            .Select(x => new {
                Id = x.Id.Value,
                LeadId = x.LeadId.Value,
                x.Type,
                x.Direction,
                x.Subject,
                x.OccurredAtUtc
            })
            .ToListAsync(ct);
        var tasks = await context.Tasks.AsNoTracking()
            .Where(x => scopedOrganizationIds.Contains(x.OrganizationId) && scopedLeadIds.Contains(x.LeadId)
                && x.Status == CrmTaskStatus.Pending)
            .OrderBy(x => x.DueAtUtc)
            .Select(x => new { Id = x.Id.Value, LeadId = x.LeadId.Value, x.Type, x.Title, x.DueAtUtc })
            .ToListAsync(ct);
        IQueryable<AssessmentAppointment> appointmentsQuery =
    context.AssessmentAppointments
        .AsNoTracking()
        .Where(appointment =>
            scopedOrganizationIds.Contains(appointment.OrganizationId)
            && appointment.Status != AssessmentAppointmentStatus.Completed
            && appointment.Status != AssessmentAppointmentStatus.Cancelled
            && appointment.Status != AssessmentAppointmentStatus.NoShow
            && appointment.StartsAtUtc >= nowUtc);

        if (branchId.HasValue)
        {
            var scopedBranchId = new BranchId(branchId.Value);

            appointmentsQuery = appointmentsQuery.Where(
                appointment => appointment.BranchId == scopedBranchId);
        }

        var upcomingAppointmentRows = await appointmentsQuery
            .OrderBy(x => x.StartsAtUtc)
            .Take(10)
            .Select(x => new
            {
                Id = x.Id.Value,
                LeadId = x.LeadId.Value,
                x.Type,
                x.DeliveryMode,
                x.Status,
                x.StartsAtUtc,
                x.EndsAtUtc,
                x.LocationDetails
            })
            .ToListAsync(ct);
        int upcomingAppointments = await appointmentsQuery.CountAsync(ct);

        IQueryable<CommercialOffer> offersQuery =
    context.CommercialOffers
        .AsNoTracking()
        .Where(offer =>
            scopedOrganizationIds.Contains(offer.OrganizationId)
            && scopedLeadIds.Contains(offer.LeadId));

        if (branchId.HasValue)
        {
            var scopedBranchId = new BranchId(branchId.Value);

            offersQuery = offersQuery.Where(
                offer => offer.BranchId == scopedBranchId);
        }

        var latestOffers = await offersQuery
            .GroupBy(offer => offer.LeadId)
            .Select(group => group
                .OrderByDescending(offer => offer.Version)
                .Select(offer => new
                {
                    LeadId = offer.LeadId.Value,
                    offer.Status,
                    offer.Amount,
                    offer.Currency,
                    offer.ValidUntilUtc
                })
                .First())
            .ToListAsync(ct);

        var pendingOffers = latestOffers.Where(x => x.Status == CommercialOfferStatus.Sent
            && x.ValidUntilUtc >= nowUtc).ToArray();
        string[] currencies = pendingOffers.Select(x => x.Currency).Distinct().ToArray();
        decimal? pipelineValue = currencies.Length <= 1 ? pendingOffers.Sum(x => x.Amount) : null;
        int expiringOffers = pendingOffers.Count(x => x.ValidUntilUtc <= nowUtc.AddDays(7));

        LeadId[] failedConversionLeadIds = await context.LeadConversions.AsNoTracking()
            .Where(x => scopedOrganizationIds.Contains(x.OrganizationId)
                && scopedLeadIds.Contains(x.LeadId)
                && x.Status == LeadConversionStatus.Failed)
            .Select(x => x.LeadId)
            .ToArrayAsync(ct);
        LeadId[] assessmentToValidateLeadIds = await context.AssessmentSessions.AsNoTracking()
            .Where(x => scopedOrganizationIds.Contains(x.OrganizationId)
                && scopedLeadIds.Contains(x.LeadId)
                && (x.ResultStatus == AssessmentResultStatus.Draft
                    || x.ResultStatus == AssessmentResultStatus.CorrectionRequested))
            .Select(x => x.LeadId)
            .Distinct()
            .ToArrayAsync(ct);

        var names = leadRows.ToDictionary(x => x.Id, x => (x.FirstName, x.LastName));
        var firstActivities = activities.GroupBy(x => x.LeadId)
            .ToDictionary(x => x.Key, x => x.Min(a => a.OccurredAtUtc));
        var lastActivities = activities.GroupBy(x => x.LeadId)
            .ToDictionary(x => x.Key, x => x.Max(a => a.OccurredAtUtc));

        int total = leadRows.Count;
        int converted = leadRows.Count(x => x.ConvertedAtUtc.HasValue);
        double? firstContactDelay = firstActivities.Count == 0 ? null : Math.Round(
            leadRows.Where(x => firstActivities.ContainsKey(x.Id))
                .Average(x => Math.Max(0, (firstActivities[x.Id] - x.CreatedAtUtc).TotalHours)), 1);

        var priorities = tasks.Where(x => x.DueAtUtc < nowUtc).Take(5)
            .Select(x => new CrmDashboardPriority(x.LeadId, names[x.LeadId].FirstName,
                names[x.LeadId].LastName, "OverdueTask", x.Title, x.DueAtUtc))
            .ToList();
        foreach (var offer in pendingOffers.Where(x => x.ValidUntilUtc <= nowUtc.AddDays(7))
                     .OrderBy(x => x.ValidUntilUtc))
        {
            if (priorities.Count >= 8 || !names.TryGetValue(offer.LeadId, out var name)) break;
            priorities.Add(new CrmDashboardPriority(offer.LeadId, name.FirstName, name.LastName,
                "OfferExpiring", "Offer expires soon", offer.ValidUntilUtc));
        }
        foreach (LeadId leadId in assessmentToValidateLeadIds)
        {
            if (priorities.Count >= 8 || !names.TryGetValue(leadId.Value, out var name)) break;
            priorities.Add(new CrmDashboardPriority(leadId.Value, name.FirstName, name.LastName,
                "AssessmentToValidate", "Assessment result to validate", null));
        }
        foreach (LeadId leadId in failedConversionLeadIds)
        {
            if (priorities.Count >= 8 || !names.TryGetValue(leadId.Value, out var name)) break;
            priorities.Add(new CrmDashboardPriority(leadId.Value, name.FirstName, name.LastName,
                "ConversionFailed", "Student conversion failed", null));
        }
        foreach (var lead in leadRows.Where(x => x.Status == LeadStatus.Dormant
                     && x.ResumeAtUtc.HasValue && x.ResumeAtUtc.Value <= nowUtc.AddDays(7))
                     .OrderBy(x => x.ResumeAtUtc))
        {
            if (priorities.Count >= 8) break;
            priorities.Add(new CrmDashboardPriority(lead.Id, lead.FirstName, lead.LastName,
                "DormantToWake", "Prospect to reactivate", lead.ResumeAtUtc));
        }
        if (priorities.Count < 8)
        {
            priorities.AddRange(leadRows.Where(x => x.Status == LeadStatus.New)
                .OrderBy(x => x.CreatedAtUtc).Take(8 - priorities.Count)
                .Select(x => new CrmDashboardPriority(x.Id, x.FirstName, x.LastName,
                    "LeadToContact", "Contact required", null)));
        }

        DateTimeOffset inactiveBefore = nowUtc.AddDays(-14);
        var inactive = leadRows.Where(x => x.Status is not LeadStatus.Won and not LeadStatus.Lost)
            .Select(x => new {
                Lead = x,
                Last = lastActivities.GetValueOrDefault(x.Id,
                x.LastModifiedAtUtc ?? x.CreatedAtUtc)
            })
            .Where(x => x.Last < inactiveBefore).OrderBy(x => x.Last).Take(10)
            .Select(x => new CrmDashboardInactiveLead(x.Lead.Id, x.Lead.FirstName,
                x.Lead.LastName, x.Lead.Status.ToString(), x.Last,
                Math.Max(0, (int)(nowUtc - x.Last).TotalDays))).ToArray();

        return new CrmDashboardResponse(
            nowUtc, branchId.HasValue ? "Branch"
                : scopedOrganizationIds.Length > 1 ? "Network" : "Organization", branchId,
            new CrmDashboardKpis(
                leadRows.Count(x => x.Status == LeadStatus.New),
                leadRows.Count(x => x.Status is LeadStatus.New or LeadStatus.Contacted),
                tasks.Count(x => x.DueAtUtc < nowUtc),
                upcomingAppointments,
                pendingOffers.Length,
                total == 0 ? 0 : Math.Round(converted * 100m / total, 1),
                firstContactDelay, pipelineValue, currencies.Length == 1 ? currencies[0] : null,
                leadRows.Count(x => x.AssignedAdvisorId == null), expiringOffers),
            priorities,
            Enum.GetValues<LeadStatus>().Select(status => new CrmDashboardPipelineStage(
                status.ToString(), leadRows.Count(x => x.Status == status))).ToArray(),
            activities.Take(10).Select(x => new CrmDashboardActivity(x.Id, x.LeadId,
                names[x.LeadId].FirstName, names[x.LeadId].LastName, x.Type.ToString(),
                x.Direction.ToString(), x.Subject, x.OccurredAtUtc)).ToArray(),
            tasks.Take(10).Select(x => new CrmDashboardTask(x.Id, x.LeadId,
                names[x.LeadId].FirstName, names[x.LeadId].LastName, x.Type.ToString(),
                x.Title, x.DueAtUtc, x.DueAtUtc < nowUtc)).ToArray(),
            upcomingAppointmentRows.Select(x => new CrmDashboardAppointment(
                x.Id, x.LeadId, names[x.LeadId].FirstName, names[x.LeadId].LastName,
                x.Type.ToString(), x.DeliveryMode.ToString(), x.Status.ToString(),
                x.StartsAtUtc, x.EndsAtUtc, x.LocationDetails)).ToArray(),
            leadRows.GroupBy(x => x.Source).OrderByDescending(x => x.Count())
                .Select(x => new CrmDashboardSource(x.Key.ToString(), x.Count())).ToArray(),
            leadRows.GroupBy(x => x.BranchId).Select(x => new CrmDashboardBranchConversion(
                x.Key, x.Count(l => l.ConvertedAtUtc.HasValue), x.Count())).ToArray(),
            inactive,
            currencies.Length > 1 ? ["financialCurrencyAggregation"] : []);
    }
}
