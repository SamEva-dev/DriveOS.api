using DriveOS.Modules.FundingBilling.Domain.Installments;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;

namespace DriveOS.Modules.FundingBilling.Infrastructure.Persistence.Repositories;

internal sealed class PaymentInstallmentRepository(FundingBillingDbContext dbContext) : IPaymentInstallmentRepository
{
    public Task<PaymentInstallment?> GetByIdAsync(PaymentInstallmentId id, CancellationToken cancellationToken = default)
        => dbContext.PaymentInstallments.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

    public Task AddAsync(PaymentInstallment installment, CancellationToken cancellationToken = default)
        => dbContext.PaymentInstallments.AddAsync(installment, cancellationToken).AsTask();

    public Task AddRangeAsync(IEnumerable<PaymentInstallment> installments, CancellationToken cancellationToken = default)
        => dbContext.PaymentInstallments.AddRangeAsync(installments, cancellationToken);
    public async Task<IReadOnlyCollection<PaymentInstallment>> ListDueAsync(OrganizationId organizationId, DateOnly beforeDate, CancellationToken cancellationToken = default) =>
        await dbContext.PaymentInstallments
            .Where(x => x.OrganizationId == organizationId && x.DueDate < beforeDate && (x.Status == PaymentInstallmentStatus.Scheduled || x.Status == PaymentInstallmentStatus.Pending || x.Status == PaymentInstallmentStatus.Rescheduled || x.Status == PaymentInstallmentStatus.PartiallyPaid))
            .ToListAsync(cancellationToken);
}

