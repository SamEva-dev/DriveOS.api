using DriveOS.Modules.ProfessionalMarketplace.Domain.StudentAssignments;
using DriveOS.Modules.ProfessionalMarketplace.Domain.ProfessionalProfiles;
using DriveOS.Modules.ProfessionalMarketplace.Domain.Invitations;
using DriveOS.Modules.ProfessionalMarketplace.Domain.Disputes;
using DriveOS.Modules.ProfessionalMarketplace.Domain.Applications;
using DriveOS.Modules.ProfessionalMarketplace.Application.Dashboard;
using DriveOS.Modules.ProfessionalMarketplace.Domain.Compliance;
using DriveOS.Modules.ProfessionalMarketplace.Domain.Engagements;
using DriveOS.Modules.ProfessionalMarketplace.Domain.Invoices;
using DriveOS.Modules.ProfessionalMarketplace.Domain.Missions;
using DriveOS.Modules.ProfessionalMarketplace.Domain.Reviews;
using DriveOS.Modules.ProfessionalMarketplace.Domain.ServiceEntries;
using DriveOS.Modules.ProfessionalMarketplace.Domain.ServiceStatements;
using DriveOS.Modules.ProfessionalMarketplace.Infrastructure.Persistence;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;

namespace DriveOS.Modules.ProfessionalMarketplace.Infrastructure.Read;

