using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DriveOS.Modules.TrainingDelivery.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialTrainingDelivery : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "training_delivery");

            migrationBuilder.CreateTable(
                name: "session_cancellations",
                schema: "training_delivery",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    TrainingSessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceBookingId = table.Column<Guid>(type: "uuid", nullable: false),
                    StudentOwnerOrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    PerformingOrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    StudentId = table.Column<Guid>(type: "uuid", nullable: false),
                    InstructorId = table.Column<Guid>(type: "uuid", nullable: false),
                    VehicleId = table.Column<Guid>(type: "uuid", nullable: true),
                    BranchId = table.Column<Guid>(type: "uuid", nullable: true),
                    ActualStartAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CancelledAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    GrossDurationMinutes = table.Column<int>(type: "integer", nullable: false),
                    InterruptionDurationMinutes = table.Column<int>(type: "integer", nullable: false),
                    DeliveredDurationMinutes = table.Column<int>(type: "integer", nullable: false),
                    DistanceKilometers = table.Column<decimal>(type: "numeric", nullable: true),
                    Reason = table.Column<int>(type: "integer", nullable: false),
                    ReasonDetails = table.Column<string>(type: "character varying(3000)", maxLength: 3000, nullable: true),
                    BillingDecision = table.Column<int>(type: "integer", nullable: false),
                    CreditDecision = table.Column<int>(type: "integer", nullable: false),
                    PartialCreditQuantity = table.Column<decimal>(type: "numeric", nullable: true),
                    ProviderCompensationDecision = table.Column<int>(type: "integer", nullable: false),
                    DecisionReason = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    TrainingCreditAccountId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReservedCreditQuantity = table.Column<decimal>(type: "numeric", nullable: true),
                    CreditReservationReference = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    PricingReference = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    OperationId = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestFingerprint = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    CancelledByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModifiedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastModifiedByUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_session_cancellations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "training_incidents",
                schema: "training_delivery",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    TrainingSessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    StudentId = table.Column<Guid>(type: "uuid", nullable: false),
                    InstructorId = table.Column<Guid>(type: "uuid", nullable: false),
                    VehicleId = table.Column<Guid>(type: "uuid", nullable: true),
                    BranchId = table.Column<Guid>(type: "uuid", nullable: true),
                    PerformingOrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    IncidentType = table.Column<int>(type: "integer", nullable: false),
                    Severity = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    OccurredAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Description = table.Column<string>(type: "character varying(5000)", maxLength: 5000, nullable: false),
                    ImmediateActions = table.Column<string>(type: "character varying(3000)", maxLength: 3000, nullable: false),
                    EscalationRequired = table.Column<bool>(type: "boolean", nullable: false),
                    RequiresFleetFollowUp = table.Column<bool>(type: "boolean", nullable: false),
                    RequiresComplianceFollowUp = table.Column<bool>(type: "boolean", nullable: false),
                    EscalatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    EscalatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    Resolution = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    ResolvedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ResolvedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ClosedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ClosedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReportOperationId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReportRequestFingerprint = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModifiedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastModifiedByUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_training_incidents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "training_session_cancellation_consequences",
                schema: "training_delivery",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    SessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    CancellationId = table.Column<Guid>(type: "uuid", nullable: false),
                    Kind = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    PayloadJson = table.Column<string>(type: "jsonb", nullable: false),
                    AttemptCount = table.Column<int>(type: "integer", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastAttemptAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    NextAttemptAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ProcessedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastErrorCode = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    LastErrorDetail = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_training_session_cancellation_consequences", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "training_session_completion_consequences",
                schema: "training_delivery",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: false),
                    session_id = table.Column<Guid>(type: "uuid", nullable: false),
                    kind = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    payload_json = table.Column<string>(type: "jsonb", nullable: false),
                    attempt_count = table.Column<int>(type: "integer", nullable: false),
                    created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    last_attempt_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    next_attempt_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    processed_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    last_error_code = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: true),
                    last_error_detail = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_training_session_completion_consequences", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "training_sessions",
                schema: "training_delivery",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    StudentOwnerOrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    PerformingOrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceBookingId = table.Column<Guid>(type: "uuid", nullable: false),
                    StudentId = table.Column<Guid>(type: "uuid", nullable: false),
                    TrainingPathId = table.Column<Guid>(type: "uuid", nullable: false),
                    InstructorId = table.Column<Guid>(type: "uuid", nullable: false),
                    BranchId = table.Column<Guid>(type: "uuid", nullable: true),
                    VehicleId = table.Column<Guid>(type: "uuid", nullable: true),
                    PlannedStartAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    PlannedEndAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    TrainingCategory = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    Objectives = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    MeetingPoint = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    PricingReference = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    TrainingCreditAccountId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreditQuantity = table.Column<decimal>(type: "numeric", nullable: true),
                    CreditReservationReference = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    ReadinessCheckedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ReadinessCheckedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReadyInstructorId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReadyVehicleId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReadyBranchId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReadyPlannedStartAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ReadyPlannedEndAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ActualInstructorId = table.Column<Guid>(type: "uuid", nullable: true),
                    ActualVehicleId = table.Column<Guid>(type: "uuid", nullable: true),
                    ActualBranchId = table.Column<Guid>(type: "uuid", nullable: true),
                    ActualStartAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    StartedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CurrentAttendanceId = table.Column<Guid>(type: "uuid", nullable: true),
                    ActualEndAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    GrossDurationMinutes = table.Column<int>(type: "integer", nullable: true),
                    InterruptionDurationMinutes = table.Column<int>(type: "integer", nullable: true),
                    DeliveredDurationMinutes = table.Column<int>(type: "integer", nullable: true),
                    DistanceKilometers = table.Column<decimal>(type: "numeric", nullable: true),
                    CompletionOperationId = table.Column<Guid>(type: "uuid", nullable: true),
                    CompletionRequestFingerprint = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    CompletedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CompletedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CancellationId = table.Column<Guid>(type: "uuid", nullable: true),
                    CancelledAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CancelledByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModifiedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastModifiedByUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_training_sessions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "training_incident_evidence",
                schema: "training_delivery",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TrainingIncidentId = table.Column<Guid>(type: "uuid", nullable: false),
                    DocumentId = table.Column<Guid>(type: "uuid", nullable: false),
                    EvidenceType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    AddedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    AddedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_training_incident_evidence", x => x.Id);
                    table.ForeignKey(
                        name: "FK_training_incident_evidence_training_incidents_TrainingIncid~",
                        column: x => x.TrainingIncidentId,
                        principalSchema: "training_delivery",
                        principalTable: "training_incidents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "training_incident_history",
                schema: "training_delivery",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TrainingIncidentId = table.Column<Guid>(type: "uuid", nullable: false),
                    OperationId = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestFingerprint = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Action = table.Column<int>(type: "integer", nullable: false),
                    FromStatus = table.Column<int>(type: "integer", nullable: false),
                    ToStatus = table.Column<int>(type: "integer", nullable: false),
                    Reason = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    ActorUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    OccurredAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_training_incident_history", x => x.Id);
                    table.ForeignKey(
                        name: "FK_training_incident_history_training_incidents_TrainingIncide~",
                        column: x => x.TrainingIncidentId,
                        principalSchema: "training_delivery",
                        principalTable: "training_incidents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "training_incident_participants",
                schema: "training_delivery",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TrainingIncidentId = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    ReferenceId = table.Column<Guid>(type: "uuid", nullable: true),
                    Label = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_training_incident_participants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_training_incident_participants_training_incidents_TrainingI~",
                        column: x => x.TrainingIncidentId,
                        principalSchema: "training_delivery",
                        principalTable: "training_incidents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "session_attendance",
                schema: "training_delivery",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TrainingSessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    OperationId = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestFingerprint = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Revision = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    ActualArrivalAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ActualDepartureAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LateMinutes = table.Column<int>(type: "integer", nullable: false),
                    Reason = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    EvidenceDocumentId = table.Column<Guid>(type: "uuid", nullable: true),
                    RecordedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    RecordedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    SupersedesAttendanceId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsOverride = table.Column<bool>(type: "boolean", nullable: false),
                    OverrideReason = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_session_attendance", x => x.Id);
                    table.ForeignKey(
                        name: "FK_session_attendance_training_sessions_TrainingSessionId",
                        column: x => x.TrainingSessionId,
                        principalSchema: "training_delivery",
                        principalTable: "training_sessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "session_competency_assessments",
                schema: "training_delivery",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TrainingSessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    OperationId = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestFingerprint = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    CompetencyId = table.Column<Guid>(type: "uuid", nullable: false),
                    CurriculumVersionId = table.Column<Guid>(type: "uuid", nullable: false),
                    PedagogyAssessmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    LevelCode = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    ObservedCriteria = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    Context = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    RelatedInterventionId = table.Column<Guid>(type: "uuid", nullable: true),
                    InternalComment = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    SharedComment = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    EvidenceDocumentId = table.Column<Guid>(type: "uuid", nullable: true),
                    AssessedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    AssessorUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    RecordedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_session_competency_assessments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_session_competency_assessments_training_sessions_TrainingSe~",
                        column: x => x.TrainingSessionId,
                        principalSchema: "training_delivery",
                        principalTable: "training_sessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "session_interruptions",
                schema: "training_delivery",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TrainingSessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    InterruptOperationId = table.Column<Guid>(type: "uuid", nullable: false),
                    InterruptRequestFingerprint = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Reason = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "character varying(3000)", maxLength: 3000, nullable: true),
                    StartedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    InterruptedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ResumeOperationId = table.Column<Guid>(type: "uuid", nullable: true),
                    ResumeRequestFingerprint = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    ResumedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ResumedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    TerminatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    TerminatedByCancellationId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_session_interruptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_session_interruptions_training_sessions_TrainingSessionId",
                        column: x => x.TrainingSessionId,
                        principalSchema: "training_delivery",
                        principalTable: "training_sessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "session_interventions",
                schema: "training_delivery",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TrainingSessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    OperationId = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestFingerprint = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Severity = table.Column<int>(type: "integer", nullable: false),
                    OccurredAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Description = table.Column<string>(type: "character varying(3000)", maxLength: 3000, nullable: true),
                    RelatedObjective = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    RecordedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    RecordedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_session_interventions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_session_interventions_training_sessions_TrainingSessionId",
                        column: x => x.TrainingSessionId,
                        principalSchema: "training_delivery",
                        principalTable: "training_sessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "session_observations",
                schema: "training_delivery",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TrainingSessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    OperationId = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestFingerprint = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    ObservedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Content = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    IsInternal = table.Column<bool>(type: "boolean", nullable: false),
                    RecordedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    RecordedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_session_observations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_session_observations_training_sessions_TrainingSessionId",
                        column: x => x.TrainingSessionId,
                        principalSchema: "training_delivery",
                        principalTable: "training_sessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "session_odometer_readings",
                schema: "training_delivery",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TrainingSessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    OperationId = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestFingerprint = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    OdometerKilometers = table.Column<decimal>(type: "numeric(12,1)", precision: 12, scale: 1, nullable: false),
                    Source = table.Column<int>(type: "integer", nullable: false),
                    ObservedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    RecordedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    RecordedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_session_odometer_readings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_session_odometer_readings_training_sessions_TrainingSession~",
                        column: x => x.TrainingSessionId,
                        principalSchema: "training_delivery",
                        principalTable: "training_sessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "session_reports",
                schema: "training_delivery",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TrainingSessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    OperationId = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestFingerprint = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ActualEndAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    GrossDurationMinutes = table.Column<int>(type: "integer", nullable: false),
                    InterruptionDurationMinutes = table.Column<int>(type: "integer", nullable: false),
                    DeliveredDurationMinutes = table.Column<int>(type: "integer", nullable: false),
                    DistanceKilometers = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: true),
                    Summary = table.Column<string>(type: "character varying(5000)", maxLength: 5000, nullable: false),
                    ObjectivesWorked = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    ObjectivesAchieved = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    NextObjective = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    InstructorComments = table.Column<string>(type: "character varying(5000)", maxLength: 5000, nullable: true),
                    CompletedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CompletedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_session_reports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_session_reports_training_sessions_TrainingSessionId",
                        column: x => x.TrainingSessionId,
                        principalSchema: "training_delivery",
                        principalTable: "training_sessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_session_attendance_TrainingSessionId_OperationId",
                schema: "training_delivery",
                table: "session_attendance",
                columns: new[] { "TrainingSessionId", "OperationId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_session_attendance_TrainingSessionId_RecordedAtUtc",
                schema: "training_delivery",
                table: "session_attendance",
                columns: new[] { "TrainingSessionId", "RecordedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_session_attendance_TrainingSessionId_Revision",
                schema: "training_delivery",
                table: "session_attendance",
                columns: new[] { "TrainingSessionId", "Revision" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_session_cancellations_OrganizationId_CancelledAtUtc",
                schema: "training_delivery",
                table: "session_cancellations",
                columns: new[] { "OrganizationId", "CancelledAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_session_cancellations_OrganizationId_OperationId",
                schema: "training_delivery",
                table: "session_cancellations",
                columns: new[] { "OrganizationId", "OperationId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_session_cancellations_OrganizationId_TrainingSessionId",
                schema: "training_delivery",
                table: "session_cancellations",
                columns: new[] { "OrganizationId", "TrainingSessionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_session_competency_assessments_PedagogyAssessmentId",
                schema: "training_delivery",
                table: "session_competency_assessments",
                column: "PedagogyAssessmentId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_session_competency_assessments_TrainingSessionId_Competency~",
                schema: "training_delivery",
                table: "session_competency_assessments",
                columns: new[] { "TrainingSessionId", "CompetencyId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_session_competency_assessments_TrainingSessionId_OperationId",
                schema: "training_delivery",
                table: "session_competency_assessments",
                columns: new[] { "TrainingSessionId", "OperationId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_session_interruptions_TrainingSessionId_InterruptOperationId",
                schema: "training_delivery",
                table: "session_interruptions",
                columns: new[] { "TrainingSessionId", "InterruptOperationId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_session_interruptions_TrainingSessionId_ResumeOperationId",
                schema: "training_delivery",
                table: "session_interruptions",
                columns: new[] { "TrainingSessionId", "ResumeOperationId" },
                unique: true,
                filter: "\"ResumeOperationId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_session_interruptions_TrainingSessionId_StartedAtUtc",
                schema: "training_delivery",
                table: "session_interruptions",
                columns: new[] { "TrainingSessionId", "StartedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_session_interventions_TrainingSessionId_OccurredAtUtc",
                schema: "training_delivery",
                table: "session_interventions",
                columns: new[] { "TrainingSessionId", "OccurredAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_session_interventions_TrainingSessionId_OperationId",
                schema: "training_delivery",
                table: "session_interventions",
                columns: new[] { "TrainingSessionId", "OperationId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_session_observations_TrainingSessionId_ObservedAtUtc",
                schema: "training_delivery",
                table: "session_observations",
                columns: new[] { "TrainingSessionId", "ObservedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_session_observations_TrainingSessionId_OperationId",
                schema: "training_delivery",
                table: "session_observations",
                columns: new[] { "TrainingSessionId", "OperationId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_session_odometer_readings_TrainingSessionId_ObservedAtUtc",
                schema: "training_delivery",
                table: "session_odometer_readings",
                columns: new[] { "TrainingSessionId", "ObservedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_session_odometer_readings_TrainingSessionId_OperationId",
                schema: "training_delivery",
                table: "session_odometer_readings",
                columns: new[] { "TrainingSessionId", "OperationId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_session_reports_TrainingSessionId",
                schema: "training_delivery",
                table: "session_reports",
                column: "TrainingSessionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_session_reports_TrainingSessionId_OperationId",
                schema: "training_delivery",
                table: "session_reports",
                columns: new[] { "TrainingSessionId", "OperationId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_training_incident_evidence_TrainingIncidentId_DocumentId",
                schema: "training_delivery",
                table: "training_incident_evidence",
                columns: new[] { "TrainingIncidentId", "DocumentId" });

            migrationBuilder.CreateIndex(
                name: "IX_training_incident_history_TrainingIncidentId_OccurredAtUtc",
                schema: "training_delivery",
                table: "training_incident_history",
                columns: new[] { "TrainingIncidentId", "OccurredAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_training_incident_history_TrainingIncidentId_OperationId",
                schema: "training_delivery",
                table: "training_incident_history",
                columns: new[] { "TrainingIncidentId", "OperationId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_training_incident_participants_TrainingIncidentId_Type_Refe~",
                schema: "training_delivery",
                table: "training_incident_participants",
                columns: new[] { "TrainingIncidentId", "Type", "ReferenceId" });

            migrationBuilder.CreateIndex(
                name: "IX_training_incidents_OrganizationId_OccurredAtUtc",
                schema: "training_delivery",
                table: "training_incidents",
                columns: new[] { "OrganizationId", "OccurredAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_training_incidents_OrganizationId_Status_Severity",
                schema: "training_delivery",
                table: "training_incidents",
                columns: new[] { "OrganizationId", "Status", "Severity" });

            migrationBuilder.CreateIndex(
                name: "IX_training_incidents_OrganizationId_TrainingSessionId_ReportO~",
                schema: "training_delivery",
                table: "training_incidents",
                columns: new[] { "OrganizationId", "TrainingSessionId", "ReportOperationId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_training_session_cancellation_consequences_OrganizationId_C~",
                schema: "training_delivery",
                table: "training_session_cancellation_consequences",
                columns: new[] { "OrganizationId", "CancellationId", "Kind" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_training_session_cancellation_consequences_Status_LastAttem~",
                schema: "training_delivery",
                table: "training_session_cancellation_consequences",
                columns: new[] { "Status", "LastAttemptAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_training_session_cancellation_consequences_Status_NextAttem~",
                schema: "training_delivery",
                table: "training_session_cancellation_consequences",
                columns: new[] { "Status", "NextAttemptAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_training_session_completion_consequences_organization_id_se~",
                schema: "training_delivery",
                table: "training_session_completion_consequences",
                columns: new[] { "organization_id", "session_id", "kind" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_training_session_completion_consequences_status_last_attemp~",
                schema: "training_delivery",
                table: "training_session_completion_consequences",
                columns: new[] { "status", "last_attempt_at_utc" });

            migrationBuilder.CreateIndex(
                name: "IX_training_session_completion_consequences_status_next_attemp~",
                schema: "training_delivery",
                table: "training_session_completion_consequences",
                columns: new[] { "status", "next_attempt_at_utc" });

            migrationBuilder.CreateIndex(
                name: "IX_training_sessions_OrganizationId_PlannedStartAtUtc",
                schema: "training_delivery",
                table: "training_sessions",
                columns: new[] { "OrganizationId", "PlannedStartAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_training_sessions_OrganizationId_SourceBookingId",
                schema: "training_delivery",
                table: "training_sessions",
                columns: new[] { "OrganizationId", "SourceBookingId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_training_sessions_OrganizationId_Status_ActualStartAtUtc",
                schema: "training_delivery",
                table: "training_sessions",
                columns: new[] { "OrganizationId", "Status", "ActualStartAtUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "session_attendance",
                schema: "training_delivery");

            migrationBuilder.DropTable(
                name: "session_cancellations",
                schema: "training_delivery");

            migrationBuilder.DropTable(
                name: "session_competency_assessments",
                schema: "training_delivery");

            migrationBuilder.DropTable(
                name: "session_interruptions",
                schema: "training_delivery");

            migrationBuilder.DropTable(
                name: "session_interventions",
                schema: "training_delivery");

            migrationBuilder.DropTable(
                name: "session_observations",
                schema: "training_delivery");

            migrationBuilder.DropTable(
                name: "session_odometer_readings",
                schema: "training_delivery");

            migrationBuilder.DropTable(
                name: "session_reports",
                schema: "training_delivery");

            migrationBuilder.DropTable(
                name: "training_incident_evidence",
                schema: "training_delivery");

            migrationBuilder.DropTable(
                name: "training_incident_history",
                schema: "training_delivery");

            migrationBuilder.DropTable(
                name: "training_incident_participants",
                schema: "training_delivery");

            migrationBuilder.DropTable(
                name: "training_session_cancellation_consequences",
                schema: "training_delivery");

            migrationBuilder.DropTable(
                name: "training_session_completion_consequences",
                schema: "training_delivery");

            migrationBuilder.DropTable(
                name: "training_sessions",
                schema: "training_delivery");

            migrationBuilder.DropTable(
                name: "training_incidents",
                schema: "training_delivery");
        }
    }
}
