using System.Reflection;
using System.Text.Json;
using DriveOS.Application.Abstractions.Authentication;
using DriveOS.Application.Abstractions.Time;
using DriveOS.Modules.FundingBilling.Domain.BillingAccounts;
using DriveOS.Modules.FundingBilling.Domain.BillingParties;
using DriveOS.Modules.FundingBilling.Domain.Collections;
using DriveOS.Modules.FundingBilling.Domain.CreditNotes;
using DriveOS.Modules.FundingBilling.Domain.FundingPlans;
using DriveOS.Modules.FundingBilling.Domain.Installments;
using DriveOS.Modules.FundingBilling.Domain.Invoices;
using DriveOS.Modules.FundingBilling.Domain.Payments;
using DriveOS.Modules.FundingBilling.Domain.Refunds;
using DriveOS.Modules.FundingBilling.Domain.TrainingCredits;
using DriveOS.Modules.FundingBilling.Infrastructure.Auditing;
using DriveOS.SharedKernel.Auditing;
using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace DriveOS.Modules.FundingBilling.Infrastructure.Persistence.Interceptors;

internal sealed class FundingBillingAuditInterceptor(IClock clock, ICurrentUser currentUser)
    : SaveChangesInterceptor
{
    private readonly HashSet<Guid> capturedEventIds = [];
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        Apply(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        Apply(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    public override void SaveChangesFailed(DbContextErrorEventData eventData)
    {
        capturedEventIds.Clear();
        base.SaveChangesFailed(eventData);
    }

    public override Task SaveChangesFailedAsync(DbContextErrorEventData eventData, CancellationToken cancellationToken = default)
    {
        capturedEventIds.Clear();
        return base.SaveChangesFailedAsync(eventData, cancellationToken);
    }

    private void Apply(DbContext? context)
    {
        if (context is null)
            return;

        ApplyEntityAudit(context);
        CaptureDomainAudit(context);
    }

    private void ApplyEntityAudit(DbContext context)
    {
        DateTimeOffset now = clock.UtcNow;
        UserId? userId = currentUser.UserId;

        foreach (var entry in context.ChangeTracker.Entries<IAuditableEntity>())
        {
            if (entry.State == EntityState.Added && entry.Entity.CreatedAtUtc == default)
                entry.Entity.SetCreatedAudit(now, userId);
            else if (entry.State == EntityState.Modified)
                entry.Entity.SetModifiedAudit(now, userId);
        }
    }

    private void CaptureDomainAudit(DbContext context)
    {
        foreach (var entry in context.ChangeTracker.Entries<BillingAccount>())
            Capture(context, entry.Entity.OrganizationId, entry.Entity.Id, "BillingAccount", entry.Entity.Id.Value, entry.Entity.DomainEvents);

        foreach (var entry in context.ChangeTracker.Entries<Invoice>())
            Capture(context, entry.Entity.OrganizationId, entry.Entity.BillingAccountId, "Invoice", entry.Entity.Id.Value, entry.Entity.DomainEvents);

        foreach (var entry in context.ChangeTracker.Entries<PaymentInstallment>())
            Capture(context, entry.Entity.OrganizationId, entry.Entity.BillingAccountId, "PaymentInstallment", entry.Entity.Id.Value, entry.Entity.DomainEvents);

        foreach (var entry in context.ChangeTracker.Entries<Payment>())
            Capture(context, entry.Entity.OrganizationId, entry.Entity.BillingAccountId, "Payment", entry.Entity.Id.Value, entry.Entity.DomainEvents);

        foreach (var entry in context.ChangeTracker.Entries<PaymentReminder>())
            Capture(context, entry.Entity.OrganizationId, entry.Entity.BillingAccountId, "PaymentReminder", entry.Entity.Id.Value, entry.Entity.DomainEvents);

        foreach (var entry in context.ChangeTracker.Entries<FundingPlan>())
            Capture(context, entry.Entity.OrganizationId, entry.Entity.BillingAccountId, "FundingPlan", entry.Entity.Id.Value, entry.Entity.DomainEvents);

        foreach (var entry in context.ChangeTracker.Entries<BillingParty>())
            Capture(context, entry.Entity.OrganizationId, entry.Entity.BillingAccountId, "BillingParty", entry.Entity.Id.Value, entry.Entity.DomainEvents);

        foreach (var entry in context.ChangeTracker.Entries<TrainingCreditAccount>())
            Capture(context, entry.Entity.OrganizationId, entry.Entity.BillingAccountId, "TrainingCreditAccount", entry.Entity.Id.Value, entry.Entity.DomainEvents);

        foreach (var entry in context.ChangeTracker.Entries<Refund>())
            Capture(context, entry.Entity.OrganizationId, entry.Entity.BillingAccountId, "Refund", entry.Entity.Id.Value, entry.Entity.DomainEvents);

        foreach (var entry in context.ChangeTracker.Entries<CreditNote>())
            Capture(context, entry.Entity.OrganizationId, entry.Entity.BillingAccountId, "CreditNote", entry.Entity.Id.Value, entry.Entity.DomainEvents);
    }

    private void Capture(
        DbContext context,
        OrganizationId organizationId,
        BillingAccountId billingAccountId,
        string aggregateType,
        Guid aggregateId,
        IReadOnlyCollection<IDomainEvent> domainEvents)
    {
        foreach (IDomainEvent domainEvent in domainEvents)
        {
            if (!capturedEventIds.Add(domainEvent.EventId))
                continue;

            string eventName = domainEvent.GetType().Name;
            string action = eventName.EndsWith("DomainEvent", StringComparison.Ordinal)
                ? eventName[..^"DomainEvent".Length]
                : eventName;

            context.Set<FinancialAuditEntry>().Add(FinancialAuditEntry.Create(
                domainEvent.EventId,
                organizationId,
                billingAccountId,
                aggregateType,
                aggregateId,
                action,
                ResolveActor(domainEvent) ?? currentUser.UserId,
                domainEvent.OccurredAtUtc,
                SerializeSafely(domainEvent)));
        }
    }

    private static UserId? ResolveActor(IDomainEvent domainEvent)
    {
        PropertyInfo? property = domainEvent.GetType()
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .FirstOrDefault(p => p.PropertyType == typeof(UserId) && p.Name.EndsWith("UserId", StringComparison.Ordinal));

        return property?.GetValue(domainEvent) is UserId userId && !userId.IsEmpty ? userId : null;
    }

    private static string? SerializeSafely(IDomainEvent domainEvent)
    {
        try
        {
            return JsonSerializer.Serialize(domainEvent, domainEvent.GetType(), JsonOptions);
        }
        catch
        {
            return null;
        }
    }
}
