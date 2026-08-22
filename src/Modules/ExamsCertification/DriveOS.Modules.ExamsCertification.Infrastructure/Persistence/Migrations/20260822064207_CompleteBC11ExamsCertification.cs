using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DriveOS.Modules.ExamsCertification.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class CompleteBC11ExamsCertification : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "exam_attempts",
                schema: "exams_certification",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    RegistrationId = table.Column<Guid>(type: "uuid", nullable: false),
                    PreparationId = table.Column<Guid>(type: "uuid", nullable: false),
                    StudentId = table.Column<Guid>(type: "uuid", nullable: false),
                    AttemptNumber = table.Column<int>(type: "integer", nullable: false),
                    PreparationRevision = table.Column<int>(type: "integer", nullable: false),
                    ConvocationVersion = table.Column<int>(type: "integer", nullable: false),
                    ExamType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    LicenseCategory = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    ExamCenterId = table.Column<Guid>(type: "uuid", nullable: false),
                    ExamPlaceId = table.Column<Guid>(type: "uuid", nullable: false),
                    ScheduledStartUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ScheduledEndUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    MeetingAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    InstructorId = table.Column<Guid>(type: "uuid", nullable: true),
                    VehicleId = table.Column<Guid>(type: "uuid", nullable: true),
                    SchedulingBookingId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    AttendanceStatus = table.Column<int>(type: "integer", nullable: false),
                    CheckedInAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DepartedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ArrivedAtCenterAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    StartedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CompletedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ReturnedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    OperationalReasonCode = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    OperationalNotes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModifiedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastModifiedByUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_exam_attempts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "exam_attestations",
                schema: "exams_certification",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: false),
                    exam_result_id = table.Column<Guid>(type: "uuid", nullable: false),
                    result_revision = table.Column<int>(type: "integer", nullable: false),
                    exam_attempt_id = table.Column<Guid>(type: "uuid", nullable: false),
                    exam_registration_id = table.Column<Guid>(type: "uuid", nullable: false),
                    student_id = table.Column<Guid>(type: "uuid", nullable: false),
                    attempt_number = table.Column<int>(type: "integer", nullable: false),
                    type = table.Column<string>(type: "character varying(48)", maxLength: 48, nullable: false),
                    reference = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    current_version = table.Column<int>(type: "integer", nullable: false),
                    supersedes_attestation_id = table.Column<Guid>(type: "uuid", nullable: true),
                    status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    issued_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    issued_by_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    expires_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    delivered_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    delivered_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    delivery_channel = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    revoked_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    revoked_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    revocation_reason_code = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    revocation_notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    superseded_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    last_modified_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    last_modified_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    operation_id = table.Column<Guid>(type: "uuid", nullable: false),
                    request_fingerprint = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_exam_attestations", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "exam_convocations",
                schema: "exams_certification",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: false),
                    registration_id = table.Column<Guid>(type: "uuid", nullable: false),
                    student_id = table.Column<Guid>(type: "uuid", nullable: false),
                    current_version = table.Column<int>(type: "integer", nullable: false),
                    delivery_status = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    delivery_channel = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    delivered_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    delivered_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    acknowledged_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    internal_meeting_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    internal_meeting_instructions = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    last_modified_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    last_modified_by_user_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_exam_convocations", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "exam_failure_analyses",
                schema: "exams_certification",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: false),
                    exam_result_id = table.Column<Guid>(type: "uuid", nullable: false),
                    result_revision = table.Column<int>(type: "integer", nullable: false),
                    attempt_id = table.Column<Guid>(type: "uuid", nullable: false),
                    registration_id = table.Column<Guid>(type: "uuid", nullable: false),
                    student_id = table.Column<Guid>(type: "uuid", nullable: false),
                    attempt_number = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    instructor_analysis = table.Column<string>(type: "character varying(8000)", maxLength: 8000, nullable: true),
                    student_feedback = table.Column<string>(type: "character varying(8000)", maxLength: 8000, nullable: true),
                    summary = table.Column<string>(type: "character varying(8000)", maxLength: 8000, nullable: true),
                    recommendation = table.Column<string>(type: "character varying(8000)", maxLength: 8000, nullable: true),
                    training_path_id = table.Column<Guid>(type: "uuid", nullable: true),
                    official_failure_reasons_snapshot = table.Column<string>(type: "character varying(8000)", maxLength: 8000, nullable: false),
                    affected_competency_ids = table.Column<string>(type: "character varying(8000)", maxLength: 8000, nullable: false),
                    factual_evidence = table.Column<string>(type: "character varying(8000)", maxLength: 8000, nullable: true),
                    probable_cause_codes = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    hypotheses = table.Column<string>(type: "character varying(8000)", maxLength: 8000, nullable: true),
                    recommendation_codes = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    recommended_hours = table.Column<int>(type: "integer", nullable: true),
                    submitted_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    submitted_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    approved_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    approved_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    completed_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    completed_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    superseded_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    last_modified_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    last_modified_by_user_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_exam_failure_analyses", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "exam_operational_plans",
                schema: "exams_certification",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: false),
                    registration_id = table.Column<Guid>(type: "uuid", nullable: false),
                    student_id = table.Column<Guid>(type: "uuid", nullable: false),
                    convocation_version = table.Column<int>(type: "integer", nullable: false),
                    official_start_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    official_end_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    meeting_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    operational_window_start_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    operational_window_end_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    travel_buffer_before_minutes = table.Column<int>(type: "integer", nullable: false),
                    travel_buffer_after_minutes = table.Column<int>(type: "integer", nullable: false),
                    departure_branch_id = table.Column<Guid>(type: "uuid", nullable: true),
                    instructor_required = table.Column<bool>(type: "boolean", nullable: false),
                    vehicle_required = table.Column<bool>(type: "boolean", nullable: false),
                    meeting_instructions = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    has_scheduling_conflicts = table.Column<bool>(type: "boolean", nullable: false),
                    instructor_candidates_available = table.Column<int>(type: "integer", nullable: false),
                    vehicle_candidates_available = table.Column<int>(type: "integer", nullable: false),
                    conflict_summary = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    last_assessed_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    status = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    last_modified_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    last_modified_by_user_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_exam_operational_plans", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "exam_preparations",
                schema: "exams_certification",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    RegistrationId = table.Column<Guid>(type: "uuid", nullable: false),
                    StudentId = table.Column<Guid>(type: "uuid", nullable: false),
                    Revision = table.Column<int>(type: "integer", nullable: false),
                    ConvocationVersion = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    MeetingPointConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    VehicleEnergyConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    InstructorConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    InstructionsTransmitted = table.Column<bool>(type: "boolean", nullable: false),
                    LastOperationId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastRequestFingerprint = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    LastEvaluatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ConfirmedRevision = table.Column<int>(type: "integer", nullable: true),
                    ConfirmedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ConfirmedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModifiedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastModifiedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    reminder_offsets_days = table.Column<List<int>>(type: "integer[]", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_exam_preparations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "exam_remediation_requests",
                schema: "exams_certification",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: false),
                    failure_analysis_id = table.Column<Guid>(type: "uuid", nullable: false),
                    exam_result_id = table.Column<Guid>(type: "uuid", nullable: false),
                    result_revision = table.Column<int>(type: "integer", nullable: false),
                    failed_attempt_id = table.Column<Guid>(type: "uuid", nullable: false),
                    registration_id = table.Column<Guid>(type: "uuid", nullable: false),
                    student_id = table.Column<Guid>(type: "uuid", nullable: false),
                    failed_attempt_number = table.Column<int>(type: "integer", nullable: false),
                    training_path_id = table.Column<Guid>(type: "uuid", nullable: true),
                    analysis_summary = table.Column<string>(type: "character varying(8000)", maxLength: 8000, nullable: false),
                    recommendation_summary = table.Column<string>(type: "character varying(8000)", maxLength: 8000, nullable: true),
                    affected_competency_ids = table.Column<string>(type: "character varying(12000)", maxLength: 12000, nullable: false),
                    recommendation_codes = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    recommended_hours = table.Column<int>(type: "integer", nullable: true),
                    responsible_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    review_date = table.Column<DateOnly>(type: "date", nullable: true),
                    target_date = table.Column<DateOnly>(type: "date", nullable: true),
                    mock_exam_required = table.Column<bool>(type: "boolean", nullable: false),
                    funding_review_required = table.Column<bool>(type: "boolean", nullable: false),
                    pedagogical_remediation_plan_id = table.Column<Guid>(type: "uuid", nullable: true),
                    status = table.Column<string>(type: "character varying(48)", maxLength: 48, nullable: false),
                    deferred_reason_code = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    failure_code = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    cancellation_reason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    provisioned_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    completed_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    validated_for_re_presentation_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    validated_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    superseded_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    last_modified_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    last_modified_by_user_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_exam_remediation_requests", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "exam_resource_assignments",
                schema: "exams_certification",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    RegistrationId = table.Column<Guid>(type: "uuid", nullable: false),
                    StudentId = table.Column<Guid>(type: "uuid", nullable: false),
                    OperationalPlanId = table.Column<Guid>(type: "uuid", nullable: false),
                    ConvocationVersion = table.Column<int>(type: "integer", nullable: false),
                    OperationId = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestFingerprint = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    InstructorCalendarResourceId = table.Column<Guid>(type: "uuid", nullable: true),
                    InstructorId = table.Column<Guid>(type: "uuid", nullable: true),
                    InstructorQualificationVerified = table.Column<bool>(type: "boolean", nullable: false),
                    InstructorAvailabilityVerified = table.Column<bool>(type: "boolean", nullable: false),
                    VehicleCalendarResourceId = table.Column<Guid>(type: "uuid", nullable: true),
                    VehicleId = table.Column<Guid>(type: "uuid", nullable: true),
                    VehicleTechnicalCompatibilityVerified = table.Column<bool>(type: "boolean", nullable: false),
                    VehicleInsuranceVerified = table.Column<bool>(type: "boolean", nullable: false),
                    VehicleMaintenanceVerified = table.Column<bool>(type: "boolean", nullable: false),
                    VehicleLocationVerified = table.Column<bool>(type: "boolean", nullable: false),
                    VehicleOwnershipVerified = table.Column<bool>(type: "boolean", nullable: false),
                    SchedulingBookingId = table.Column<Guid>(type: "uuid", nullable: true),
                    SchedulingErrorCode = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModifiedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastModifiedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    instructor_warnings = table.Column<List<string>>(type: "text[]", nullable: false),
                    vehicle_external_reviews = table.Column<List<string>>(type: "text[]", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_exam_resource_assignments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "exam_results",
                schema: "exams_certification",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    AttemptId = table.Column<Guid>(type: "uuid", nullable: false),
                    RegistrationId = table.Column<Guid>(type: "uuid", nullable: false),
                    StudentId = table.Column<Guid>(type: "uuid", nullable: false),
                    AttemptNumber = table.Column<int>(type: "integer", nullable: false),
                    CurrentRevision = table.Column<int>(type: "integer", nullable: false),
                    Outcome = table.Column<int>(type: "integer", nullable: false),
                    Score = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: true),
                    FailureReasonCode = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: true),
                    Comments = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    SourceKind = table.Column<int>(type: "integer", nullable: false),
                    ProviderCode = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    ExternalResultId = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    EvidenceDocumentId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReceivedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    VerifiedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    VerifiedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    VerificationReference = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    FinalizedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    FinalizedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModifiedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastModifiedByUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_exam_results", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "exam_success_consequences",
                schema: "exams_certification",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: false),
                    result_id = table.Column<Guid>(type: "uuid", nullable: false),
                    result_revision = table.Column<int>(type: "integer", nullable: false),
                    kind = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    payload_json = table.Column<string>(type: "jsonb", nullable: false),
                    attempt_count = table.Column<int>(type: "integer", nullable: false),
                    created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    last_attempt_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    next_attempt_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    processed_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    superseded_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    last_error_code = table.Column<string>(type: "character varying(180)", maxLength: 180, nullable: true),
                    last_error_detail = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_exam_success_consequences", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "exam_success_processes",
                schema: "exams_certification",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: false),
                    exam_result_id = table.Column<Guid>(type: "uuid", nullable: false),
                    result_revision = table.Column<int>(type: "integer", nullable: false),
                    attempt_id = table.Column<Guid>(type: "uuid", nullable: false),
                    registration_id = table.Column<Guid>(type: "uuid", nullable: false),
                    student_id = table.Column<Guid>(type: "uuid", nullable: false),
                    attempt_number = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    completed_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    completed_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    superseded_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    archived_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    archived_by_user_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_exam_success_processes", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "exam_attempt_timeline",
                schema: "exams_certification",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AttemptId = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    OperationId = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestFingerprint = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Note = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    OccurredAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ActorUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Latitude = table.Column<decimal>(type: "numeric(9,6)", precision: 9, scale: 6, nullable: true),
                    Longitude = table.Column<decimal>(type: "numeric(9,6)", precision: 9, scale: 6, nullable: true),
                    AccuracyMeters = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: true),
                    LocationPurpose = table.Column<int>(type: "integer", nullable: true),
                    InstructorId = table.Column<Guid>(type: "uuid", nullable: true),
                    VehicleId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_exam_attempt_timeline", x => x.Id);
                    table.ForeignKey(
                        name: "FK_exam_attempt_timeline_exam_attempts_AttemptId",
                        column: x => x.AttemptId,
                        principalSchema: "exams_certification",
                        principalTable: "exam_attempts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "exam_attestation_revisions",
                schema: "exams_certification",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    attestation_id = table.Column<Guid>(type: "uuid", nullable: false),
                    version = table.Column<int>(type: "integer", nullable: false),
                    template_code = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    template_version = table.Column<int>(type: "integer", nullable: false),
                    document_id = table.Column<Guid>(type: "uuid", nullable: false),
                    document_sha256 = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    public_verification_token_hash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    signature_process_reference = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    signature_evidence_hash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    signed_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    signed_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    generated_by_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    generated_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_exam_attestation_revisions", x => x.id);
                    table.ForeignKey(
                        name: "FK_exam_attestation_revisions_exam_attestations_attestation_id",
                        column: x => x.attestation_id,
                        principalSchema: "exams_certification",
                        principalTable: "exam_attestations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "exam_convocation_revisions",
                schema: "exams_certification",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    convocation_id = table.Column<Guid>(type: "uuid", nullable: false),
                    version = table.Column<int>(type: "integer", nullable: false),
                    exam_center_id = table.Column<Guid>(type: "uuid", nullable: false),
                    center_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    center_address = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    time_zone_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    scheduled_start_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    scheduled_end_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    provider_code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    official_reference = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    candidate_reference = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    instructions = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    required_documents = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    provider_payload_reference = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    operation_id = table.Column<Guid>(type: "uuid", nullable: false),
                    request_fingerprint = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    received_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_exam_convocation_revisions", x => x.id);
                    table.ForeignKey(
                        name: "FK_exam_convocation_revisions_exam_convocations_convocation_id",
                        column: x => x.convocation_id,
                        principalSchema: "exams_certification",
                        principalTable: "exam_convocations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "exam_failure_findings",
                schema: "exams_certification",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    kind = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    code = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    detail = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    critical = table.Column<bool>(type: "boolean", nullable: false),
                    source = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    actor_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    failure_analysis_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_exam_failure_findings", x => x.id);
                    table.ForeignKey(
                        name: "FK_exam_failure_findings_exam_failure_analyses_failure_analysi~",
                        column: x => x.failure_analysis_id,
                        principalSchema: "exams_certification",
                        principalTable: "exam_failure_analyses",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "exam_preparation_checks",
                schema: "exams_certification",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PreparationId = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Required = table.Column<bool>(type: "boolean", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    MessageKey = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    Source = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Evidence = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    IsConfirmed = table.Column<bool>(type: "boolean", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_exam_preparation_checks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_exam_preparation_checks_exam_preparations_PreparationId",
                        column: x => x.PreparationId,
                        principalSchema: "exams_certification",
                        principalTable: "exam_preparations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "exam_result_revisions",
                schema: "exams_certification",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ResultId = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    RevisionNumber = table.Column<int>(type: "integer", nullable: false),
                    Outcome = table.Column<int>(type: "integer", nullable: false),
                    Score = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: true),
                    FailureReasonCode = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: true),
                    Comments = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    SourceKind = table.Column<int>(type: "integer", nullable: false),
                    ProviderCode = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    ExternalResultId = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    EvidenceDocumentId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReceivedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CorrectionReason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    OperationId = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestFingerprint = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ActorUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_exam_result_revisions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_exam_result_revisions_exam_results_ResultId",
                        column: x => x.ResultId,
                        principalSchema: "exams_certification",
                        principalTable: "exam_results",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "exam_success_actions",
                schema: "exams_certification",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    blocking = table.Column<bool>(type: "boolean", nullable: false),
                    status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    evidence_reference = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    reason_code = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    detail = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    updated_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    updated_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    success_process_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_exam_success_actions", x => x.id);
                    table.ForeignKey(
                        name: "FK_exam_success_actions_exam_success_processes_success_process~",
                        column: x => x.success_process_id,
                        principalSchema: "exams_certification",
                        principalTable: "exam_success_processes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_exam_attempt_timeline_AttemptId_OperationId",
                schema: "exams_certification",
                table: "exam_attempt_timeline",
                columns: new[] { "AttemptId", "OperationId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_exam_attempt_timeline_OrganizationId_OccurredAtUtc",
                schema: "exams_certification",
                table: "exam_attempt_timeline",
                columns: new[] { "OrganizationId", "OccurredAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_exam_attempts_OrganizationId_RegistrationId",
                schema: "exams_certification",
                table: "exam_attempts",
                columns: new[] { "OrganizationId", "RegistrationId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_exam_attempts_OrganizationId_Status_ScheduledStartUtc",
                schema: "exams_certification",
                table: "exam_attempts",
                columns: new[] { "OrganizationId", "Status", "ScheduledStartUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_exam_attempts_OrganizationId_StudentId_ExamType_LicenseCate~",
                schema: "exams_certification",
                table: "exam_attempts",
                columns: new[] { "OrganizationId", "StudentId", "ExamType", "LicenseCategory", "AttemptNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_exam_attestation_revisions_attestation_id_version",
                schema: "exams_certification",
                table: "exam_attestation_revisions",
                columns: new[] { "attestation_id", "version" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_exam_attestation_revisions_public_verification_token_hash",
                schema: "exams_certification",
                table: "exam_attestation_revisions",
                column: "public_verification_token_hash",
                unique: true,
                filter: "public_verification_token_hash IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_exam_attestations_result_type",
                schema: "exams_certification",
                table: "exam_attestations",
                columns: new[] { "organization_id", "exam_result_id", "type" });

            migrationBuilder.CreateIndex(
                name: "ix_exam_attestations_student_issued",
                schema: "exams_certification",
                table: "exam_attestations",
                columns: new[] { "organization_id", "student_id", "issued_at_utc" });

            migrationBuilder.CreateIndex(
                name: "ux_exam_attestations_operation",
                schema: "exams_certification",
                table: "exam_attestations",
                columns: new[] { "organization_id", "operation_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ux_exam_convocation_revision_operation",
                schema: "exams_certification",
                table: "exam_convocation_revisions",
                columns: new[] { "convocation_id", "operation_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ux_exam_convocation_revision_version",
                schema: "exams_certification",
                table: "exam_convocation_revisions",
                columns: new[] { "convocation_id", "version" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_exam_convocation_student",
                schema: "exams_certification",
                table: "exam_convocations",
                columns: new[] { "organization_id", "student_id" });

            migrationBuilder.CreateIndex(
                name: "ux_exam_convocation_registration",
                schema: "exams_certification",
                table: "exam_convocations",
                columns: new[] { "organization_id", "registration_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ux_exam_failure_analysis_result_revision",
                schema: "exams_certification",
                table: "exam_failure_analyses",
                columns: new[] { "organization_id", "exam_result_id", "result_revision" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_exam_failure_findings_failure_analysis_id_kind_code",
                schema: "exams_certification",
                table: "exam_failure_findings",
                columns: new[] { "failure_analysis_id", "kind", "code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_exam_operational_plan_start",
                schema: "exams_certification",
                table: "exam_operational_plans",
                columns: new[] { "organization_id", "official_start_utc" });

            migrationBuilder.CreateIndex(
                name: "ux_exam_operational_plan_registration",
                schema: "exams_certification",
                table: "exam_operational_plans",
                columns: new[] { "organization_id", "registration_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_exam_preparation_checks_PreparationId_Code",
                schema: "exams_certification",
                table: "exam_preparation_checks",
                columns: new[] { "PreparationId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_exam_preparations_OrganizationId_RegistrationId",
                schema: "exams_certification",
                table: "exam_preparations",
                columns: new[] { "OrganizationId", "RegistrationId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_exam_remediation_requests_organization_id_exam_result_id_re~",
                schema: "exams_certification",
                table: "exam_remediation_requests",
                columns: new[] { "organization_id", "exam_result_id", "result_revision" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_exam_remediation_requests_organization_id_failure_analysis_~",
                schema: "exams_certification",
                table: "exam_remediation_requests",
                columns: new[] { "organization_id", "failure_analysis_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_exam_remediation_requests_organization_id_student_id_status",
                schema: "exams_certification",
                table: "exam_remediation_requests",
                columns: new[] { "organization_id", "student_id", "status" });

            migrationBuilder.CreateIndex(
                name: "IX_exam_remediation_requests_pedagogical_remediation_plan_id",
                schema: "exams_certification",
                table: "exam_remediation_requests",
                column: "pedagogical_remediation_plan_id");

            migrationBuilder.CreateIndex(
                name: "IX_exam_resource_assignments_OrganizationId_OperationId",
                schema: "exams_certification",
                table: "exam_resource_assignments",
                columns: new[] { "OrganizationId", "OperationId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_exam_resource_assignments_OrganizationId_RegistrationId",
                schema: "exams_certification",
                table: "exam_resource_assignments",
                columns: new[] { "OrganizationId", "RegistrationId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_exam_result_revisions_OrganizationId_OperationId",
                schema: "exams_certification",
                table: "exam_result_revisions",
                columns: new[] { "OrganizationId", "OperationId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_exam_result_revisions_ResultId_RevisionNumber",
                schema: "exams_certification",
                table: "exam_result_revisions",
                columns: new[] { "ResultId", "RevisionNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_exam_results_OrganizationId_AttemptId",
                schema: "exams_certification",
                table: "exam_results",
                columns: new[] { "OrganizationId", "AttemptId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_exam_results_OrganizationId_ExternalResultId",
                schema: "exams_certification",
                table: "exam_results",
                columns: new[] { "OrganizationId", "ExternalResultId" });

            migrationBuilder.CreateIndex(
                name: "IX_exam_results_OrganizationId_StudentId_Status",
                schema: "exams_certification",
                table: "exam_results",
                columns: new[] { "OrganizationId", "StudentId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_exam_success_actions_success_process_id_code",
                schema: "exams_certification",
                table: "exam_success_actions",
                columns: new[] { "success_process_id", "code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_exam_success_consequences_organization_id_result_id_result_~",
                schema: "exams_certification",
                table: "exam_success_consequences",
                columns: new[] { "organization_id", "result_id", "result_revision", "kind" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_exam_success_consequences_status_last_attempt_at_utc",
                schema: "exams_certification",
                table: "exam_success_consequences",
                columns: new[] { "status", "last_attempt_at_utc" });

            migrationBuilder.CreateIndex(
                name: "IX_exam_success_consequences_status_next_attempt_at_utc",
                schema: "exams_certification",
                table: "exam_success_consequences",
                columns: new[] { "status", "next_attempt_at_utc" });

            migrationBuilder.CreateIndex(
                name: "ux_exam_success_process_result_revision",
                schema: "exams_certification",
                table: "exam_success_processes",
                columns: new[] { "organization_id", "exam_result_id", "result_revision" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "exam_attempt_timeline",
                schema: "exams_certification");

            migrationBuilder.DropTable(
                name: "exam_attestation_revisions",
                schema: "exams_certification");

            migrationBuilder.DropTable(
                name: "exam_convocation_revisions",
                schema: "exams_certification");

            migrationBuilder.DropTable(
                name: "exam_failure_findings",
                schema: "exams_certification");

            migrationBuilder.DropTable(
                name: "exam_operational_plans",
                schema: "exams_certification");

            migrationBuilder.DropTable(
                name: "exam_preparation_checks",
                schema: "exams_certification");

            migrationBuilder.DropTable(
                name: "exam_remediation_requests",
                schema: "exams_certification");

            migrationBuilder.DropTable(
                name: "exam_resource_assignments",
                schema: "exams_certification");

            migrationBuilder.DropTable(
                name: "exam_result_revisions",
                schema: "exams_certification");

            migrationBuilder.DropTable(
                name: "exam_success_actions",
                schema: "exams_certification");

            migrationBuilder.DropTable(
                name: "exam_success_consequences",
                schema: "exams_certification");

            migrationBuilder.DropTable(
                name: "exam_attempts",
                schema: "exams_certification");

            migrationBuilder.DropTable(
                name: "exam_attestations",
                schema: "exams_certification");

            migrationBuilder.DropTable(
                name: "exam_convocations",
                schema: "exams_certification");

            migrationBuilder.DropTable(
                name: "exam_failure_analyses",
                schema: "exams_certification");

            migrationBuilder.DropTable(
                name: "exam_preparations",
                schema: "exams_certification");

            migrationBuilder.DropTable(
                name: "exam_results",
                schema: "exams_certification");

            migrationBuilder.DropTable(
                name: "exam_success_processes",
                schema: "exams_certification");
        }
    }
}
