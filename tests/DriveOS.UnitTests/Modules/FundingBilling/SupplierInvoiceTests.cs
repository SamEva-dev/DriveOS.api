using DriveOS.Modules.FundingBilling.Domain.SupplierInvoices;
using DriveOS.SharedKernel.Identifiers;
using Xunit;

namespace DriveOS.UnitTests.Modules.FundingBilling;

public sealed class SupplierInvoiceTests
{
    private static readonly UserId Actor=new(Guid.NewGuid());
    private static readonly OrganizationId Client=new(Guid.NewGuid());
    private static readonly DateTimeOffset Now=DateTimeOffset.UtcNow;

    private static SupplierInvoice Create()=>SupplierInvoice.Receive(
        new(Guid.NewGuid()),Client,Guid.NewGuid(),SupplierInvoiceSourceType.ProfessionalMarketplace,
        Guid.NewGuid(),Guid.NewGuid(),"F-2026-001",
        new DateOnly(2026,10,1),new DateOnly(2026,10,31),
        "EUR",100m,20m,"FreelanceIssued",Now,Actor).Value;

    [Fact]
    public void Supplier_invoice_requires_matching_before_operational_approval()
    {
        var invoice=Create();
        Assert.True(invoice.ApproveOperational(Actor,Now).IsFailure);
        Assert.True(invoice.MarkMatched(Actor,Now).IsSuccess);
        Assert.Equal(SupplierInvoiceStatus.PendingOperationalApproval,invoice.Status);
    }

    [Fact]
    public void Operational_and_financial_approvals_are_separate()
    {
        var invoice=Create();
        invoice.MarkMatched(Actor,Now);
        Assert.True(invoice.ApproveOperational(Actor,Now).IsSuccess);
        Assert.Equal(SupplierInvoiceStatus.PendingFinancialApproval,invoice.Status);
        Assert.True(invoice.ApproveFinancial(Actor,Now).IsSuccess);
        Assert.Equal(SupplierInvoiceStatus.Approved,invoice.Status);
    }

    [Fact]
    public void Payment_cannot_be_scheduled_before_financial_approval()
    {
        var invoice=Create();
        invoice.MarkMatched(Actor,Now);
        invoice.ApproveOperational(Actor,Now);
        Assert.True(invoice.SchedulePayment(invoice.RemainingAmount,Actor,Now).IsFailure);
        invoice.ApproveFinancial(Actor,Now);
        Assert.True(invoice.SchedulePayment(invoice.RemainingAmount,Actor,Now).IsSuccess);
        Assert.Equal(SupplierInvoiceStatus.ScheduledForPayment,invoice.Status);
    }
}
