using DriveOS.Modules.TrainingDelivery.Domain.Sessions;
using DriveOS.Modules.TrainingDelivery.Domain.Cancellations;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.UnitTests.Modules.TrainingDelivery;

public sealed class TrainingSessionTests
{

    [Fact]
    public void Start_ShouldBeIdempotentByOperationId_AndRejectConflictingReplay()
    {
        DateTimeOffset plannedStart = DateTimeOffset.UtcNow.AddMinutes(5);
        var source = CreateSource(plannedStart, plannedStart.AddHours(1));
        TrainingSession session = TrainingSession.Materialize(
            TrainingSessionId.New(), source, null, plannedStart.AddHours(-1)).Value;
        var readiness = new TrainingSessionReadinessSnapshot(
            true, source.InstructorId, source.BranchId, source.VehicleId, source.PlannedStartAtUtc, source.PlannedEndAtUtc);
        UserId actor = new(Guid.NewGuid());
        Assert.True(session.MarkReady(readiness, actor, plannedStart.AddMinutes(-10), 30).IsSuccess);

        Guid operationId = Guid.NewGuid();
        Result first = session.Start(operationId, readiness, actor, plannedStart, 15, 180, 15);
        Result retry = session.Start(operationId, readiness, actor, plannedStart, 15, 180, 15);
        Result conflict = session.Start(operationId, readiness, actor, plannedStart.AddMinutes(1), 15, 180, 15);

        Assert.True(first.IsSuccess);
        Assert.True(retry.IsSuccess);
        Assert.True(conflict.IsFailure);
        Assert.Equal(TrainingSessionErrors.StartOperationConflict, conflict.Error);
        Assert.Equal(operationId, session.StartOperationId);
        Assert.Equal(plannedStart.ToUniversalTime(), session.ActualStartAtUtc);
    }
    [Fact]
    public void Materialize_ShouldPreserveConfirmedBookingSnapshot()
    {
        var bookingId = BookingId.New();
        DateTimeOffset now = DateTimeOffset.UtcNow;
        var source = CreateSource(now.AddHours(1), now.AddHours(2), bookingId);

        var result = TrainingSession.Materialize(TrainingSessionId.New(), source, new UserId(Guid.NewGuid()), now);

        Assert.True(result.IsSuccess);
        Assert.Equal(bookingId, result.Value.SourceBookingId);
        Assert.Equal(TrainingSessionStatus.Scheduled, result.Value.Status);
        Assert.Equal(source.PlannedStartAtUtc.ToUniversalTime(), result.Value.PlannedStartAtUtc);
    }

    [Fact]
    public void Materialize_ShouldRejectMissingTrainingPath()
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;
        var source = CreateSource(now.AddHours(1), now.AddHours(2)) with { TrainingPathId = TrainingPathId.Empty };

        var result = TrainingSession.Materialize(TrainingSessionId.New(), source, null, now);

