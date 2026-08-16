using DriveOS.Modules.CRM.Domain.Leads;
using DriveOS.Modules.CRM.Domain.Leads.Events;
using DriveOS.SharedKernel.Identifiers;
using FluentAssertions;

namespace DriveOS.UnitTests.Modules.CRM.Leads;

public sealed class LeadClosureTests
{
    [Fact]
    public void Close_PreservesLeadAndStopsAutomaticFollowUps()
    {
        Lead lead = CreateLead();
        var result = lead.Close(
            LeadStatus.Lost,
            LeadClosureReason.PriceTooHigh,
            "Budget insuffisant",
            DateTimeOffset.Parse("2026-08-12T12:00:00Z")
        );
        result.IsSuccess.Should().BeTrue();
        lead.Status.Should().Be(LeadStatus.Lost);
        lead.AutomaticFollowUpsEnabled.Should().BeFalse();
        lead.DomainEvents.Should().Contain(e => e is LeadMarkedLostDomainEvent);
    }

    [Fact]
    public void SetDormant_RequiresFutureResumeDate()
    {
        Lead lead = CreateLead();
        var now = DateTimeOffset.Parse("2026-08-12T12:00:00Z");
        var result = lead.SetDormant(
            LeadClosureReason.ProjectPostponed,
            now,
            UserId.New(),
            null,
            null,
            now
        );
        result.Error.Code.Should().Be("Crm.Leads.Dormancy.ResumeDate.Future");
    }

    [Fact]
    public void Reopen_ReactivatesFollowUps()
    {
        Lead lead = CreateLead();
        var now = DateTimeOffset.Parse("2026-08-12T12:00:00Z");
        lead.Close(LeadStatus.NoResponse, LeadClosureReason.NoResponse, null, now);
        lead.Reopen("Nouveau contact", now.AddDays(1)).IsSuccess.Should().BeTrue();
        lead.Status.Should().Be(LeadStatus.New);
        lead.AutomaticFollowUpsEnabled.Should().BeTrue();
        lead.DomainEvents.Should().Contain(e => e is LeadReopenedDomainEvent);
    }

    [Fact]
    public void ReferToPartner_RequiresExplicitConsentDate()
    {
        Lead lead = CreateLead();
        var result = lead.ReferToPartner(
            "Partenaire A",
            "Nom et téléphone",
            default,
            null,
            DateTimeOffset.Parse("2026-08-12T12:00:00Z")
        );
        result.Error.Code.Should().Be("Crm.Leads.Referral.Consent.Required");
    }

    private static Lead CreateLead() =>
        Lead.Create(
            LeadId.New(),
            OrganizationId.New(),
            null,
            LeadIdentity.Create("Sam", "Test", "sam@example.com", null).Value,
            RequestedTraining.Create("B", TransmissionPreference.Manual, "Nice").Value,
            LeadSource.Create(LeadSourceType.Website).Value
        ).Value;
}
