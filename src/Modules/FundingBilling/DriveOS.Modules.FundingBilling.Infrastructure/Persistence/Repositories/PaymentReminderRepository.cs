using DriveOS.Modules.FundingBilling.Domain.Collections;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;
namespace DriveOS.Modules.FundingBilling.Infrastructure.Persistence.Repositories;
internal sealed class PaymentReminderRepository(FundingBillingDbContext dbContext) : IPaymentReminderRepository
{
    public Task<bool> HasPendingAsync(OrganizationId organizationId, PaymentReminderTargetType targetType, Guid targetId, CancellationToken cancellationToken = default) => dbContext.PaymentReminders.AnyAsync(x => x.OrganizationId == organizationId && x.TargetType == targetType && x.TargetId == targetId && x.Status == PaymentReminderStatus.Pending, cancellationToken);
    public Task<int> CountForTargetAsync(OrganizationId organizationId, PaymentReminderTargetType targetType, Guid targetId, CancellationToken cancellationToken = default) => dbContext.PaymentReminders.CountAsync(x => x.OrganizationId == organizationId && x.TargetType == targetType && x.TargetId == targetId, cancellationToken);
    public Task<PaymentReminder?> GetByIdAsync(PaymentReminderId id, CancellationToken cancellationToken = default) => dbContext.PaymentReminders.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    public async Task<IReadOnlyCollection<PaymentReminder>> ListPendingAsync(OrganizationId organizationId, int take, CancellationToken cancellationToken = default) => await dbContext.PaymentReminders.Where(x => x.OrganizationId == organizationId && x.Status == PaymentReminderStatus.Pending).OrderBy(x => x.RequestedAtUtc).Take(take).ToListAsync(cancellationToken);
    public Task AddAsync(PaymentReminder reminder, CancellationToken cancellationToken = default) => dbContext.PaymentReminders.AddAsync(reminder, cancellationToken).AsTask();
}
