using DriveOS.Modules.CRM.Domain.Leads;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;
using FluentAssertions;

namespace DriveOS.UnitTests.Modules.CRM.Leads;

public sealed class LeadLifecycleTests
{
    [Fact]
    public void ChangeStatus_ShouldFollowCommercialLifecycle()
    {
        Lead lead = CreateLead();

        lead.ChangeStatus(LeadStatus.Contacted).IsSuccess.Should().BeTrue();
        lead.Qualify(CreateQualification()).IsSuccess.Should().BeTrue();
        lead.ChangeStatus(LeadStatus.OfferSent).IsSuccess.Should().BeTrue();
        lead.ChangeStatus(LeadStatus.Negotiation).IsSuccess.Should().BeTrue();
        lead.ChangeStatus(LeadStatus.Won).IsSuccess.Should().BeTrue();
        lead.Status.Should().Be(LeadStatus.Won);
    }

    [Fact]
    public void Qualify_ShouldRequireContactedLeadAndStoreBusinessData()
    {
        Lead lead = CreateLead();
        lead.ChangeStatus(LeadStatus.Contacted);

        Result result = lead.Qualify(CreateQualification());

        result.IsSuccess.Should().BeTrue();
        lead.Status.Should().Be(LeadStatus.Qualified);
        lead.Qualification!.Financing.Should().Be(FinancingOption.CPF);
    }

    [Fact]
    public void ChangeStatus_ShouldNotBypassQualification()
    {
        Lead lead = CreateLead();
        lead.ChangeStatus(LeadStatus.Contacted);

        Result result = lead.ChangeStatus(LeadStatus.Qualified);

        result.IsFailure.Should().BeTrue();
        lead.Status.Should().Be(LeadStatus.Contacted);
    }

    [Fact]
    public void ChangeStatus_ShouldRejectInvalidTransition()
    {
        Lead lead = CreateLead();

        var result = lead.ChangeStatus(LeadStatus.Won);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Crm.Leads.Status.InvalidTransition");
        lead.Status.Should().Be(LeadStatus.New);
    }

    [Fact]
    public void Lose_ShouldRequireReason()
    {
        Lead lead = CreateLead();

        var result = lead.ChangeStatus(LeadStatus.Lost);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Crm.Leads.LossReason.Required");
    }

    [Fact]
    public void Reactivate_ShouldReturnLostLeadToNew()
    {
        Lead lead = CreateLead();
        lead.ChangeStatus(LeadStatus.Lost, "Projet reporté");

        lead.ChangeStatus(LeadStatus.New).IsSuccess.Should().BeTrue();
        lead.Status.Should().Be(LeadStatus.New);
    }

    private static Lead CreateLead() =>
        Lead.Create(
            LeadId.New(),
            OrganizationId.New(),
            null,
            LeadIdentity.Create("Jane", "Doe", "jane@example.com", null).Value,
            RequestedTraining.Create("B", TransmissionPreference.Manual, null).Value,
            LeadSource.Create(LeadSourceType.Website).Value
        ).Value;

    private static LeadQualification CreateQualification() =>
        LeadQualification
            .Create(
                "Obtenir le permis pour travailler",
                "B",
                "Soirs et samedi",
                new DateOnly(2026, 12, 1),
                FinancingOption.CPF,
                "Dossier CPF à vérifier"
            )
            .Value;
}
