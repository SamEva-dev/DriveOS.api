using DriveOS.Modules.TrainingDelivery.Domain.Sessions;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.TrainingDelivery.Application.Sessions;

public sealed record TrainingSessionAttendanceResponse(
    Guid Id,
    Guid OperationId,
    int Revision,
    int Status,
    DateTimeOffset? ActualArrivalAtUtc,
    DateTimeOffset? ActualDepartureAtUtc,
    int LateMinutes,
    string? Reason,
    Guid? EvidenceDocumentId,
    Guid RecordedByUserId,
    DateTimeOffset RecordedAtUtc,
    Guid? SupersedesAttendanceId,
    bool IsOverride,
    string? OverrideReason);

public sealed record TrainingSessionInterventionResponse(
    Guid Id,
    Guid OperationId,
    int Type,
    int Severity,
    DateTimeOffset OccurredAtUtc,
    string Context,
    string Reason,
    Guid? RelatedCompetencyId,
    string? Outcome,
    string? SharedExplanation,
    Guid RecordedByUserId,
    DateTimeOffset RecordedAtUtc);

public sealed record TrainingSessionObservationResponse(
    Guid Id,
    Guid OperationId,
    int Type,
    DateTimeOffset ObservedAtUtc,
    string Content,
    bool IsInternal,
    Guid RecordedByUserId,
    DateTimeOffset RecordedAtUtc);

public sealed record TrainingSessionMarkerResponse(
    Guid Id,
    Guid OperationId,
    int Type,
    DateTimeOffset OccurredAtUtc,
    Guid? CompetencyId,
    string ShortNote,
    int Severity,
    decimal? Latitude,
    decimal? Longitude,
    bool CreatedOffline,
    Guid RecordedByUserId,
    DateTimeOffset RecordedAtUtc);

public sealed record TrainingSessionInterruptionResponse(
    Guid Id,
    Guid InterruptOperationId,
    int Reason,
    string? Description,
    DateTimeOffset StartedAtUtc,
    Guid InterruptedByUserId,
    Guid? ResumeOperationId,
    DateTimeOffset? ResumedAtUtc,
    Guid? ResumedByUserId,
    DateTimeOffset? TerminatedAtUtc,
    Guid? TerminatedByCancellationId,
    bool IsActive);

public sealed record TrainingSessionOdometerReadingResponse(
    Guid Id,
    Guid OperationId,
    decimal OdometerKilometers,
    int Source,
    DateTimeOffset ObservedAtUtc,
    Guid RecordedByUserId,
    DateTimeOffset RecordedAtUtc);

public sealed record TrainingSessionEnergyEntryResponse(
    Guid Id,
    Guid OperationId,
    int Type,
    decimal? EnergyLevelPercent,
    decimal? Quantity,
    DateTimeOffset ObservedAtUtc,
    string? Note,
    bool CreatedOffline,
    Guid RecordedByUserId,
    DateTimeOffset RecordedAtUtc);


public sealed record TrainingSessionCompetencyAssessmentResponse(
    Guid Id,
    Guid OperationId,
    Guid CompetencyId,
    Guid CurriculumVersionId,
    Guid PedagogyAssessmentId,
    string LevelCode,
    string? ObservedCriteria,
    string? Context,
    Guid? RelatedInterventionId,
    string? InternalComment,
    string? SharedComment,
    Guid? EvidenceDocumentId,
    DateTimeOffset AssessedAtUtc,
    Guid AssessorUserId,
    DateTimeOffset RecordedAtUtc);

public sealed record TrainingSessionReportResponse(
    Guid Id,
    Guid OperationId,
    int Status,
    int Version,
    int LastCompletedStep,
    DateTimeOffset ActualEndAtUtc,
    int GrossDurationMinutes,
    int InterruptionDurationMinutes,
    int DeliveredDurationMinutes,
    decimal? DistanceKilometers,
    string Summary,
    string? ObjectivesWorked,
    string? ObjectivesAchieved,
    string? NextObjective,
    string? SharedComment,
    string? InternalNote,
    string? InstructorComments,
    Guid LastSavedByUserId,
    DateTimeOffset LastSavedAtUtc,
    Guid CompletedByUserId,
    DateTimeOffset CompletedAtUtc);

