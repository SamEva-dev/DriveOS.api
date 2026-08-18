using DriveOS.Modules.FundingBilling.Application.Auditing;
using DriveOS.SharedKernel.Identifiers;
using FluentAssertions;

namespace DriveOS.UnitTests.Modules.FundingBilling.Auditing;

public sealed class GetFinancialAuditQueryHandlerTests
{
    [Fact]
    public async Task Handle_ShouldForwardTenantAndBillingAccountScope()
    {
        OrganizationId organizationId = OrganizationId.New();
        BillingAccountId billingAccountId = BillingAccountId.New();
        var expected = new[]
        {
            new FinancialAuditEntryResponse(
                Guid.NewGuid(),
                billingAccountId.Value,
                "Payment",
                Guid.NewGuid(),
                "PaymentReceived",
                Guid.NewGuid(),
                DateTimeOffset.UtcNow,
                null)
        };
        var readService = new CapturingReadService(expected);
        var handler = new GetFinancialAuditQueryHandler(readService);

        var result = await handler.Handle(
            new GetFinancialAuditQuery(organizationId, billingAccountId),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(expected);
        readService.OrganizationId.Should().Be(organizationId);
        readService.BillingAccountId.Should().Be(billingAccountId);
    }

    private sealed class CapturingReadService(IReadOnlyList<FinancialAuditEntryResponse> entries)
        : IFinancialAuditReadService
    {
        public OrganizationId OrganizationId { get; private set; }
        public BillingAccountId BillingAccountId { get; private set; }

        public Task<IReadOnlyList<FinancialAuditEntryResponse>> ListAsync(
            OrganizationId organizationId,
            BillingAccountId billingAccountId,
            CancellationToken cancellationToken = default)
        {
            OrganizationId = organizationId;
            BillingAccountId = billingAccountId;
            return Task.FromResult(entries);
        }
    }
}
