using DriveOS.Modules.FundingBilling.Application.Collections.Read;
using DriveOS.Modules.FundingBilling.Domain.Collections;
using DriveOS.Modules.FundingBilling.Domain.Invoices;
using DriveOS.Modules.FundingBilling.Domain.Installments;
using DriveOS.Modules.FundingBilling.Infrastructure.Persistence;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;
namespace DriveOS.Modules.FundingBilling.Infrastructure.Read;
internal sealed class CollectionReadService(FundingBillingDbContext dbContext) : ICollectionReadService
{
    public async Task<IReadOnlyCollection<OverdueItemResponse>> ListOverdueAsync(OrganizationId organizationId, DateOnly businessDate, CancellationToken cancellationToken = default)
    {
        var reminders = await dbContext.PaymentReminders.AsNoTracking().Where(x => x.OrganizationId == organizationId).ToListAsync(cancellationToken);
        var invoiceRows = await dbContext.Invoices.AsNoTracking().Include(x => x.Lines).Where(x => x.OrganizationId == organizationId && x.Status == InvoiceStatus.Overdue).ToListAsync(cancellationToken);
        var installmentRows = await dbContext.PaymentInstallments.AsNoTracking().Where(x => x.OrganizationId == organizationId && x.Status == PaymentInstallmentStatus.Overdue).ToListAsync(cancellationToken);
        var result = new List<OverdueItemResponse>();
        foreach (var x in invoiceRows.Where(x => x.DueDate.HasValue && x.RemainingAmount > 0m))
        {
            var targetReminders = reminders.Where(r => r.TargetType == PaymentReminderTargetType.Invoice && r.TargetId == x.Id.Value).ToArray();
            result.Add(new("Invoice", x.Id.Value, x.BillingAccountId.Value, x.DueDate!.Value, x.RemainingAmount, x.Currency, Math.Max(0, businessDate.DayNumber - x.DueDate.Value.DayNumber), targetReminders.Length, targetReminders.MaxBy(r => r.RequestedAtUtc)?.RequestedAtUtc));
        }
        foreach (var x in installmentRows.Where(x => x.RemainingAmount > 0m))
        {
            var targetReminders = reminders.Where(r => r.TargetType == PaymentReminderTargetType.Installment && r.TargetId == x.Id.Value).ToArray();
            result.Add(new("Installment", x.Id.Value, x.BillingAccountId.Value, x.DueDate, x.RemainingAmount, x.Currency, Math.Max(0, businessDate.DayNumber - x.DueDate.DayNumber), targetReminders.Length, targetReminders.MaxBy(r => r.RequestedAtUtc)?.RequestedAtUtc));
        }
        return result.OrderByDescending(x => x.DaysOverdue).ThenBy(x => x.DueDate).ToArray();
    }
}