public sealed record TrainingSessionResponse(
    Guid Id,
    Guid OrganizationId,
    Guid StudentOwnerOrganizationId,
    Guid PerformingOrganizationId,
    Guid SourceBookingId,
    Guid StudentId,
    Guid TrainingPathId,
    Guid InstructorId,
    Guid? BranchId,
    Guid? VehicleId,
    DateTimeOffset PlannedStartAtUtc,
    DateTimeOffset PlannedEndAtUtc,
    string? TrainingCategory,
    string? Objectives,
    string? MeetingPoint,
    string? PricingReference,
    Guid? TrainingCreditAccountId,
    decimal? CreditQuantity,
    string? CreditReservationReference,
    int Status,
    DateTimeOffset? ReadinessCheckedAtUtc,
    Guid? ReadinessCheckedByUserId,
    Guid? ReadyInstructorId,
    Guid? ReadyVehicleId,
    Guid? ReadyBranchId,
    DateTimeOffset? ReadyPlannedStartAtUtc,
    DateTimeOffset? ReadyPlannedEndAtUtc,
    Guid? ActualInstructorId,
    Guid? ActualVehicleId,
    Guid? ActualBranchId,
    DateTimeOffset? ActualStartAtUtc,
    Guid? StartedByUserId,
    Guid? CurrentAttendanceId,
    TrainingSessionAttendanceResponse? CurrentAttendance,
    IReadOnlyCollection<TrainingSessionAttendanceResponse> AttendanceHistory,
    IReadOnlyCollection<TrainingSessionInterventionResponse> Interventions,
    IReadOnlyCollection<TrainingSessionObservationResponse> Observations,
    IReadOnlyCollection<TrainingSessionMarkerResponse> Markers,
    IReadOnlyCollection<TrainingSessionInterruptionResponse> Interruptions,
    IReadOnlyCollection<TrainingSessionOdometerReadingResponse> OdometerReadings,
    IReadOnlyCollection<TrainingSessionEnergyEntryResponse> EnergyEntries,
    IReadOnlyCollection<TrainingSessionCompetencyAssessmentResponse> CompetencyAssessments,
    decimal? LatestOdometerKilometers,
    decimal? StartEnergyLevelPercent,
    decimal? LatestEnergyLevelPercent,
    decimal FuelAddedLiters,
    decimal ChargedEnergyKwh,
    DateTimeOffset? ActualEndAtUtc,
    decimal? EndEnergyLevelPercent,
    int? GrossDurationMinutes,
    int? InterruptionDurationMinutes,
    int? DeliveredDurationMinutes,
    decimal? DistanceKilometers,
    Guid? CompletionOperationId,
    Guid? CompletedByUserId,
    DateTimeOffset? CompletedAtUtc,
    Guid? CancellationId,
    DateTimeOffset? CancelledAtUtc,
    Guid? CancelledByUserId,
    TrainingSessionReportResponse? Report,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? LastModifiedAtUtc);

public sealed record TrainingSessionPreparationResponse(
    Guid SessionId,
    int SessionStatus,
    bool CanStart,
    DateTimeOffset CheckedAtUtc,
    Guid CurrentInstructorId,
    Guid? CurrentVehicleId,
    Guid? CurrentBranchId,
    DateTimeOffset CurrentPlannedStartAtUtc,
    DateTimeOffset CurrentPlannedEndAtUtc,
    IReadOnlyCollection<TrainingSessionReadinessCheck> Checks);

public interface ITrainingSessionReadService
{
    Task<TrainingSessionResponse?> GetAsync(
        OrganizationId organizationId,
        TrainingSessionId sessionId,
        CancellationToken cancellationToken = default);
}

