using System.Reflection;
using System.Text.Json;
using DriveOS.Application.Abstractions.Authentication;
using DriveOS.Application.Abstractions.Time;
using DriveOS.Modules.Contracts.Domain.ContractAmendments;
using DriveOS.Modules.Contracts.Domain.ContractDocuments;
using DriveOS.Modules.Contracts.Domain.SignatureProcesses;
using DriveOS.Modules.Contracts.Domain.TrainingContracts;
using DriveOS.Modules.Contracts.Infrastructure.Auditing;
using DriveOS.SharedKernel.Auditing;
using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace DriveOS.Modules.Contracts.Infrastructure.Persistence.Interceptors;

internal sealed class ContractsAuditInterceptor(IClock clock, ICurrentUser currentUser)
    : SaveChangesInterceptor
{
    private readonly HashSet<Guid> capturedEventIds = [];
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
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
        foreach (var entry in context.ChangeTracker.Entries<TrainingContract>())
            Capture(context, entry.Entity.OrganizationId, entry.Entity.Id, "TrainingContract", entry.Entity.Id.Value, entry.Entity.DomainEvents);

        foreach (var entry in context.ChangeTracker.Entries<ContractAmendment>())
            Capture(context, entry.Entity.OrganizationId, entry.Entity.ContractId, "ContractAmendment", entry.Entity.Id.Value, entry.Entity.DomainEvents);

        foreach (var entry in context.ChangeTracker.Entries<ContractDocument>())
            Capture(context, entry.Entity.OrganizationId, entry.Entity.ContractId, "ContractDocument", entry.Entity.Id.Value, entry.Entity.DomainEvents);

        foreach (var entry in context.ChangeTracker.Entries<SignatureProcess>())
            Capture(context, entry.Entity.OrganizationId, entry.Entity.ContractId, "SignatureProcess", entry.Entity.Id.Value, entry.Entity.DomainEvents);
    }

    private void Capture(
        DbContext context,
        OrganizationId organizationId,
        TrainingContractId contractId,
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

            UserId? actor = ResolveActor(domainEvent) ?? currentUser.UserId;
            string? details = SerializeSafely(domainEvent);

            context.Set<ContractAuditEntry>().Add(ContractAuditEntry.Create(
                domainEvent.EventId,
                organizationId,
                contractId,
                aggregateType,
                aggregateId,
                action,
                actor,
                domainEvent.OccurredAtUtc,
                details));
        }
    }

    private static UserId? ResolveActor(IDomainEvent domainEvent)
    {
        PropertyInfo? property = domainEvent.GetType()
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .FirstOrDefault(p =>
                p.PropertyType == typeof(UserId) &&
                p.Name.EndsWith("UserId", StringComparison.Ordinal));

        return property?.GetValue(domainEvent) is UserId userId && !userId.IsEmpty
            ? userId
            : null;
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
