using DriveOS.Modules.TrainingDelivery.Domain.Incidents;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.UnitTests.Modules.TrainingDelivery;

public sealed class TrainingIncidentTests
{
    [Fact]
    public void Report_CriticalIncident_ShouldRequestEscalationAndPreserveParticipants()
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;
        var result = TrainingIncident.Report(
            TrainingIncidentId.New(), new OrganizationId(Guid.NewGuid()), TrainingSessionId.New(), new PersonId(Guid.NewGuid()),
            new UserId(Guid.NewGuid()), Guid.NewGuid(), new BranchId(Guid.NewGuid()), new OrganizationId(Guid.NewGuid()),
            Guid.NewGuid(), TrainingIncidentType.Accident, TrainingIncidentSeverity.Critical, now.AddMinutes(-2),
            "Collision légère pendant la séance.", "Mise en sécurité et arrêt du véhicule.",
            [(TrainingIncidentParticipantType.ThirdParty, null, "Conducteur tiers")], new UserId(Guid.NewGuid()), now);

        Assert.True(result.IsSuccess);
        Assert.True(result.Value.EscalationRequired);
        Assert.True(result.Value.RequiresFleetFollowUp);
        Assert.True(result.Value.RequiresComplianceFollowUp);
        Assert.Contains(result.Value.Participants, x => x.Type == TrainingIncidentParticipantType.Student);
        Assert.Contains(result.Value.Participants, x => x.Type == TrainingIncidentParticipantType.Instructor);
        Assert.Contains(result.Value.Participants, x => x.Type == TrainingIncidentParticipantType.Vehicle);
        Assert.Contains(result.Value.Participants, x => x.Type == TrainingIncidentParticipantType.ThirdParty);
        Assert.Single(result.Value.History);
    }

    [Fact]
    public void Resolve_CriticalIncident_ShouldRequireEscalationFirst()
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;
        UserId actor = new(Guid.NewGuid());
        TrainingIncident incident = CreateCritical(now, actor);

        var rejected = incident.Resolve(Guid.NewGuid(), "Analyse terminée.", actor, now.AddMinutes(10));
        Assert.True(rejected.IsFailure);
        Assert.Equal(TrainingIncidentErrors.CriticalMustBeEscalated, rejected.Error);

        Assert.True(incident.Escalate(Guid.NewGuid(), "Escalade au responsable sécurité.", actor, now.AddMinutes(11)).IsSuccess);
        Assert.True(incident.Resolve(Guid.NewGuid(), "Véhicule immobilisé et dossier transmis.", actor, now.AddMinutes(20)).IsSuccess);
        Assert.Equal(TrainingIncidentStatus.Resolved, incident.Status);
    }

    [Fact]
    public void Mutation_ShouldBeIdempotentAndRejectConflictingRetry()
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;
        UserId actor = new(Guid.NewGuid());
        TrainingIncident incident = CreateCritical(now, actor);
        Guid operationId = Guid.NewGuid();

        var first = incident.Escalate(operationId, "Escalade sécurité", actor, now.AddMinutes(1));
        var retry = incident.Escalate(operationId, "Escalade sécurité", actor, now.AddMinutes(2));
        var conflict = incident.Escalate(operationId, "Autre motif", actor, now.AddMinutes(3));

        Assert.True(first.IsSuccess);
        Assert.True(retry.IsSuccess);
        Assert.True(conflict.IsFailure);
        Assert.Equal(TrainingIncidentErrors.OperationConflict, conflict.Error);
        Assert.Equal(2, incident.History.Count); // Report + single escalation.
    }

    [Fact]
    public void Close_ShouldRequireResolvedStatus()
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;
        UserId actor = new(Guid.NewGuid());
        TrainingIncident incident = CreateCritical(now, actor);
        Assert.True(incident.Escalate(Guid.NewGuid(), "Escalade", actor, now.AddMinutes(1)).IsSuccess);

        var premature = incident.Close(Guid.NewGuid(), null, actor, now.AddMinutes(2));
        Assert.True(premature.IsFailure);
        Assert.Equal(TrainingIncidentErrors.ResolutionRequired, premature.Error);
    }

    private static TrainingIncident CreateCritical(DateTimeOffset now, UserId actor) => TrainingIncident.Report(
        TrainingIncidentId.New(), new OrganizationId(Guid.NewGuid()), TrainingSessionId.New(), new PersonId(Guid.NewGuid()),
        new UserId(Guid.NewGuid()), Guid.NewGuid(), new BranchId(Guid.NewGuid()), new OrganizationId(Guid.NewGuid()),
        Guid.NewGuid(), TrainingIncidentType.Accident, TrainingIncidentSeverity.Critical, now,
        "Accident pendant la séance", "Sécurisation immédiate", [], actor, now).Value;
}