        Assert.True(result.IsFailure);
        Assert.Equal(TrainingSessionErrors.InvalidTrainingPath, result.Error);
    }

    [Fact]
    public void MarkReady_ShouldPersistCurrentSchedulingResourcesWithoutOverwritingOriginalPlan()
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;
        var source = CreateSource(now.AddMinutes(20), now.AddHours(1));
        TrainingSession session = TrainingSession.Materialize(TrainingSessionId.New(), source, new UserId(Guid.NewGuid()), now.AddHours(-1)).Value;
        var replacementInstructor = new UserId(Guid.NewGuid());
        Guid replacementVehicle = Guid.NewGuid();
        var readiness = new TrainingSessionReadinessSnapshot(true, replacementInstructor, source.BranchId, replacementVehicle, now.AddMinutes(20), now.AddHours(1));

        var result = session.MarkReady(readiness, new UserId(Guid.NewGuid()), now, 30);

        Assert.True(result.IsSuccess);
        Assert.Equal(TrainingSessionStatus.Ready, session.Status);
        Assert.Equal(replacementInstructor, session.ReadyInstructorId);
        Assert.Equal(replacementVehicle, session.ReadyVehicleId);
        Assert.Equal(source.InstructorId, session.InstructorId);
        Assert.Equal(source.VehicleId, session.VehicleId);
    }

    [Fact]
    public void MarkReady_ShouldRejectPreparationTooEarly()
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;
        var source = CreateSource(now.AddHours(2), now.AddHours(3));
        TrainingSession session = TrainingSession.Materialize(TrainingSessionId.New(), source, null, now).Value;
        var readiness = new TrainingSessionReadinessSnapshot(true, source.InstructorId, source.BranchId, source.VehicleId, source.PlannedStartAtUtc, source.PlannedEndAtUtc);

        var result = session.MarkReady(readiness, new UserId(Guid.NewGuid()), now, 30);

        Assert.True(result.IsFailure);
        Assert.Equal(TrainingSessionErrors.PreparationTooEarly, result.Error);
        Assert.Equal(TrainingSessionStatus.Scheduled, session.Status);
    }

    [Fact]
    public void Start_ShouldRecordActualResourcesAndActualStart()
    {
        DateTimeOffset plannedStart = DateTimeOffset.UtcNow.AddMinutes(5);
        DateTimeOffset plannedEnd = plannedStart.AddHours(1);
        var source = CreateSource(plannedStart, plannedEnd);
        TrainingSession session = TrainingSession.Materialize(TrainingSessionId.New(), source, null, plannedStart.AddHours(-1)).Value;
        var actualInstructor = new UserId(Guid.NewGuid());
        Guid actualVehicle = Guid.NewGuid();
        var readiness = new TrainingSessionReadinessSnapshot(true, actualInstructor, source.BranchId, actualVehicle, plannedStart, plannedEnd);
        UserId actor = new(Guid.NewGuid());
        Assert.True(session.MarkReady(readiness, actor, plannedStart.AddMinutes(-10), 30).IsSuccess);

        DateTimeOffset actualStart = plannedStart.AddMinutes(-2);
        var result = session.Start(readiness, actor, actualStart, 15, 180, 15);

        Assert.True(result.IsSuccess);
        Assert.Equal(TrainingSessionStatus.InProgress, session.Status);
        Assert.Equal(actualInstructor, session.ActualInstructorId);
        Assert.Equal(actualVehicle, session.ActualVehicleId);
        Assert.Equal(actualStart.ToUniversalTime(), session.ActualStartAtUtc);
        Assert.Equal(actor, session.StartedByUserId);
    }

    [Fact]
    public void Start_ShouldRejectExpiredReadiness()
    {
        DateTimeOffset plannedStart = DateTimeOffset.UtcNow.AddMinutes(5);
        var source = CreateSource(plannedStart, plannedStart.AddHours(1));
        TrainingSession session = TrainingSession.Materialize(TrainingSessionId.New(), source, null, plannedStart.AddHours(-1)).Value;
        var readiness = new TrainingSessionReadinessSnapshot(true, source.InstructorId, source.BranchId, source.VehicleId, source.PlannedStartAtUtc, source.PlannedEndAtUtc);
        UserId actor = new(Guid.NewGuid());
        Assert.True(session.MarkReady(readiness, actor, plannedStart.AddMinutes(-20), 30).IsSuccess);

        var result = session.Start(readiness, actor, plannedStart, 15, 180, 5);

        Assert.True(result.IsFailure);
        Assert.Equal(TrainingSessionErrors.ReadinessExpired, result.Error);
        Assert.Equal(TrainingSessionStatus.Ready, session.Status);
    }


    [Fact]
    public void RecordAttendance_ShouldCreateAuthoritativeAppendOnlyRecord()
    {
        DateTimeOffset plannedStart = DateTimeOffset.UtcNow.AddMinutes(5);
        var source = CreateSource(plannedStart, plannedStart.AddHours(1));
        TrainingSession session = TrainingSession.Materialize(TrainingSessionId.New(), source, null, plannedStart.AddHours(-1)).Value;
        var readiness = new TrainingSessionReadinessSnapshot(true, source.InstructorId, source.BranchId, source.VehicleId, source.PlannedStartAtUtc, source.PlannedEndAtUtc);
        UserId actor = new(Guid.NewGuid());
        Assert.True(session.MarkReady(readiness, actor, plannedStart.AddMinutes(-10), 30).IsSuccess);
        Assert.True(session.Start(readiness, actor, plannedStart, 15, 180, 15).IsSuccess);

        Guid operationId = Guid.NewGuid();
        var result = session.RecordAttendance(operationId, TrainingSessionAttendanceStatus.Present, plannedStart, null, null, null, actor, plannedStart.AddMinutes(1), 30);

        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value.Id, session.CurrentAttendanceId);
        Assert.Single(session.AttendanceHistory);
        Assert.Equal(1, result.Value.Revision);
        Assert.Equal(actor, result.Value.RecordedByUserId);
    }

    [Fact]
    public void RecordAttendance_ShouldBeIdempotentForSameOperation()
    {
        DateTimeOffset plannedStart = DateTimeOffset.UtcNow.AddMinutes(5);
        var source = CreateSource(plannedStart, plannedStart.AddHours(1));
        TrainingSession session = TrainingSession.Materialize(TrainingSessionId.New(), source, null, plannedStart.AddHours(-1)).Value;
        var readiness = new TrainingSessionReadinessSnapshot(true, source.InstructorId, source.BranchId, source.VehicleId, source.PlannedStartAtUtc, source.PlannedEndAtUtc);
        UserId actor = new(Guid.NewGuid());
        Assert.True(session.MarkReady(readiness, actor, plannedStart.AddMinutes(-10), 30).IsSuccess);
        Assert.True(session.Start(readiness, actor, plannedStart, 15, 180, 15).IsSuccess);
        Guid operationId = Guid.NewGuid();

        var first = session.RecordAttendance(operationId, TrainingSessionAttendanceStatus.Present, plannedStart, null, null, null, actor, plannedStart.AddMinutes(1), 30);
        var retry = session.RecordAttendance(operationId, TrainingSessionAttendanceStatus.Present, plannedStart, null, null, null, actor, plannedStart.AddMinutes(2), 30);

        Assert.True(first.IsSuccess);
        Assert.True(retry.IsSuccess);
        Assert.Equal(first.Value.Id, retry.Value.Id);
        Assert.Single(session.AttendanceHistory);
    }


    [Fact]
    public void RecordAttendance_ShouldRejectSameOperationWithDifferentPayload()
    {
        DateTimeOffset plannedStart = DateTimeOffset.UtcNow.AddMinutes(5);
        var source = CreateSource(plannedStart, plannedStart.AddHours(1));
        TrainingSession session = TrainingSession.Materialize(TrainingSessionId.New(), source, null, plannedStart.AddHours(-1)).Value;
        var readiness = new TrainingSessionReadinessSnapshot(true, source.InstructorId, source.BranchId, source.VehicleId, source.PlannedStartAtUtc, source.PlannedEndAtUtc);
        UserId actor = new(Guid.NewGuid());
        Assert.True(session.MarkReady(readiness, actor, plannedStart.AddMinutes(-10), 30).IsSuccess);
        Assert.True(session.Start(readiness, actor, plannedStart, 15, 180, 15).IsSuccess);
        Guid operationId = Guid.NewGuid();
        Assert.True(session.RecordAttendance(operationId, TrainingSessionAttendanceStatus.Present, plannedStart, null, null, null, actor, plannedStart.AddMinutes(1), 30).IsSuccess);

        var result = session.RecordAttendance(operationId, TrainingSessionAttendanceStatus.LateArrival, plannedStart.AddMinutes(5), null, null, null, actor, plannedStart.AddMinutes(6), 30);

        Assert.True(result.IsFailure);
        Assert.Equal(TrainingSessionErrors.AttendanceOperationConflict, result.Error);
    }

    [Fact]
    public void CorrectAttendance_ShouldAppendRevisionAndPreservePreviousObservation()
    {
        DateTimeOffset plannedStart = DateTimeOffset.UtcNow.AddMinutes(5);
        var source = CreateSource(plannedStart, plannedStart.AddHours(1));
        TrainingSession session = TrainingSession.Materialize(TrainingSessionId.New(), source, null, plannedStart.AddHours(-1)).Value;
        var readiness = new TrainingSessionReadinessSnapshot(true, source.InstructorId, source.BranchId, source.VehicleId, source.PlannedStartAtUtc, source.PlannedEndAtUtc);
        UserId actor = new(Guid.NewGuid());
        Assert.True(session.MarkReady(readiness, actor, plannedStart.AddMinutes(-10), 30).IsSuccess);
        Assert.True(session.Start(readiness, actor, plannedStart, 15, 180, 15).IsSuccess);
        SessionAttendance first = session.RecordAttendance(Guid.NewGuid(), TrainingSessionAttendanceStatus.Present, plannedStart, null, null, null, actor, plannedStart.AddMinutes(1), 30).Value;

        var correction = session.CorrectAttendance(Guid.NewGuid(), TrainingSessionAttendanceStatus.LateArrival, plannedStart.AddMinutes(7), null, "Correction terrain", null, actor, plannedStart.AddHours(1), 24, false, null);

        Assert.True(correction.IsSuccess);
        Assert.Equal(2, correction.Value.Revision);
        Assert.Equal(first.Id, correction.Value.SupersedesAttendanceId);
        Assert.Equal(2, session.AttendanceHistory.Count);
        Assert.Equal(correction.Value.Id, session.CurrentAttendanceId);
        Assert.Equal(7, correction.Value.LateMinutes);
    }

    [Fact]
    public void CorrectAttendance_ShouldRequireOverrideAfterCorrectionWindow()
    {
        DateTimeOffset plannedStart = DateTimeOffset.UtcNow.AddMinutes(5);
        var source = CreateSource(plannedStart, plannedStart.AddHours(1));
        TrainingSession session = TrainingSession.Materialize(TrainingSessionId.New(), source, null, plannedStart.AddHours(-1)).Value;
        var readiness = new TrainingSessionReadinessSnapshot(true, source.InstructorId, source.BranchId, source.VehicleId, source.PlannedStartAtUtc, source.PlannedEndAtUtc);
        UserId actor = new(Guid.NewGuid());
        Assert.True(session.MarkReady(readiness, actor, plannedStart.AddMinutes(-10), 30).IsSuccess);
        Assert.True(session.Start(readiness, actor, plannedStart, 15, 180, 15).IsSuccess);
        session.RecordAttendance(Guid.NewGuid(), TrainingSessionAttendanceStatus.Present, plannedStart, null, null, null, actor, plannedStart.AddMinutes(1), 30);

        var rejected = session.CorrectAttendance(Guid.NewGuid(), TrainingSessionAttendanceStatus.LateArrival, plannedStart.AddMinutes(4), null, null, null, actor, plannedStart.AddHours(26), 24, false, null);
        var overridden = session.CorrectAttendance(Guid.NewGuid(), TrainingSessionAttendanceStatus.LateArrival, plannedStart.AddMinutes(4), null, null, null, actor, plannedStart.AddHours(26), 24, true, "Validation responsable pédagogique");

        Assert.True(rejected.IsFailure);
        Assert.Equal(TrainingSessionErrors.AttendanceCorrectionWindowExpired, rejected.Error);
        Assert.True(overridden.IsSuccess);
        Assert.True(overridden.Value.IsOverride);
    }

    [Fact]
    public void RecordAttendance_ShouldAllowStudentAbsenceWithoutStartingSession()
    {
        DateTimeOffset plannedStart = DateTimeOffset.UtcNow.AddMinutes(5);
        var source = CreateSource(plannedStart, plannedStart.AddHours(1));
        TrainingSession session = TrainingSession.Materialize(TrainingSessionId.New(), source, null, plannedStart.AddHours(-1)).Value;
        UserId actor = new(Guid.NewGuid());

        var result = session.RecordAttendance(Guid.NewGuid(), TrainingSessionAttendanceStatus.StudentAbsent, null, null, "Élève non présenté", null, actor, plannedStart, 30);

        Assert.True(result.IsSuccess);
        Assert.Equal(TrainingSessionAttendanceStatus.StudentAbsent, result.Value.Status);
        Assert.Null(result.Value.ActualArrivalAtUtc);
    }


    [Fact]
    public void RecordIntervention_ShouldBeIdempotentAndKeepActualOccurrenceTime()
    {
        DateTimeOffset plannedStart = DateTimeOffset.UtcNow.AddMinutes(5);
        var source = CreateSource(plannedStart, plannedStart.AddHours(1));
        TrainingSession session = TrainingSession.Materialize(TrainingSessionId.New(), source, null, plannedStart.AddHours(-1)).Value;
        var readiness = new TrainingSessionReadinessSnapshot(true, source.InstructorId, source.BranchId, source.VehicleId, source.PlannedStartAtUtc, source.PlannedEndAtUtc);
        UserId actor = new(Guid.NewGuid());
        Assert.True(session.MarkReady(readiness, actor, plannedStart.AddMinutes(-10), 30).IsSuccess);
        Assert.True(session.Start(readiness, actor, plannedStart, 15, 180, 15).IsSuccess);
        Guid operationId = Guid.NewGuid();
        DateTimeOffset occurredAt = plannedStart.AddMinutes(12);

        var first = session.RecordIntervention(operationId, TrainingSessionInterventionType.DualControlUse, TrainingSessionInterventionSeverity.Significant, occurredAt, "Priorité non détectée à une intersection", "Sécuriser la trajectoire", null, "Situation maîtrisée", "", "Explication donnée à l’élève", actor, occurredAt.AddMinutes(2));
        var retry = session.RecordIntervention(operationId, TrainingSessionInterventionType.DualControlUse, TrainingSessionInterventionSeverity.Significant, occurredAt, "Priorité non détectée à une intersection", "Sécuriser la trajectoire", null, "Situation maîtrisée", "", "Explication donnée à l’élève", actor, occurredAt.AddMinutes(3));

        Assert.True(first.IsSuccess);
        Assert.True(retry.IsSuccess);
        Assert.Equal(first.Value.Id, retry.Value.Id);
        Assert.Single(session.Interventions);
        Assert.Equal(occurredAt.ToUniversalTime(), first.Value.OccurredAtUtc);
    }

    [Fact]
    public void InterruptAndResume_ShouldPreserveActualTimesAndHistory()
    {
        DateTimeOffset plannedStart = DateTimeOffset.UtcNow.AddMinutes(5);
        var source = CreateSource(plannedStart, plannedStart.AddHours(1));
        TrainingSession session = TrainingSession.Materialize(TrainingSessionId.New(), source, null, plannedStart.AddHours(-1)).Value;
        var readiness = new TrainingSessionReadinessSnapshot(true, source.InstructorId, source.BranchId, source.VehicleId, source.PlannedStartAtUtc, source.PlannedEndAtUtc);
        UserId actor = new(Guid.NewGuid());
        Assert.True(session.MarkReady(readiness, actor, plannedStart.AddMinutes(-10), 30).IsSuccess);
        Assert.True(session.Start(readiness, actor, plannedStart, 15, 180, 15).IsSuccess);
        DateTimeOffset interruptedAt = plannedStart.AddMinutes(20);
        DateTimeOffset resumedAt = plannedStart.AddMinutes(27);

        var interrupted = session.Interrupt(Guid.NewGuid(), TrainingSessionInterruptionReason.VehicleIssue, "Voyant moteur", interruptedAt, actor, interruptedAt.AddMinutes(1));
        Assert.True(interrupted.IsSuccess);
        Assert.Equal(TrainingSessionStatus.Interrupted, session.Status);

        Guid resumeOperationId = Guid.NewGuid();
        var resumed = session.Resume(resumeOperationId, resumedAt, actor, resumedAt.AddMinutes(1));
        var retry = session.Resume(resumeOperationId, resumedAt, actor, resumedAt.AddMinutes(2));

        Assert.True(resumed.IsSuccess);
        Assert.True(retry.IsSuccess);
        Assert.Equal(TrainingSessionStatus.InProgress, session.Status);
        Assert.Single(session.Interruptions);
        Assert.Equal(interruptedAt.ToUniversalTime(), session.Interruptions.Single().StartedAtUtc);
        Assert.Equal(resumedAt.ToUniversalTime(), session.Interruptions.Single().ResumedAtUtc);
    }

    [Fact]
    public void RecordOdometer_ShouldRequireMonotonicChronologicalReadings()
    {
        DateTimeOffset plannedStart = DateTimeOffset.UtcNow.AddMinutes(5);
        var source = CreateSource(plannedStart, plannedStart.AddHours(1));
        TrainingSession session = TrainingSession.Materialize(TrainingSessionId.New(), source, null, plannedStart.AddHours(-1)).Value;
        var readiness = new TrainingSessionReadinessSnapshot(true, source.InstructorId, source.BranchId, source.VehicleId, source.PlannedStartAtUtc, source.PlannedEndAtUtc);
        UserId actor = new(Guid.NewGuid());
        Assert.True(session.MarkReady(readiness, actor, plannedStart.AddMinutes(-10), 30).IsSuccess);
        Assert.True(session.Start(readiness, actor, plannedStart, 15, 180, 15).IsSuccess);

        var first = session.RecordOdometer(Guid.NewGuid(), 50210.4m, TrainingSessionOdometerSource.Manual, plannedStart.AddMinutes(1), actor, plannedStart.AddMinutes(2));
        var lower = session.RecordOdometer(Guid.NewGuid(), 50209.9m, TrainingSessionOdometerSource.Manual, plannedStart.AddMinutes(10), actor, plannedStart.AddMinutes(11));

        Assert.True(first.IsSuccess);
        Assert.True(lower.IsFailure);
        Assert.Equal(TrainingSessionErrors.OdometerMustBeMonotonic, lower.Error);
        Assert.Equal(50210.4m, session.LatestOdometerKilometers);
    }

    [Fact]
    public void RecordEnergy_ShouldDeriveStartLatestAndTotals()
    {
        DateTimeOffset plannedStart = DateTimeOffset.UtcNow.AddMinutes(5);
        var source = CreateSource(plannedStart, plannedStart.AddHours(1));
        TrainingSession session = TrainingSession.Materialize(TrainingSessionId.New(), source, null, plannedStart.AddHours(-1)).Value;
        var readiness = new TrainingSessionReadinessSnapshot(true, source.InstructorId, source.BranchId, source.VehicleId, source.PlannedStartAtUtc, source.PlannedEndAtUtc);
        UserId actor = new(Guid.NewGuid());
        Assert.True(session.MarkReady(readiness, actor, plannedStart.AddMinutes(-10), 30).IsSuccess);
        Assert.True(session.Start(readiness, actor, plannedStart, 15, 180, 15).IsSuccess);

        Assert.True(session.RecordEnergy(Guid.NewGuid(), TrainingSessionEnergyEntryType.LevelObservation, 72m, null, plannedStart.AddMinutes(1), null, false, actor, plannedStart.AddMinutes(2)).IsSuccess);
        Assert.True(session.RecordEnergy(Guid.NewGuid(), TrainingSessionEnergyEntryType.FuelAdded, 75m, 8.5m, plannedStart.AddMinutes(20), "Appoint carburant", false, actor, plannedStart.AddMinutes(21)).IsSuccess);
        Assert.True(session.RecordEnergy(Guid.NewGuid(), TrainingSessionEnergyEntryType.Charging, 80m, 4.2m, plannedStart.AddMinutes(30), null, false, actor, plannedStart.AddMinutes(31)).IsSuccess);

        Assert.Equal(72m, session.StartEnergyLevelPercent);
        Assert.Equal(80m, session.LatestEnergyLevelPercent);
        Assert.Equal(8.5m, session.FuelAddedLiters);
        Assert.Equal(4.2m, session.ChargedEnergyKwh);
    }

    [Fact]
    public void RecordEnergy_ShouldBeIdempotentAndRejectConflictingRetry()
    {
        DateTimeOffset plannedStart = DateTimeOffset.UtcNow.AddMinutes(5);
        var source = CreateSource(plannedStart, plannedStart.AddHours(1));
        TrainingSession session = TrainingSession.Materialize(TrainingSessionId.New(), source, null, plannedStart.AddHours(-1)).Value;
        var readiness = new TrainingSessionReadinessSnapshot(true, source.InstructorId, source.BranchId, source.VehicleId, source.PlannedStartAtUtc, source.PlannedEndAtUtc);
        UserId actor = new(Guid.NewGuid());
        Assert.True(session.MarkReady(readiness, actor, plannedStart.AddMinutes(-10), 30).IsSuccess);
        Assert.True(session.Start(readiness, actor, plannedStart, 15, 180, 15).IsSuccess);
        Guid operationId = Guid.NewGuid();

        var first = session.RecordEnergy(operationId, TrainingSessionEnergyEntryType.LevelObservation, 60m, null, plannedStart.AddMinutes(1), null, true, actor, plannedStart.AddMinutes(2));
        var retry = session.RecordEnergy(operationId, TrainingSessionEnergyEntryType.LevelObservation, 60m, null, plannedStart.AddMinutes(1), null, true, actor, plannedStart.AddMinutes(3));
        var conflict = session.RecordEnergy(operationId, TrainingSessionEnergyEntryType.LevelObservation, 59m, null, plannedStart.AddMinutes(1), null, true, actor, plannedStart.AddMinutes(4));

        Assert.True(first.IsSuccess);
        Assert.True(retry.IsSuccess);
        Assert.True(conflict.IsFailure);
        Assert.Equal(TrainingSessionErrors.EnergyOperationConflict, conflict.Error);
        Assert.Single(session.EnergyEntries);
    }

    [Fact]
    public void RecordObservation_ShouldAllowObservationWhileInterrupted()
    {
        DateTimeOffset plannedStart = DateTimeOffset.UtcNow.AddMinutes(5);
        var source = CreateSource(plannedStart, plannedStart.AddHours(1));
        TrainingSession session = TrainingSession.Materialize(TrainingSessionId.New(), source, null, plannedStart.AddHours(-1)).Value;
        var readiness = new TrainingSessionReadinessSnapshot(true, source.InstructorId, source.BranchId, source.VehicleId, source.PlannedStartAtUtc, source.PlannedEndAtUtc);
        UserId actor = new(Guid.NewGuid());
        Assert.True(session.MarkReady(readiness, actor, plannedStart.AddMinutes(-10), 30).IsSuccess);
        Assert.True(session.Start(readiness, actor, plannedStart, 15, 180, 15).IsSuccess);
        DateTimeOffset interruptedAt = plannedStart.AddMinutes(15);
        Assert.True(session.Interrupt(Guid.NewGuid(), TrainingSessionInterruptionReason.SafetyIncident, "Pause sécurité", interruptedAt, actor, interruptedAt.AddMinutes(1)).IsSuccess);

        var result = session.RecordObservation(Guid.NewGuid(), TrainingSessionObservationType.SituationEncountered, interruptedAt, "Intersection complexe ayant nécessité une pause pédagogique.", true, actor, interruptedAt.AddMinutes(2));

        Assert.True(result.IsSuccess);
        Assert.Single(session.Observations);
        Assert.True(result.Value.IsInternal);
    }


    [Fact]
    public void Complete_ShouldCalculateDeliveredDurationDistanceAndFinalizeAttendance()
    {
        DateTimeOffset start = DateTimeOffset.UtcNow.AddHours(-1);
        var source = CreateSource(start, start.AddHours(1));
        TrainingSession session = TrainingSession.Materialize(TrainingSessionId.New(), source, null, start.AddHours(-1)).Value;
        var readiness = new TrainingSessionReadinessSnapshot(true, source.InstructorId, source.BranchId, source.VehicleId, source.PlannedStartAtUtc, source.PlannedEndAtUtc);
        UserId actor = new(Guid.NewGuid());
        Assert.True(session.MarkReady(readiness, actor, start.AddMinutes(-10), 30).IsSuccess);
        Assert.True(session.Start(readiness, actor, start, 15, 180, 15).IsSuccess);
        Assert.True(session.RecordAttendance(Guid.NewGuid(), TrainingSessionAttendanceStatus.Present, start, null, null, null, actor, start.AddMinutes(1), 30).IsSuccess);
        Assert.True(session.RecordOdometer(Guid.NewGuid(), 1000m, TrainingSessionOdometerSource.Manual, start.AddMinutes(1), actor, start.AddMinutes(2)).IsSuccess);
        Assert.True(session.Interrupt(Guid.NewGuid(), TrainingSessionInterruptionReason.Break, "Pause pédagogique", start.AddMinutes(20), actor, start.AddMinutes(20)).IsSuccess);
        Assert.True(session.Resume(Guid.NewGuid(), start.AddMinutes(25), actor, start.AddMinutes(25)).IsSuccess);
        Assert.True(session.RecordOdometer(Guid.NewGuid(), 1012.5m, TrainingSessionOdometerSource.Manual, start.AddMinutes(55), actor, start.AddMinutes(56)).IsSuccess);

        Guid operationId = Guid.NewGuid();
        var result = session.Complete(operationId, start.AddMinutes(60), "Séance réalisée conformément aux objectifs.", "Intersections", "Contrôles visuels", "Autonomie en intersection", null, actor, start.AddMinutes(61));

        Assert.True(result.IsSuccess);
        Assert.Equal(TrainingSessionStatus.Completed, session.Status);
        Assert.Equal(60, session.GrossDurationMinutes);
        Assert.Equal(5, session.InterruptionDurationMinutes);
        Assert.Equal(55, session.DeliveredDurationMinutes);
        Assert.Equal(12.5m, session.DistanceKilometers);
        Assert.NotNull(session.Report);
        Assert.Equal(start.AddMinutes(60).ToUniversalTime(), session.ActualEndAtUtc);
        Assert.Equal(start.AddMinutes(60).ToUniversalTime(), session.AttendanceHistory.OrderBy(x => x.Revision).Last().ActualDepartureAtUtc);
    }

    [Fact]
    public void Complete_ShouldBeIdempotentAndNotRepublishCompletionEvents()
    {
        DateTimeOffset start = DateTimeOffset.UtcNow.AddHours(-1);
        var source = CreateSource(start, start.AddHours(1));
        TrainingSession session = TrainingSession.Materialize(TrainingSessionId.New(), source, null, start.AddHours(-1)).Value;
        var readiness = new TrainingSessionReadinessSnapshot(true, source.InstructorId, source.BranchId, source.VehicleId, source.PlannedStartAtUtc, source.PlannedEndAtUtc);
        UserId actor = new(Guid.NewGuid());
        session.MarkReady(readiness, actor, start.AddMinutes(-10), 30);
        session.Start(readiness, actor, start, 15, 180, 15);
        session.RecordAttendance(Guid.NewGuid(), TrainingSessionAttendanceStatus.Present, start, null, null, null, actor, start.AddMinutes(1), 30);
        session.ClearDomainEvents();
        Guid operationId = Guid.NewGuid();

        var first = session.Complete(operationId, start.AddMinutes(55), "Compte rendu final.", null, null, null, null, actor, start.AddMinutes(56));
        int firstEventCount = session.DomainEvents.Count;
        var retry = session.Complete(operationId, start.AddMinutes(55), "Compte rendu final.", null, null, null, null, actor, start.AddMinutes(57));

        Assert.True(first.IsSuccess);
        Assert.True(retry.IsSuccess);
        Assert.Equal(first.Value.Id, retry.Value.Id);
        Assert.Equal(firstEventCount, session.DomainEvents.Count);
    }

    [Fact]
    public void Complete_ShouldRejectWhenInterruptionIsStillActive()
    {
        DateTimeOffset start = DateTimeOffset.UtcNow.AddHours(-1);
        var source = CreateSource(start, start.AddHours(1));
        TrainingSession session = TrainingSession.Materialize(TrainingSessionId.New(), source, null, start.AddHours(-1)).Value;
        var readiness = new TrainingSessionReadinessSnapshot(true, source.InstructorId, source.BranchId, source.VehicleId, source.PlannedStartAtUtc, source.PlannedEndAtUtc);
        UserId actor = new(Guid.NewGuid());
        session.MarkReady(readiness, actor, start.AddMinutes(-10), 30);
        session.Start(readiness, actor, start, 15, 180, 15);
        session.RecordAttendance(Guid.NewGuid(), TrainingSessionAttendanceStatus.Present, start, null, null, null, actor, start.AddMinutes(1), 30);
        session.Interrupt(Guid.NewGuid(), TrainingSessionInterruptionReason.VehicleIssue, "Panne", start.AddMinutes(20), actor, start.AddMinutes(20));

        var result = session.Complete(Guid.NewGuid(), start.AddMinutes(40), "Compte rendu.", null, null, null, null, actor, start.AddMinutes(41));

        Assert.True(result.IsFailure);
        Assert.Equal(TrainingSessionErrors.CompletionActiveInterruption, result.Error);
        Assert.Equal(TrainingSessionStatus.Interrupted, session.Status);
    }

    [Fact]
    public void RecordCompetencyAssessment_ShouldKeepSessionContextAndBeIdempotent()
    {
        DateTimeOffset start = DateTimeOffset.UtcNow.AddMinutes(-30);
        var source = CreateSource(start, start.AddHours(1));
        TrainingSession session = TrainingSession.Materialize(TrainingSessionId.New(), source, null, start.AddHours(-1)).Value;
        var readiness = new TrainingSessionReadinessSnapshot(true, source.InstructorId, source.BranchId, source.VehicleId, source.PlannedStartAtUtc, source.PlannedEndAtUtc);
        UserId actor = new(Guid.NewGuid());
        session.MarkReady(readiness, actor, start.AddMinutes(-10), 30);
        session.Start(readiness, actor, start, 15, 180, 15);
        Guid op = Guid.NewGuid();
        CompetencyId competencyId = new(Guid.NewGuid());
        CurriculumVersionId versionId = new(Guid.NewGuid());
        Guid pedagogyId = Guid.NewGuid();

        var first = session.RecordCompetencyAssessment(op, competencyId, versionId, pedagogyId, "IN_PROGRESS", "Observation correcte", "Intersection urbaine", null, "Note interne", "Bonne progression", null, start.AddMinutes(20), actor, start.AddMinutes(21));
        var retry = session.RecordCompetencyAssessment(op, competencyId, versionId, pedagogyId, "IN_PROGRESS", "Observation correcte", "Intersection urbaine", null, "Note interne", "Bonne progression", null, start.AddMinutes(20), actor, start.AddMinutes(22));

        Assert.True(first.IsSuccess);
        Assert.True(retry.IsSuccess);
        Assert.Equal(first.Value.Id, retry.Value.Id);
        Assert.Single(session.CompetencyAssessments);
        Assert.Equal(pedagogyId, first.Value.PedagogyAssessmentId);
    }

    [Fact]
    public void RecordCompetencyAssessment_ShouldRejectSecondAssessmentForSameCompetencyInSameSession()
    {
        DateTimeOffset start = DateTimeOffset.UtcNow.AddMinutes(-30);
        var source = CreateSource(start, start.AddHours(1));
        TrainingSession session = TrainingSession.Materialize(TrainingSessionId.New(), source, null, start.AddHours(-1)).Value;
        var readiness = new TrainingSessionReadinessSnapshot(true, source.InstructorId, source.BranchId, source.VehicleId, source.PlannedStartAtUtc, source.PlannedEndAtUtc);
        UserId actor = new(Guid.NewGuid());
        session.MarkReady(readiness, actor, start.AddMinutes(-10), 30);
        session.Start(readiness, actor, start, 15, 180, 15);
        CompetencyId competencyId = new(Guid.NewGuid());
        CurriculumVersionId versionId = new(Guid.NewGuid());
        Assert.True(session.RecordCompetencyAssessment(Guid.NewGuid(), competencyId, versionId, Guid.NewGuid(), "INTRODUCED", null, null, null, null, null, null, start.AddMinutes(10), actor, start.AddMinutes(11)).IsSuccess);

        var second = session.RecordCompetencyAssessment(Guid.NewGuid(), competencyId, versionId, Guid.NewGuid(), "ACQUIRED", null, null, null, null, null, null, start.AddMinutes(20), actor, start.AddMinutes(21));

        Assert.True(second.IsFailure);
        Assert.Equal(TrainingSessionErrors.AssessmentCompetencyAlreadyRecorded, second.Error);
    }


    [Fact]
    public void CancelDuringExecution_ShouldFinalizeActiveInterruptionAndActualDuration()
    {
        DateTimeOffset start = DateTimeOffset.UtcNow.AddHours(-1);
        var source = CreateSource(start, start.AddHours(1));
        TrainingSession session = TrainingSession.Materialize(TrainingSessionId.New(), source, null, start.AddHours(-1)).Value;
        var readiness = new TrainingSessionReadinessSnapshot(true, source.InstructorId, source.BranchId, source.VehicleId, source.PlannedStartAtUtc, source.PlannedEndAtUtc);
        UserId actor = new(Guid.NewGuid());
        session.MarkReady(readiness, actor, start.AddMinutes(-10), 30);
        session.Start(readiness, actor, start, 15, 180, 15);
        session.RecordAttendance(Guid.NewGuid(), TrainingSessionAttendanceStatus.Present, start, null, null, null, actor, start.AddMinutes(1), 30);
        session.Interrupt(Guid.NewGuid(), TrainingSessionInterruptionReason.VehicleIssue, "Panne", start.AddMinutes(20), actor, start.AddMinutes(20));
        SessionCancellationId cancellationId = SessionCancellationId.New();

        var result = session.CancelDuringExecution(cancellationId, start.AddMinutes(40), actor, start.AddMinutes(41));

        Assert.True(result.IsSuccess);
        Assert.Equal(TrainingSessionStatus.Cancelled, session.Status);
        Assert.Equal(cancellationId, session.CancellationId);
        Assert.Equal(20, result.Value.DeliveredDurationMinutes);
        Assert.False(session.Interruptions.Single().IsActive);
        Assert.Equal(cancellationId, session.Interruptions.Single().TerminatedByCancellationId);
        Assert.Equal(start.AddMinutes(40).ToUniversalTime(), session.AttendanceHistory.OrderBy(x => x.Revision).Last().ActualDepartureAtUtc);
    }

    [Fact]
    public void CancelDuringExecution_ShouldRejectPreStartCancellationOwnedByScheduling()
    {
        DateTimeOffset start = DateTimeOffset.UtcNow.AddHours(1);
        TrainingSession session = TrainingSession.Materialize(TrainingSessionId.New(), CreateSource(start, start.AddHours(1)), null, DateTimeOffset.UtcNow).Value;

        var result = session.CancelDuringExecution(SessionCancellationId.New(), DateTimeOffset.UtcNow, new UserId(Guid.NewGuid()), DateTimeOffset.UtcNow);

        Assert.True(result.IsFailure);
        Assert.Equal(SessionCancellationErrors.UseSchedulingBeforeStart, result.Error);
    }

    private static TrainingSessionMaterialization CreateSource(DateTimeOffset start, DateTimeOffset end, BookingId? bookingId = null) =>
        new(
            new OrganizationId(Guid.NewGuid()),
            new OrganizationId(Guid.NewGuid()),
            new OrganizationId(Guid.NewGuid()),
            bookingId ?? BookingId.New(),
            new PersonId(Guid.NewGuid()),
            TrainingPathId.New(),
            new UserId(Guid.NewGuid()),
            new BranchId(Guid.NewGuid()),
            Guid.NewGuid(),
            start,
            end,
            "B",
            "Intersections",
            "Agence",
            "price-v1",
            TrainingCreditAccountId.New(),
            1m,
            "credit-reservation");
}
