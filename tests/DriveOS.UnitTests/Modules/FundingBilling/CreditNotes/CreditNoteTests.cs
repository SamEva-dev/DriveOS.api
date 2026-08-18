using DriveOS.Modules.FundingBilling.Domain.CreditNotes;
using DriveOS.SharedKernel.Identifiers;
using FluentAssertions;
using Xunit;
namespace DriveOS.UnitTests.Modules.FundingBilling.CreditNotes;
public sealed class CreditNoteTests
{
 [Fact] public void Issue_freezes_credit_note_and_checks_maximum(){var note=CreditNote.CreateDraft(CreditNoteId.New(),new OrganizationId(Guid.NewGuid()),new BillingAccountId(Guid.NewGuid()),new InvoiceId(Guid.NewGuid()),"EUR","Correction tarifaire").Value;note.AddLine(CreditNoteLineId.New(),null,"Correction",1,"unit",100m,0m,20m).IsSuccess.Should().BeTrue();var actor=new UserId(Guid.NewGuid());note.Issue("AV-2026-0001",DateOnly.FromDateTime(DateTime.UtcNow),120m,actor,DateTimeOffset.UtcNow).IsSuccess.Should().BeTrue();note.Status.Should().Be(CreditNoteStatus.Issued);note.TotalAmount.Should().Be(120m);note.AddLine(CreditNoteLineId.New(),null,"Other",1,"unit",1m,0m,0m).IsFailure.Should().BeTrue();}
 [Fact] public void Issue_rejects_amount_over_invoice_creditable_amount(){var note=CreditNote.CreateDraft(CreditNoteId.New(),new OrganizationId(Guid.NewGuid()),new BillingAccountId(Guid.NewGuid()),new InvoiceId(Guid.NewGuid()),"EUR","Correction tarifaire").Value;note.AddLine(CreditNoteLineId.New(),null,"Correction",1,"unit",100m,0m,20m);note.Issue("AV-2026-0001",DateOnly.FromDateTime(DateTime.UtcNow),100m,new UserId(Guid.NewGuid()),DateTimeOffset.UtcNow).IsFailure.Should().BeTrue();}
}