public static class TrainingSessionMappings
{
    public static TrainingSessionResponse ToResponse(TrainingSession x) => new(
        x.Id.Value,
        x.OrganizationId.Value,
        x.StudentOwnerOrganizationId.Value,
        x.PerformingOrganizationId.Value,
        x.SourceBookingId.Value,
        x.StudentId.Value,
        x.TrainingPathId.Value,
        x.InstructorId.Value,
        x.BranchId?.Value,
        x.VehicleId,
        x.PlannedStartAtUtc,
        x.PlannedEndAtUtc,
        x.TrainingCategory,
        x.Objectives,
        x.MeetingPoint,
        x.PricingReference,
        x.TrainingCreditAccountId?.Value,
        x.CreditQuantity,
        x.CreditReservationReference,
        (int)x.Status,
        x.ReadinessCheckedAtUtc,
        x.ReadinessCheckedByUserId?.Value,
        x.ReadyInstructorId?.Value,
        x.ReadyVehicleId,
        x.ReadyBranchId?.Value,
        x.ReadyPlannedStartAtUtc,
        x.ReadyPlannedEndAtUtc,
        x.ActualInstructorId?.Value,
        x.ActualVehicleId,
        x.ActualBranchId?.Value,
        x.ActualStartAtUtc,
        x.StartedByUserId?.Value,
        x.CurrentAttendanceId?.Value,
        x.CurrentAttendanceId.HasValue
            ? ToAttendanceResponse(x.AttendanceHistory.First(a => a.Id == x.CurrentAttendanceId.Value))
            : null,
        x.AttendanceHistory.OrderBy(a => a.Revision).Select(ToAttendanceResponse).ToArray(),
        x.Interventions.OrderBy(i => i.OccurredAtUtc).Select(i => new TrainingSessionInterventionResponse(
            i.Id.Value, i.OperationId, (int)i.Type, (int)i.Severity, i.OccurredAtUtc, i.Context, i.Reason, i.RelatedCompetencyId?.Value, i.Outcome, i.SharedExplanation, i.RecordedByUserId.Value, i.RecordedAtUtc)).ToArray(),
        x.Observations.OrderBy(o => o.ObservedAtUtc).Select(o => new TrainingSessionObservationResponse(
            o.Id.Value, o.OperationId, (int)o.Type, o.ObservedAtUtc, o.Content, o.IsInternal, o.RecordedByUserId.Value, o.RecordedAtUtc)).ToArray(),
        x.Markers.OrderBy(m => m.OccurredAtUtc).Select(m => new TrainingSessionMarkerResponse(
            m.Id.Value, m.OperationId, (int)m.Type, m.OccurredAtUtc, m.CompetencyId?.Value, m.ShortNote, (int)m.Severity, m.Latitude, m.Longitude, m.CreatedOffline, m.RecordedByUserId.Value, m.RecordedAtUtc)).ToArray(),
        x.Interruptions.OrderBy(i => i.StartedAtUtc).Select(i => new TrainingSessionInterruptionResponse(
            i.Id.Value, i.InterruptOperationId, (int)i.Reason, i.Description, i.StartedAtUtc, i.InterruptedByUserId.Value, i.ResumeOperationId, i.ResumedAtUtc, i.ResumedByUserId?.Value, i.TerminatedAtUtc, i.TerminatedByCancellationId?.Value, i.IsActive)).ToArray(),
        x.OdometerReadings.OrderBy(o => o.ObservedAtUtc).Select(o => new TrainingSessionOdometerReadingResponse(
            o.Id.Value, o.OperationId, o.OdometerKilometers, (int)o.Source, o.ObservedAtUtc, o.RecordedByUserId.Value, o.RecordedAtUtc)).ToArray(),
        x.EnergyEntries.OrderBy(e => e.ObservedAtUtc).Select(e => new TrainingSessionEnergyEntryResponse(
            e.Id.Value, e.OperationId, (int)e.Type, e.EnergyLevelPercent, e.Quantity, e.ObservedAtUtc, e.Note, e.CreatedOffline, e.RecordedByUserId.Value, e.RecordedAtUtc)).ToArray(),
        x.CompetencyAssessments.OrderBy(a => a.AssessedAtUtc).Select(a => new TrainingSessionCompetencyAssessmentResponse(
            a.Id.Value, a.OperationId, a.CompetencyId.Value, a.CurriculumVersionId.Value, a.PedagogyAssessmentId, a.LevelCode,
            a.ObservedCriteria, a.Context, a.RelatedInterventionId?.Value, a.InternalComment, a.SharedComment, a.EvidenceDocumentId,
            a.AssessedAtUtc, a.AssessorUserId.Value, a.RecordedAtUtc)).ToArray(),
        x.LatestOdometerKilometers,
        x.StartEnergyLevelPercent,
        x.LatestEnergyLevelPercent,
        x.FuelAddedLiters,
        x.ChargedEnergyKwh,
        x.ActualEndAtUtc,
        x.EndEnergyLevelPercent,
        x.GrossDurationMinutes,
        x.InterruptionDurationMinutes,
        x.DeliveredDurationMinutes,
        x.DistanceKilometers,
        x.CompletionOperationId,
        x.CompletedByUserId?.Value,
        x.CompletedAtUtc,
        x.CancellationId?.Value,
        x.CancelledAtUtc,
        x.CancelledByUserId?.Value,
        x.Report is null ? null : new TrainingSessionReportResponse(
            x.Report.Id.Value, x.Report.OperationId, (int)x.Report.Status, x.Report.Version, x.Report.LastCompletedStep,
            x.Report.ActualEndAtUtc, x.Report.GrossDurationMinutes, x.Report.InterruptionDurationMinutes, x.Report.DeliveredDurationMinutes, x.Report.DistanceKilometers,
            x.Report.Summary, x.Report.ObjectivesWorked, x.Report.ObjectivesAchieved, x.Report.NextObjective, x.Report.SharedComment, null,
            null, x.Report.LastSavedByUserId.Value, x.Report.LastSavedAtUtc, x.Report.CompletedByUserId.Value, x.Report.CompletedAtUtc),
        x.CreatedAtUtc,
        x.LastModifiedAtUtc);

    internal static TrainingSessionAttendanceResponse ToAttendanceResponse(SessionAttendance x) => new(
        x.Id.Value,
        x.OperationId,
        x.Revision,
        (int)x.Status,
        x.ActualArrivalAtUtc,
        x.ActualDepartureAtUtc,
        x.LateMinutes,
        x.Reason,
        x.EvidenceDocumentId,
        x.RecordedByUserId.Value,
        x.RecordedAtUtc,
        x.SupersedesAttendanceId?.Value,
        x.IsOverride,
        x.OverrideReason);

    internal static TrainingSessionPreparationResponse ToPreparationResponse(TrainingSession session, TrainingSessionExecutionReadiness readiness, DateTimeOffset checkedAtUtc) => new(
        session.Id.Value,
        (int)session.Status,
        readiness.IsReady && session.Status == TrainingSessionStatus.Ready,
        session.ReadinessCheckedAtUtc ?? checkedAtUtc,
        readiness.InstructorId.Value,
        readiness.VehicleId,
        readiness.BranchId?.Value,
        readiness.PlannedStartAtUtc,
        readiness.PlannedEndAtUtc,
        readiness.Checks);
}
