using DriveOS.Modules.FundingBilling.Domain.Collections;
using DriveOS.SharedKernel.Identifiers;
namespace DriveOS.Modules.FundingBilling.Application.Collections.Read;
public sealed record OverdueItemResponse(string TargetType, Guid TargetId, Guid BillingAccountId, DateOnly DueDate, decimal OutstandingAmount, string Currency, int DaysOverdue, int ReminderCount, DateTimeOffset? LastReminderAtUtc);
public interface ICollectionReadService { Task<IReadOnlyCollection<OverdueItemResponse>> ListOverdueAsync(OrganizationId organizationId, DateOnly businessDate, CancellationToken cancellationToken = default); }
