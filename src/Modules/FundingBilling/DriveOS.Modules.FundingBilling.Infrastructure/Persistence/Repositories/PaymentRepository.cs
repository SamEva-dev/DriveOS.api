using DriveOS.Modules.FundingBilling.Domain.Payments;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;

namespace DriveOS.Modules.FundingBilling.Infrastructure.Persistence.Repositories;

internal sealed class PaymentRepository(FundingBillingDbContext dbContext) : IPaymentRepository
{
    public Task<Payment?> GetByIdAsync(PaymentId id, CancellationToken cancellationToken = default) =>
        dbContext.Payments.Include(x => x.Allocations).SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

    public Task<Payment?> GetByExternalReferenceAsync(OrganizationId organizationId, string externalReference, CancellationToken cancellationToken = default) =>
        dbContext.Payments.Include(x => x.Allocations).SingleOrDefaultAsync(x => x.OrganizationId == organizationId && x.ExternalReference == externalReference, cancellationToken);

    public Task AddAsync(Payment payment, CancellationToken cancellationToken = default) =>
        dbContext.Payments.AddAsync(payment, cancellationToken).AsTask();
}
