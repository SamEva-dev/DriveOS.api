using DriveOS.Modules.ProfessionalMarketplace.Domain.Invoices;
using DriveOS.Modules.ProfessionalMarketplace.Domain.ServiceEntries;
using DriveOS.Modules.ProfessionalMarketplace.Domain.ServiceStatements;
using DriveOS.SharedKernel.Identifiers;
using Xunit;

namespace DriveOS.UnitTests.Modules.ProfessionalMarketplace;
public sealed class ProfessionalInvoiceTests
{
    private static readonly UserId Actor=new(Guid.NewGuid());
    private static readonly DateTimeOffset Now=DateTimeOffset.UtcNow;

    private static ServiceStatement ApprovedStatement()
    {
        var engagement=new ProfessionalEngagementId(Guid.NewGuid());
        var profile=new ProfessionalProfileId(Guid.NewGuid());
        var org=new OrganizationId(Guid.NewGuid());
        var e=ServiceEntry.Create(new(Guid.NewGuid()),engagement,null,profile,org,null,ServiceEntrySourceType.TrainingSession,
            Guid.NewGuid(),new DateOnly(2026,9,10),"DRIVING",60,100m,0m,0m,0m,"EUR","Séance conduite",
            new DateOnly(2026,9,1),new DateOnly(2026,9,30),Now,Actor).Value;
        e.Submit(Now,Actor);e.Approve(Now,Actor);
        var s=ServiceStatement.Create(new(Guid.NewGuid()),engagement,profile,org,Guid.NewGuid(),
            new DateOnly(2026,9,1),new DateOnly(2026,9,30),[e],Now,Actor).Value;
        s.Submit(Now,Actor);s.StartReview(Now,Actor);s.RecalculateReviewStatus(Now,Actor);
        return s;
    }

    [Fact] public void Invoice_uses_only_approved_statement_amount()
    {
        var s=ApprovedStatement();
        var r=ProfessionalInvoice.Create(new(Guid.NewGuid()),s,ProfessionalInvoiceMode.FreelanceIssued,
            new DateOnly(2026,10,1),new DateOnly(2026,10,31),20m,"F-2026-001","IBAN snapshot",Now,Actor);
        Assert.True(r.IsSuccess);
        Assert.Equal(100m,r.Value.Subtotal);
        Assert.Equal(120m,r.Value.Total);
    }

    [Fact] public void Freelance_issued_invoice_requires_number()
    {
        var s=ApprovedStatement();
        var r=ProfessionalInvoice.Create(new(Guid.NewGuid()),s,ProfessionalInvoiceMode.FreelanceIssued,
            new DateOnly(2026,10,1),new DateOnly(2026,10,31),0m,null,null,Now,Actor);
        Assert.True(r.IsFailure);
    }

    [Fact] public void Finance_request_requires_validation()
    {
        var s=ApprovedStatement();
        var invoice=ProfessionalInvoice.Create(new(Guid.NewGuid()),s,ProfessionalInvoiceMode.SelfBilling,
            new DateOnly(2026,10,1),new DateOnly(2026,10,31),0m,null,null,Now,Actor).Value;
        Assert.True(invoice.RequestFinance(Guid.NewGuid(),"PendingOperationalApproval",Now,Actor).IsFailure);
        Assert.True(invoice.Validate(Now,Actor).IsSuccess);
        Assert.True(invoice.RequestFinance(Guid.NewGuid(),"PendingOperationalApproval",Now,Actor).IsSuccess);
        Assert.Equal(ProfessionalInvoiceStatus.Requested,invoice.Status);
        Assert.Single(invoice.DomainEvents);
    }
}