internal sealed class MarketplaceDashboardReadService(
    ProfessionalMarketplaceDbContext db) : IMarketplaceDashboardReadService
{
    public Task<OrganizationMarketplaceDashboardResponse> GetOrganizationAsync(
        OrganizationId organizationId,DateOnly from,DateOnly to,CancellationToken cancellationToken=default)=>
        BuildOrganizationAsync(organizationId,from,to,cancellationToken);

    public Task<ProfessionalMarketplaceDashboardResponse> GetProfessionalAsync(
        ProfessionalProfileId professionalProfileId,DateOnly from,DateOnly to,CancellationToken cancellationToken=default)=>
        BuildProfessionalAsync(professionalProfileId,from,to,cancellationToken);

    private async Task<OrganizationMarketplaceDashboardResponse> BuildOrganizationAsync(
        OrganizationId organizationId,DateOnly from,DateOnly to,CancellationToken ct)
    {
        DateTimeOffset fromUtc=new(from.Year,from.Month,from.Day,0,0,0,TimeSpan.Zero);
        DateTimeOffset toUtc=new(to.Year,to.Month,to.Day,23,59,59,TimeSpan.Zero);
        DateOnly today=DateOnly.FromDateTime(DateTime.UtcNow);
        DateOnly credentialAlertDate=today.AddDays(30);

        int activeEngagements=await db.ProfessionalEngagements.AsNoTracking()
            .CountAsync(x=>x.OrganizationId==organizationId&&x.Status==ProfessionalEngagementStatus.Active,ct);

        int activeMissions=await db.ProfessionalMissions.AsNoTracking()
            .CountAsync(x=>x.OrganizationId==organizationId&&x.Status==ProfessionalMissionStatus.Active,ct);

        int pendingEntries=await db.ServiceEntries.AsNoTracking()
            .CountAsync(x=>x.OrganizationId==organizationId&&
                (x.Status==ServiceEntryStatus.Submitted||x.Status==ServiceEntryStatus.Recorded),ct);

        int disputedEntries=await db.ServiceEntries.AsNoTracking()
            .CountAsync(x=>x.OrganizationId==organizationId&&x.Status==ServiceEntryStatus.Disputed,ct);

        int pendingStatements=await db.ServiceStatements.AsNoTracking()
            .CountAsync(x=>x.ClientOrganizationId==organizationId&&
                (x.Status==ServiceStatementStatus.Submitted||x.Status==ServiceStatementStatus.UnderReview||
                 x.Status==ServiceStatementStatus.PartiallyApproved||x.Status==ServiceStatementStatus.Disputed),ct);

        int pendingInvoices=await db.ProfessionalInvoices.AsNoTracking()
            .CountAsync(x=>x.ClientOrganizationId==organizationId&&
                (x.Status==ProfessionalInvoiceStatus.Draft||x.Status==ProfessionalInvoiceStatus.Validated||
                 x.Status==ProfessionalInvoiceStatus.Requested)&&x.PaymentStatus!=ProfessionalInvoicePaymentStatus.Paid,ct);

        int scheduledPayments=await db.ProfessionalInvoices.AsNoTracking()
            .CountAsync(x=>x.ClientOrganizationId==organizationId&&x.PaymentStatus==ProfessionalInvoicePaymentStatus.Scheduled,ct);

        int failedPayments=await db.ProfessionalInvoices.AsNoTracking()
            .CountAsync(x=>x.ClientOrganizationId==organizationId&&x.PaymentStatus==ProfessionalInvoicePaymentStatus.Failed,ct);

        int paidInvoices=await db.ProfessionalInvoices.AsNoTracking()
            .CountAsync(x=>x.ClientOrganizationId==organizationId&&x.PaymentStatus==ProfessionalInvoicePaymentStatus.Paid&&
                x.FinanceStatusSyncedAtUtc>=fromUtc&&x.FinanceStatusSyncedAtUtc<=toUtc,ct);

        int openReports=await db.ProfessionalReviewReports.AsNoTracking()
            .CountAsync(x=>x.OrganizationId==organizationId&&x.Status==ProfessionalReviewReportStatus.Open,ct);

        Guid[] organizationProfileIds=await db.ProfessionalEngagements.AsNoTracking()
            .Where(x=>x.OrganizationId==organizationId&&x.Status==ProfessionalEngagementStatus.Active)
            .Select(x=>x.ProfessionalProfileId.Value)
            .Distinct()
            .ToArrayAsync(ct);

        int expiringCredentials=organizationProfileIds.Length==0?0:
            await db.ProfessionalCredentials.AsNoTracking().CountAsync(x=>
                organizationProfileIds.Contains(x.ProfessionalProfileId.Value)&&
                x.Status==ProfessionalCredentialStatus.Verified&&
                x.ValidUntil!=null&&x.ValidUntil>=today&&x.ValidUntil<=credentialAlertDate,ct);

        decimal? avgValidation=await AverageValidationDelayOrganizationAsync(organizationId,fromUtc,toUtc,ct);
        decimal? avgPayment=await AveragePaymentDelayOrganizationAsync(organizationId,fromUtc,toUtc,ct);

        var alerts=new List<MarketplaceDashboardAlert>();
        if(disputedEntries>0)alerts.Add(new("service-entries.disputed","warning","professionalMarketplace.dashboard.alerts.disputedServiceEntries",null,"ServiceEntry",null));
        if(failedPayments>0)alerts.Add(new("payments.failed","critical","professionalMarketplace.dashboard.alerts.failedPayments",null,"ProfessionalInvoice",null));
        if(expiringCredentials>0)alerts.Add(new("credentials.expiring","warning","professionalMarketplace.dashboard.alerts.expiringCredentials",null,"ProfessionalCredential",credentialAlertDate));
        if(pendingEntries>0)alerts.Add(new("service-entries.pending","info","professionalMarketplace.dashboard.alerts.pendingServiceEntries",null,"ServiceEntry",null));

        MarketplaceDashboardAdvancedKpis advanced=await BuildAdvancedOrganizationAsync(
            organizationId,from,to,fromUtc,toUtc,ct);
        if(advanced.OverdueInvoices>0)
            alerts.Add(new("invoices.overdue","critical","professionalMarketplace.dashboard.alerts.overdueInvoices",null,"ProfessionalInvoice",null));
        if(advanced.OpenDisputes>0)
            alerts.Add(new("disputes.open","warning","professionalMarketplace.dashboard.alerts.openDisputes",null,"ServiceDispute",null));

        return new(
            new MarketplaceDashboardKpis(activeEngagements,activeMissions,pendingEntries,disputedEntries,pendingStatements,
                pendingInvoices,scheduledPayments,failedPayments,paidInvoices,openReports,expiringCredentials,avgValidation,avgPayment),
            alerts.ToArray(),
            advanced);
    }

    private async Task<ProfessionalMarketplaceDashboardResponse> BuildProfessionalAsync(
        ProfessionalProfileId profileId,DateOnly from,DateOnly to,CancellationToken ct)
    {
        DateTimeOffset fromUtc=new(from.Year,from.Month,from.Day,0,0,0,TimeSpan.Zero);
        DateTimeOffset toUtc=new(to.Year,to.Month,to.Day,23,59,59,TimeSpan.Zero);
        DateOnly today=DateOnly.FromDateTime(DateTime.UtcNow);
        DateOnly credentialAlertDate=today.AddDays(30);

        int activeEngagements=await db.ProfessionalEngagements.AsNoTracking()
            .CountAsync(x=>x.ProfessionalProfileId==profileId&&x.Status==ProfessionalEngagementStatus.Active,ct);
        int activeMissions=await db.ProfessionalMissions.AsNoTracking()
            .CountAsync(x=>x.ProfessionalProfileId==profileId&&x.Status==ProfessionalMissionStatus.Active,ct);
        int pendingEntries=await db.ServiceEntries.AsNoTracking()
            .CountAsync(x=>x.ProfessionalProfileId==profileId&&x.Status==ServiceEntryStatus.Recorded,ct);
        int disputedEntries=await db.ServiceEntries.AsNoTracking()
            .CountAsync(x=>x.ProfessionalProfileId==profileId&&x.Status==ServiceEntryStatus.Disputed,ct);
        int pendingStatements=await db.ServiceStatements.AsNoTracking()
            .CountAsync(x=>x.ProfessionalProfileId==profileId&&
                (x.Status==ServiceStatementStatus.Draft||x.Status==ServiceStatementStatus.Submitted||
                 x.Status==ServiceStatementStatus.UnderReview||x.Status==ServiceStatementStatus.PartiallyApproved||
                 x.Status==ServiceStatementStatus.Disputed),ct);
        int pendingInvoices=await db.ProfessionalInvoices.AsNoTracking()
            .CountAsync(x=>x.ProfessionalProfileId==profileId&&x.PaymentStatus!=ProfessionalInvoicePaymentStatus.Paid&&
                x.Status!=ProfessionalInvoiceStatus.Cancelled,ct);
        int scheduledPayments=await db.ProfessionalInvoices.AsNoTracking()
            .CountAsync(x=>x.ProfessionalProfileId==profileId&&x.PaymentStatus==ProfessionalInvoicePaymentStatus.Scheduled,ct);
        int failedPayments=await db.ProfessionalInvoices.AsNoTracking()
            .CountAsync(x=>x.ProfessionalProfileId==profileId&&x.PaymentStatus==ProfessionalInvoicePaymentStatus.Failed,ct);
        int paidInvoices=await db.ProfessionalInvoices.AsNoTracking()
            .CountAsync(x=>x.ProfessionalProfileId==profileId&&x.PaymentStatus==ProfessionalInvoicePaymentStatus.Paid&&
                x.FinanceStatusSyncedAtUtc>=fromUtc&&x.FinanceStatusSyncedAtUtc<=toUtc,ct);
        int openReports=await db.ProfessionalReviews.AsNoTracking()
            .Where(x=>x.ProfessionalProfileId==profileId)
            .Join(db.ProfessionalReviewReports.AsNoTracking(),r=>r.Id,rr=>rr.ReviewId,(r,rr)=>rr)
            .CountAsync(x=>x.Status==ProfessionalReviewReportStatus.Open,ct);
        int expiringCredentials=await db.ProfessionalCredentials.AsNoTracking()
            .CountAsync(x=>x.ProfessionalProfileId==profileId&&x.Status==ProfessionalCredentialStatus.Verified&&
                x.ValidUntil!=null&&x.ValidUntil>=today&&x.ValidUntil<=credentialAlertDate,ct);

        decimal? avgValidation=await AverageValidationDelayProfessionalAsync(profileId,fromUtc,toUtc,ct);
        decimal? avgPayment=await AveragePaymentDelayProfessionalAsync(profileId,fromUtc,toUtc,ct);

        var alerts=new List<MarketplaceDashboardAlert>();
        if(disputedEntries>0)alerts.Add(new("service-entries.disputed","warning","professionalMarketplace.dashboard.alerts.disputedServiceEntries",null,"ServiceEntry",null));
        if(failedPayments>0)alerts.Add(new("payments.failed","critical","professionalMarketplace.dashboard.alerts.failedPayments",null,"ProfessionalInvoice",null));
        if(expiringCredentials>0)alerts.Add(new("credentials.expiring","warning","professionalMarketplace.dashboard.alerts.expiringCredentials",null,"ProfessionalCredential",credentialAlertDate));
        if(pendingInvoices>0)alerts.Add(new("invoices.pending","info","professionalMarketplace.dashboard.alerts.pendingInvoices",null,"ProfessionalInvoice",null));

        MarketplaceDashboardAdvancedKpis advanced=await BuildAdvancedProfessionalAsync(
            profileId,from,to,fromUtc,toUtc,ct);
        if(advanced.OverdueInvoices>0)
            alerts.Add(new("invoices.overdue","critical","professionalMarketplace.dashboard.alerts.overdueInvoices",null,"ProfessionalInvoice",null));
        if(advanced.OpenDisputes>0)
            alerts.Add(new("disputes.open","warning","professionalMarketplace.dashboard.alerts.openDisputes",null,"ServiceDispute",null));

        return new(
            new MarketplaceDashboardKpis(activeEngagements,activeMissions,pendingEntries,disputedEntries,pendingStatements,
                pendingInvoices,scheduledPayments,failedPayments,paidInvoices,openReports,expiringCredentials,avgValidation,avgPayment),
            alerts.ToArray(),
            advanced);
    }

    private async Task<decimal?> AverageValidationDelayOrganizationAsync(
        OrganizationId org,DateTimeOffset fromUtc,DateTimeOffset toUtc,CancellationToken ct)
    {
        var rows=await db.ServiceEntries.AsNoTracking()
            .Where(x=>x.OrganizationId==org&&x.SubmittedAtUtc!=null&&x.ReviewedAtUtc!=null&&
                x.ReviewedAtUtc>=fromUtc&&x.ReviewedAtUtc<=toUtc)
            .Select(x=>new{x.SubmittedAtUtc,x.ReviewedAtUtc})
            .ToListAsync(ct);

        return rows.Count==0?null:decimal.Round((decimal)rows.Average(x=>(x.ReviewedAtUtc!.Value-x.SubmittedAtUtc!.Value).TotalHours),2);
    }

    private async Task<decimal?> AverageValidationDelayProfessionalAsync(
        ProfessionalProfileId profileId,DateTimeOffset fromUtc,DateTimeOffset toUtc,CancellationToken ct)
    {
        var rows=await db.ServiceEntries.AsNoTracking()
            .Where(x=>x.ProfessionalProfileId==profileId&&x.SubmittedAtUtc!=null&&x.ReviewedAtUtc!=null&&
                x.ReviewedAtUtc>=fromUtc&&x.ReviewedAtUtc<=toUtc)
            .Select(x=>new{x.SubmittedAtUtc,x.ReviewedAtUtc})
            .ToListAsync(ct);

        return rows.Count==0?null:decimal.Round((decimal)rows.Average(x=>(x.ReviewedAtUtc!.Value-x.SubmittedAtUtc!.Value).TotalHours),2);
    }

    private async Task<decimal?> AveragePaymentDelayOrganizationAsync(
        OrganizationId org,DateTimeOffset fromUtc,DateTimeOffset toUtc,CancellationToken ct)
    {
        var rows=await db.ProfessionalInvoices.AsNoTracking()
            .Where(x=>x.ClientOrganizationId==org&&x.PaymentStatus==ProfessionalInvoicePaymentStatus.Paid&&
                x.RequestedAtUtc!=null&&x.FinanceStatusSyncedAtUtc!=null&&
                x.FinanceStatusSyncedAtUtc>=fromUtc&&x.FinanceStatusSyncedAtUtc<=toUtc)
            .Select(x=>new{x.RequestedAtUtc,x.FinanceStatusSyncedAtUtc})
            .ToListAsync(ct);

        return rows.Count==0?null:decimal.Round((decimal)rows.Average(x=>(x.FinanceStatusSyncedAtUtc!.Value-x.RequestedAtUtc!.Value).TotalHours),2);
    }

    private async Task<decimal?> AveragePaymentDelayProfessionalAsync(
        ProfessionalProfileId profileId,DateTimeOffset fromUtc,DateTimeOffset toUtc,CancellationToken ct)
    {
        var rows=await db.ProfessionalInvoices.AsNoTracking()
            .Where(x=>x.ProfessionalProfileId==profileId&&x.PaymentStatus==ProfessionalInvoicePaymentStatus.Paid&&
                x.RequestedAtUtc!=null&&x.FinanceStatusSyncedAtUtc!=null&&
                x.FinanceStatusSyncedAtUtc>=fromUtc&&x.FinanceStatusSyncedAtUtc<=toUtc)
            .Select(x=>new{x.RequestedAtUtc,x.FinanceStatusSyncedAtUtc})
            .ToListAsync(ct);

        return rows.Count==0?null:decimal.Round((decimal)rows.Average(x=>(x.FinanceStatusSyncedAtUtc!.Value-x.RequestedAtUtc!.Value).TotalHours),2);
    }

    private async Task<MarketplaceDashboardAdvancedKpis> BuildAdvancedOrganizationAsync(
        OrganizationId org,DateOnly from,DateOnly to,DateTimeOffset fromUtc,DateTimeOffset toUtc,CancellationToken ct)
    {
        var invitations=db.FreelanceInvitations.AsNoTracking()
            .Where(x=>x.ClientOrganizationId==org&&x.SentAtUtc>=fromUtc&&x.SentAtUtc<=toUtc);
        int invitationsSent=await invitations.CountAsync(ct);
        int invitationsAccepted=await invitations.CountAsync(x=>x.Status==FreelanceInvitationStatus.Accepted,ct);

        Guid[] acceptedProfileIds=await invitations
            .Where(x=>x.Status==FreelanceInvitationStatus.Accepted&&x.ProfessionalProfileId!=null)
            .Select(x=>x.ProfessionalProfileId!.Value.Value).Distinct().ToArrayAsync(ct);
        int invitationsActivated=acceptedProfileIds.Length==0?0:
            await db.ProfessionalProfiles.AsNoTracking().CountAsync(x=>
                acceptedProfileIds.Contains(x.Id.Value)&&x.Status==ProfessionalProfileStatus.Active,ct);

        var decidedApps=db.ProfessionalApplications.AsNoTracking()
            .Where(x=>x.OrganizationId==org&&x.DecidedAtUtc>=fromUtc&&x.DecidedAtUtc<=toUtc);
        int applicationsDecided=await decidedApps.CountAsync(ct);
        int applicationsAccepted=await decidedApps.CountAsync(x=>x.Status==ProfessionalApplicationStatus.Accepted,ct);

        Guid[] profileIds=await db.ProfessionalEngagements.AsNoTracking()
            .Where(x=>x.OrganizationId==org)
            .Select(x=>x.ProfessionalProfileId.Value).Distinct().ToArrayAsync(ct);
        int profilesInScope=profileIds.Length;
        int completeProfiles=0;
        if(profilesInScope>0)
        {
            var scopedProfiles=await db.ProfessionalProfiles.AsNoTracking()
                .Where(x=>profileIds.Contains(x.Id.Value)).ToListAsync(ct);
            completeProfiles=scopedProfiles.Count(x=>x.IsProfileComplete);
        }

        decimal? avgDocumentValidation=await AverageDocumentValidationDelayAsync(profileIds,fromUtc,toUtc,ct);

        int contracts=await db.ProfessionalEngagements.AsNoTracking().CountAsync(x=>
            x.OrganizationId==org&&x.ContractPrepared&&x.StartsOn<=to&&x.EndsOn>=from,ct);

        decimal plannedMinutes=await db.ProfessionalMissions.AsNoTracking()
            .Where(x=>x.OrganizationId==org&&x.StartsOn<=to&&x.EndsOn>=from&&
                x.Status!=ProfessionalMissionStatus.Draft&&x.Status!=ProfessionalMissionStatus.Declined&&x.Status!=ProfessionalMissionStatus.Cancelled)
            .SumAsync(x=>(decimal?)(x.EstimatedMinutes??0),ct)??0m;

        decimal realizedMinutes=await db.ServiceEntries.AsNoTracking()
            .Where(x=>x.OrganizationId==org&&x.ServiceDate>=from&&x.ServiceDate<=to&&x.Status==ServiceEntryStatus.Approved)
            .SumAsync(x=>(decimal?)x.QuantityMinutes,ct)??0m;

        int cancelledMissions=await db.ProfessionalMissions.AsNoTracking().CountAsync(x=>
            x.OrganizationId==org&&x.Status==ProfessionalMissionStatus.Cancelled&&
            x.CancelledAtUtc>=fromUtc&&x.CancelledAtUtc<=toUtc,ct);

        int students=await db.ProfessionalStudentAssignments.AsNoTracking()
            .Where(x=>x.OrganizationId==org&&x.StartsOn<=to&&x.EndsOn>=from)
            .Select(x=>x.StudentId.Value).Distinct().CountAsync(ct);

        var reviewed=db.ServiceEntries.AsNoTracking().Where(x=>
            x.OrganizationId==org&&x.ReviewedAtUtc>=fromUtc&&x.ReviewedAtUtc<=toUtc);
        int reviewedCount=await reviewed.CountAsync(ct);
        int firstPass=await reviewed.CountAsync(x=>x.Status==ServiceEntryStatus.Approved,ct);

        int overdueInvoices=await db.ProfessionalInvoices.AsNoTracking().CountAsync(x=>
            x.ClientOrganizationId==org&&x.PaymentStatus==ProfessionalInvoicePaymentStatus.Overdue,ct);

        int openDisputes=await db.ServiceDisputes.AsNoTracking().CountAsync(x=>
            x.ClientOrganizationId==org&&x.Status!=ServiceDisputeStatus.Resolved&&x.Status!=ServiceDisputeStatus.Rejected,ct);

        var costRows=await db.ServiceEntries.AsNoTracking()
            .Where(x=>x.OrganizationId==org&&x.ServiceDate>=from&&x.ServiceDate<=to&&x.Status==ServiceEntryStatus.Approved)
            .Select(x=>new{x.QuantityMinutes,x.UnitRate,x.Currency}).ToListAsync(ct);
        string[] currencies=costRows.Select(x=>x.Currency).Distinct().ToArray();
        decimal? avgHourlyCost=costRows.Count==0||currencies.Length!=1?null:
            decimal.Round(costRows.Sum(x=>x.UnitRate*x.QuantityMinutes/60m)/(costRows.Sum(x=>x.QuantityMinutes)/60m),2);

        MarketplaceDashboardAdvancedKpis result=BuildAdvanced(
            invitationsSent,invitationsAccepted,invitationsActivated,applicationsDecided,applicationsAccepted,
            completeProfiles,profilesInScope,avgDocumentValidation,contracts,plannedMinutes,realizedMinutes,
            cancelledMissions,students,reviewedCount,firstPass,overdueInvoices,openDisputes,
            avgHourlyCost,currencies.Length==1?currencies[0]:null);

        int initialIntegrations=await db.ProfessionalEngagements.AsNoTracking().CountAsync(x=>
            x.OrganizationId==org&&x.InitialIntegrationCompletedAtUtc>=fromUtc&&x.InitialIntegrationCompletedAtUtc<=toUtc,ct);
        int reliableRelationships=await db.ProfessionalEngagements.AsNoTracking().CountAsync(x=>
            x.OrganizationId==org&&x.ReliableRelationshipEstablishedAtUtc!=null,ct);

        decimal? invitationActivationDelay=await AverageInvitationActivationDelayOrganizationAsync(
            org,fromUtc,toUtc,ct);

        int relationshipDenominator=await db.ProfessionalEngagements.AsNoTracking().CountAsync(x=>
            x.OrganizationId==org&&x.CreatedAtUtc>=fromUtc&&x.CreatedAtUtc<=toUtc,ct);
        decimal? signedContractRate=Percent(contracts,relationshipDenominator);

        int missionDenominator=await db.ProfessionalMissions.AsNoTracking().CountAsync(x=>
            x.OrganizationId==org&&x.StartsOn<=to&&x.EndsOn>=from&&
            x.Status!=ProfessionalMissionStatus.Draft,ct);
        decimal? cancellationRate=Percent(cancelledMissions,missionDenominator);

        var invoiceRows=await db.ProfessionalInvoices.AsNoTracking()
            .Where(x=>x.ClientOrganizationId==org&&x.Status!=ProfessionalInvoiceStatus.Cancelled&&
                x.CreatedAtUtc>=fromUtc&&x.CreatedAtUtc<=toUtc)
            .Select(x=>new{x.Subtotal,x.TaxAmount,x.Currency}).ToListAsync(ct);
        string[] invoiceCurrencies=invoiceRows.Select(x=>x.Currency).Distinct().ToArray();
        decimal? invoicedAmount=invoiceRows.Count==0||invoiceCurrencies.Length!=1?null:
            decimal.Round(invoiceRows.Sum(x=>x.Subtotal+x.TaxAmount),2);

        int reviewedOrDisputed=await db.ServiceEntries.AsNoTracking().CountAsync(x=>
            x.OrganizationId==org&&x.ServiceDate>=from&&x.ServiceDate<=to&&
            (x.Status==ServiceEntryStatus.Approved||x.Status==ServiceEntryStatus.Rejected||x.Status==ServiceEntryStatus.Disputed),ct);
        int disputesOpened=await db.ServiceDisputes.AsNoTracking().CountAsync(x=>
            x.ClientOrganizationId==org&&x.CreatedAtUtc>=fromUtc&&x.CreatedAtUtc<=toUtc,ct);

        return result with
        {
            InitialIntegrationsCompleted=initialIntegrations,
            ReliableRelationships=reliableRelationships,
            AverageInvitationToActivationDelayHours=invitationActivationDelay,
            SignedContractRatePercent=signedContractRate,
            MissionCancellationRatePercent=cancellationRate,
            InvoicedAmount=invoicedAmount,
            InvoicedCurrency=invoiceCurrencies.Length==1?invoiceCurrencies[0]:null,
            DisputeRatePercent=Percent(disputesOpened,reviewedOrDisputed)
        };
    }

    private async Task<MarketplaceDashboardAdvancedKpis> BuildAdvancedProfessionalAsync(
        ProfessionalProfileId profileId,DateOnly from,DateOnly to,DateTimeOffset fromUtc,DateTimeOffset toUtc,CancellationToken ct)
    {
        var invitations=db.FreelanceInvitations.AsNoTracking()
            .Where(x=>x.ProfessionalProfileId==profileId&&x.SentAtUtc>=fromUtc&&x.SentAtUtc<=toUtc);
        int invitationsSent=await invitations.CountAsync(ct);
        int invitationsAccepted=await invitations.CountAsync(x=>x.Status==FreelanceInvitationStatus.Accepted,ct);
        bool active=await db.ProfessionalProfiles.AsNoTracking().AnyAsync(x=>x.Id==profileId&&x.Status==ProfessionalProfileStatus.Active,ct);
        int invitationsActivated=active?invitationsAccepted:0;

        var decidedApps=db.ProfessionalApplications.AsNoTracking()
            .Where(x=>x.ProfessionalProfileId==profileId&&x.DecidedAtUtc>=fromUtc&&x.DecidedAtUtc<=toUtc);
        int applicationsDecided=await decidedApps.CountAsync(ct);
        int applicationsAccepted=await decidedApps.CountAsync(x=>x.Status==ProfessionalApplicationStatus.Accepted,ct);

        ProfessionalProfile? profile=await db.ProfessionalProfiles.AsNoTracking()
            .SingleOrDefaultAsync(x=>x.Id==profileId,ct);
        bool complete=profile?.IsProfileComplete==true;
        decimal? avgDocumentValidation=await AverageDocumentValidationDelayAsync([profileId.Value],fromUtc,toUtc,ct);

        int contracts=await db.ProfessionalEngagements.AsNoTracking().CountAsync(x=>
            x.ProfessionalProfileId==profileId&&x.ContractPrepared&&x.StartsOn<=to&&x.EndsOn>=from,ct);

        decimal plannedMinutes=await db.ProfessionalMissions.AsNoTracking()
            .Where(x=>x.ProfessionalProfileId==profileId&&x.StartsOn<=to&&x.EndsOn>=from&&
                x.Status!=ProfessionalMissionStatus.Draft&&x.Status!=ProfessionalMissionStatus.Declined&&x.Status!=ProfessionalMissionStatus.Cancelled)
            .SumAsync(x=>(decimal?)(x.EstimatedMinutes??0),ct)??0m;

        decimal realizedMinutes=await db.ServiceEntries.AsNoTracking()
            .Where(x=>x.ProfessionalProfileId==profileId&&x.ServiceDate>=from&&x.ServiceDate<=to&&x.Status==ServiceEntryStatus.Approved)
            .SumAsync(x=>(decimal?)x.QuantityMinutes,ct)??0m;

        int cancelledMissions=await db.ProfessionalMissions.AsNoTracking().CountAsync(x=>
            x.ProfessionalProfileId==profileId&&x.Status==ProfessionalMissionStatus.Cancelled&&
            x.CancelledAtUtc>=fromUtc&&x.CancelledAtUtc<=toUtc,ct);

        int students=await db.ProfessionalStudentAssignments.AsNoTracking()
            .Where(x=>x.ProfessionalProfileId==profileId&&x.StartsOn<=to&&x.EndsOn>=from)
            .Select(x=>x.StudentId.Value).Distinct().CountAsync(ct);

        var reviewed=db.ServiceEntries.AsNoTracking().Where(x=>
            x.ProfessionalProfileId==profileId&&x.ReviewedAtUtc>=fromUtc&&x.ReviewedAtUtc<=toUtc);
        int reviewedCount=await reviewed.CountAsync(ct);
        int firstPass=await reviewed.CountAsync(x=>x.Status==ServiceEntryStatus.Approved,ct);

        int overdueInvoices=await db.ProfessionalInvoices.AsNoTracking().CountAsync(x=>
            x.ProfessionalProfileId==profileId&&x.PaymentStatus==ProfessionalInvoicePaymentStatus.Overdue,ct);

        int openDisputes=await db.ServiceDisputes.AsNoTracking().CountAsync(x=>
            x.ProfessionalProfileId==profileId&&x.Status!=ServiceDisputeStatus.Resolved&&x.Status!=ServiceDisputeStatus.Rejected,ct);

        var costRows=await db.ServiceEntries.AsNoTracking()
            .Where(x=>x.ProfessionalProfileId==profileId&&x.ServiceDate>=from&&x.ServiceDate<=to&&x.Status==ServiceEntryStatus.Approved)
            .Select(x=>new{x.QuantityMinutes,x.UnitRate,x.Currency}).ToListAsync(ct);
        string[] currencies=costRows.Select(x=>x.Currency).Distinct().ToArray();
        decimal? avgHourlyCost=costRows.Count==0||currencies.Length!=1?null:
            decimal.Round(costRows.Sum(x=>x.UnitRate*x.QuantityMinutes/60m)/(costRows.Sum(x=>x.QuantityMinutes)/60m),2);

        MarketplaceDashboardAdvancedKpis result=BuildAdvanced(
            invitationsSent,invitationsAccepted,invitationsActivated,applicationsDecided,applicationsAccepted,
            complete?1:0,1,avgDocumentValidation,contracts,plannedMinutes,realizedMinutes,
            cancelledMissions,students,reviewedCount,firstPass,overdueInvoices,openDisputes,
            avgHourlyCost,currencies.Length==1?currencies[0]:null);

        int initialIntegrations=await db.ProfessionalEngagements.AsNoTracking().CountAsync(x=>
            x.ProfessionalProfileId==profileId&&x.InitialIntegrationCompletedAtUtc>=fromUtc&&x.InitialIntegrationCompletedAtUtc<=toUtc,ct);
        int reliableRelationships=await db.ProfessionalEngagements.AsNoTracking().CountAsync(x=>
            x.ProfessionalProfileId==profileId&&x.ReliableRelationshipEstablishedAtUtc!=null,ct);

        decimal? invitationActivationDelay=await AverageInvitationActivationDelayProfessionalAsync(
            profileId,fromUtc,toUtc,ct);

        int relationshipDenominator=await db.ProfessionalEngagements.AsNoTracking().CountAsync(x=>
            x.ProfessionalProfileId==profileId&&x.CreatedAtUtc>=fromUtc&&x.CreatedAtUtc<=toUtc,ct);
        decimal? signedContractRate=Percent(contracts,relationshipDenominator);

        int missionDenominator=await db.ProfessionalMissions.AsNoTracking().CountAsync(x=>
            x.ProfessionalProfileId==profileId&&x.StartsOn<=to&&x.EndsOn>=from&&
            x.Status!=ProfessionalMissionStatus.Draft,ct);
        decimal? cancellationRate=Percent(cancelledMissions,missionDenominator);

        var invoiceRows=await db.ProfessionalInvoices.AsNoTracking()
            .Where(x=>x.ProfessionalProfileId==profileId&&x.Status!=ProfessionalInvoiceStatus.Cancelled&&
                x.CreatedAtUtc>=fromUtc&&x.CreatedAtUtc<=toUtc)
            .Select(x=>new{x.Subtotal,x.TaxAmount,x.Currency}).ToListAsync(ct);
        string[] invoiceCurrencies=invoiceRows.Select(x=>x.Currency).Distinct().ToArray();
        decimal? invoicedAmount=invoiceRows.Count==0||invoiceCurrencies.Length!=1?null:
            decimal.Round(invoiceRows.Sum(x=>x.Subtotal+x.TaxAmount),2);

        int reviewedOrDisputed=await db.ServiceEntries.AsNoTracking().CountAsync(x=>
            x.ProfessionalProfileId==profileId&&x.ServiceDate>=from&&x.ServiceDate<=to&&
            (x.Status==ServiceEntryStatus.Approved||x.Status==ServiceEntryStatus.Rejected||x.Status==ServiceEntryStatus.Disputed),ct);
        int disputesOpened=await db.ServiceDisputes.AsNoTracking().CountAsync(x=>
            x.ProfessionalProfileId==profileId&&x.CreatedAtUtc>=fromUtc&&x.CreatedAtUtc<=toUtc,ct);

        return result with
        {
            InitialIntegrationsCompleted=initialIntegrations,
            ReliableRelationships=reliableRelationships,
            AverageInvitationToActivationDelayHours=invitationActivationDelay,
            SignedContractRatePercent=signedContractRate,
            MissionCancellationRatePercent=cancellationRate,
            InvoicedAmount=invoicedAmount,
            InvoicedCurrency=invoiceCurrencies.Length==1?invoiceCurrencies[0]:null,
            DisputeRatePercent=Percent(disputesOpened,reviewedOrDisputed)
        };
    }

    private async Task<decimal?> AverageInvitationActivationDelayOrganizationAsync(
        OrganizationId org,DateTimeOffset fromUtc,DateTimeOffset toUtc,CancellationToken ct)
    {
        var invitations=await db.FreelanceInvitations.AsNoTracking()
            .Where(x=>x.ClientOrganizationId==org&&x.SentAtUtc>=fromUtc&&x.SentAtUtc<=toUtc&&
                x.Status==FreelanceInvitationStatus.Accepted&&x.ProfessionalProfileId!=null)
            .Select(x=>new{x.ProfessionalProfileId,x.SentAtUtc})
            .ToListAsync(ct);

        if(invitations.Count==0)return null;
        var delays=new List<double>();
        foreach(var invitation in invitations)
        {
            DateTimeOffset? activated=await db.ProfessionalEngagements.AsNoTracking()
                .Where(x=>x.OrganizationId==org&&x.ProfessionalProfileId==invitation.ProfessionalProfileId!.Value&&
                    x.ActivatedAtUtc!=null&&x.ActivatedAtUtc>=invitation.SentAtUtc)
                .OrderBy(x=>x.ActivatedAtUtc)
                .Select(x=>x.ActivatedAtUtc)
                .FirstOrDefaultAsync(ct);
            if(activated is not null&&invitation.SentAtUtc is not null)
                delays.Add((activated.Value-invitation.SentAtUtc.Value).TotalHours);
        }
        return delays.Count==0?null:decimal.Round((decimal)delays.Average(),2);
    }

    private async Task<decimal?> AverageInvitationActivationDelayProfessionalAsync(
        ProfessionalProfileId profileId,DateTimeOffset fromUtc,DateTimeOffset toUtc,CancellationToken ct)
    {
        var invitations=await db.FreelanceInvitations.AsNoTracking()
            .Where(x=>x.ProfessionalProfileId==profileId&&x.SentAtUtc>=fromUtc&&x.SentAtUtc<=toUtc&&
                x.Status==FreelanceInvitationStatus.Accepted)
            .Select(x=>new{x.ClientOrganizationId,x.SentAtUtc})
            .ToListAsync(ct);

        if(invitations.Count==0)return null;
        var delays=new List<double>();
        foreach(var invitation in invitations)
        {
            DateTimeOffset? activated=await db.ProfessionalEngagements.AsNoTracking()
                .Where(x=>x.OrganizationId==invitation.ClientOrganizationId&&x.ProfessionalProfileId==profileId&&
                    x.ActivatedAtUtc!=null&&x.ActivatedAtUtc>=invitation.SentAtUtc)
                .OrderBy(x=>x.ActivatedAtUtc)
                .Select(x=>x.ActivatedAtUtc)
                .FirstOrDefaultAsync(ct);
            if(activated is not null&&invitation.SentAtUtc is not null)
                delays.Add((activated.Value-invitation.SentAtUtc.Value).TotalHours);
        }
        return delays.Count==0?null:decimal.Round((decimal)delays.Average(),2);
    }

    private async Task<decimal?> AverageDocumentValidationDelayAsync(
        Guid[] profileIds,DateTimeOffset fromUtc,DateTimeOffset toUtc,CancellationToken ct)
    {
        if(profileIds.Length==0)return null;
        var rows=await db.ProfessionalDocuments.AsNoTracking()
            .Where(x=>profileIds.Contains(x.ProfessionalProfileId.Value)&&x.VerifiedAtUtc!=null&&
                x.VerifiedAtUtc>=fromUtc&&x.VerifiedAtUtc<=toUtc)
            .Select(x=>new{x.CreatedAtUtc,x.VerifiedAtUtc})
            .ToListAsync(ct);
        return rows.Count==0?null:
            decimal.Round((decimal)rows.Average(x=>(x.VerifiedAtUtc!.Value-x.CreatedAtUtc).TotalHours),2);
    }

    private static MarketplaceDashboardAdvancedKpis BuildAdvanced(
        int invitationsSent,int invitationsAccepted,int invitationsActivated,
        int applicationsDecided,int applicationsAccepted,int completeProfiles,int profilesInScope,
        decimal? avgDocumentValidation,int contracts,decimal plannedMinutes,decimal realizedMinutes,
        int cancelledMissions,int students,int reviewedCount,int firstPass,int overdueInvoices,int openDisputes,
        decimal? avgHourlyCost,string? costCurrency)
    {
        decimal plannedHours=decimal.Round(plannedMinutes/60m,2);
        decimal realizedHours=decimal.Round(realizedMinutes/60m,2);
        return new(
            invitationsSent,
            invitationsAccepted,
            invitationsActivated,
            Percent(invitationsAccepted,invitationsSent),
            Percent(invitationsActivated,invitationsSent),
            applicationsDecided,
            applicationsAccepted,
            Percent(applicationsAccepted,applicationsDecided),
            completeProfiles,
            profilesInScope,
            Percent(completeProfiles,profilesInScope),
            avgDocumentValidation,
            contracts,
            plannedHours,
            realizedHours,
            cancelledMissions,
            plannedMinutes<=0?null:decimal.Round(realizedMinutes/plannedMinutes*100m,2),
            students,
            reviewedCount,
            firstPass,
            Percent(firstPass,reviewedCount),
            overdueInvoices,
            openDisputes,
            avgHourlyCost,
            costCurrency);
    }

    private static decimal? Percent(int numerator,int denominator)=>
        denominator<=0?null:decimal.Round((decimal)numerator/denominator*100m,2);
}
