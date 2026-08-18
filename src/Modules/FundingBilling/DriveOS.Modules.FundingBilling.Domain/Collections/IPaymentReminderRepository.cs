using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.FundingBilling.Domain.Collections;

public interface IPaymentReminderRepository
{
    Task<bool> HasPendingAsync(OrganizationId organizationId, PaymentReminderTargetType targetType, Guid targetId, CancellationToken cancellationToken = default);
    Task<int> CountForTargetAsync(OrganizationId organizationId, PaymentReminderTargetType targetType, Guid targetId, CancellationToken cancellationToken = default);
    Task<PaymentReminder?> GetByIdAsync(PaymentReminderId id, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<PaymentReminder>> ListPendingAsync(OrganizationId organizationId, int take, CancellationToken cancellationToken = default);
    Task AddAsync(PaymentReminder reminder, CancellationToken cancellationToken = default);
}
