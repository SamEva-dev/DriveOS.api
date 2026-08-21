using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DriveOS.Modules.TrainingDelivery.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class updateTrainingDelivery : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                schema: "training_delivery",
                table: "session_interventions");

            migrationBuilder.DropColumn(
                name: "RelatedObjective",
                schema: "training_delivery",
                table: "session_interventions");

            migrationBuilder.AddColumn<decimal>(
                name: "EndEnergyLevelPercent",
                schema: "training_delivery",
                table: "training_sessions",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "StartOperationId",
                schema: "training_delivery",
                table: "training_sessions",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StartRequestFingerprint",
                schema: "training_delivery",
                table: "training_sessions",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CorrectedDeliveredDurationMinutes",
                schema: "training_delivery",
                table: "session_reports",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InternalNote",
                schema: "training_delivery",
                table: "session_reports",
                type: "character varying(5000)",
                maxLength: 5000,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LastCompletedStep",
                schema: "training_delivery",
                table: "session_reports",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LastSavedAtUtc",
                schema: "training_delivery",
                table: "session_reports",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<Guid>(
                name: "LastSavedByUserId",
                schema: "training_delivery",
                table: "session_reports",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "SharedComment",
                schema: "training_delivery",
                table: "session_reports",
                type: "character varying(5000)",
                maxLength: 5000,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                schema: "training_delivery",
                table: "session_reports",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Version",
                schema: "training_delivery",
                table: "session_reports",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Context",
                schema: "training_delivery",
                table: "session_interventions",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "InternalComment",
                schema: "training_delivery",
                table: "session_interventions",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Outcome",
                schema: "training_delivery",
                table: "session_interventions",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Reason",
                schema: "training_delivery",
                table: "session_interventions",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "RelatedCompetencyId",
                schema: "training_delivery",
                table: "session_interventions",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SharedExplanation",
                schema: "training_delivery",
                table: "session_interventions",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "group_training_sessions",
                schema: "training_delivery",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceBookingId = table.Column<Guid>(type: "uuid", nullable: false),
                    Program = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Capacity = table.Column<int>(type: "integer", nullable: false),
                    TrainerId = table.Column<Guid>(type: "uuid", nullable: false),
                    BranchId = table.Column<Guid>(type: "uuid", nullable: true),
                    RoomResourceId = table.Column<Guid>(type: "uuid", nullable: true),
                    RoomName = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    PlannedStartAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    PlannedEndAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    SharedObjectives = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CollectiveReport = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_group_training_sessions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "session_energy_entries",
                schema: "training_delivery",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TrainingSessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    OperationId = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestFingerprint = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    EnergyLevelPercent = table.Column<decimal>(type: "numeric(5,1)", precision: 5, scale: 1, nullable: true),
                    Quantity = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: true),
                    ObservedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Note = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedOffline = table.Column<bool>(type: "boolean", nullable: false),
                    RecordedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    RecordedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_session_energy_entries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_session_energy_entries_training_sessions_TrainingSessionId",
                        column: x => x.TrainingSessionId,
                        principalSchema: "training_delivery",
                        principalTable: "training_sessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "session_markers",
                schema: "training_delivery",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TrainingSessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    OperationId = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestFingerprint = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    OccurredAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CompetencyId = table.Column<Guid>(type: "uuid", nullable: true),
                    ShortNote = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Severity = table.Column<int>(type: "integer", nullable: false),
                    Latitude = table.Column<decimal>(type: "numeric(9,6)", precision: 9, scale: 6, nullable: true),
                    Longitude = table.Column<decimal>(type: "numeric(9,6)", precision: 9, scale: 6, nullable: true),
                    CreatedOffline = table.Column<bool>(type: "boolean", nullable: false),
                    RecordedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    RecordedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_session_markers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_session_markers_training_sessions_TrainingSessionId",
                        column: x => x.TrainingSessionId,
                        principalSchema: "training_delivery",
                        principalTable: "training_sessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "session_report_narrative_revisions",
                schema: "training_delivery",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SessionReportId = table.Column<Guid>(type: "uuid", nullable: false),
                    Kind = table.Column<int>(type: "integer", nullable: false),
                    ReportVersion = table.Column<int>(type: "integer", nullable: false),
                    Content = table.Column<string>(type: "character varying(5000)", maxLength: 5000, nullable: true),
                    ChangedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ChangedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_session_report_narrative_revisions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_session_report_narrative_revisions_session_reports_SessionR~",
                        column: x => x.SessionReportId,
                        principalSchema: "training_delivery",
                        principalTable: "session_reports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "session_report_revisions",
                schema: "training_delivery",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SessionReportId = table.Column<Guid>(type: "uuid", nullable: false),
                    OperationId = table.Column<Guid>(type: "uuid", nullable: false),
                    Scenario = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    FieldCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CurrentValue = table.Column<string>(type: "character varying(5000)", maxLength: 5000, nullable: false),
                    ProposedValue = table.Column<string>(type: "character varying(5000)", maxLength: 5000, nullable: false),
                    Reason = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    HasFinancialImpact = table.Column<bool>(type: "boolean", nullable: false),
                    RequestedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    DecidedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    DecidedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DecisionReason = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    AppliedReportVersion = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_session_report_revisions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_session_report_revisions_session_reports_SessionReportId",
                        column: x => x.SessionReportId,
                        principalSchema: "training_delivery",
                        principalTable: "session_reports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "group_training_session_operations",
                schema: "training_delivery",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    GroupTrainingSessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    OperationId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_group_training_session_operations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_group_training_session_operations_group_training_sessions_G~",
                        column: x => x.GroupTrainingSessionId,
                        principalSchema: "training_delivery",
                        principalTable: "group_training_sessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "group_training_session_participants",
                schema: "training_delivery",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    GroupTrainingSessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    StudentId = table.Column<Guid>(type: "uuid", nullable: false),
                    AddedOutsideOriginalList = table.Column<bool>(type: "boolean", nullable: false),
                    AttendanceStatus = table.Column<int>(type: "integer", nullable: false),
                    AttendanceMethod = table.Column<int>(type: "integer", nullable: true),
                    CheckInAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CheckOutAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    AttendanceRecordedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    CompetencyId = table.Column<Guid>(type: "uuid", nullable: true),
                    AssessmentLevel = table.Column<int>(type: "integer", nullable: true),
                    QuizScore = table.Column<decimal>(type: "numeric", nullable: true),
                    IndividualObservation = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    AssessmentRecordedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    CertificateStatus = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_group_training_session_participants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_group_training_session_participants_group_training_sessions~",
                        column: x => x.GroupTrainingSessionId,
                        principalSchema: "training_delivery",
                        principalTable: "group_training_sessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_group_training_session_operations_GroupTrainingSessionId_Op~",
                schema: "training_delivery",
                table: "group_training_session_operations",
                columns: new[] { "GroupTrainingSessionId", "OperationId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_group_training_session_participants_GroupTrainingSessionId_~",
                schema: "training_delivery",
                table: "group_training_session_participants",
                columns: new[] { "GroupTrainingSessionId", "StudentId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_group_training_sessions_OrganizationId_PlannedStartAtUtc",
                schema: "training_delivery",
                table: "group_training_sessions",
                columns: new[] { "OrganizationId", "PlannedStartAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_group_training_sessions_OrganizationId_SourceBookingId",
                schema: "training_delivery",
                table: "group_training_sessions",
                columns: new[] { "OrganizationId", "SourceBookingId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_session_energy_entries_TrainingSessionId_ObservedAtUtc",
                schema: "training_delivery",
                table: "session_energy_entries",
                columns: new[] { "TrainingSessionId", "ObservedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_session_energy_entries_TrainingSessionId_OperationId",
                schema: "training_delivery",
                table: "session_energy_entries",
                columns: new[] { "TrainingSessionId", "OperationId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_session_markers_TrainingSessionId_OccurredAtUtc",
                schema: "training_delivery",
                table: "session_markers",
                columns: new[] { "TrainingSessionId", "OccurredAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_session_markers_TrainingSessionId_OperationId",
                schema: "training_delivery",
                table: "session_markers",
                columns: new[] { "TrainingSessionId", "OperationId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_session_report_narrative_revisions_SessionReportId_Kind_Rep~",
                schema: "training_delivery",
                table: "session_report_narrative_revisions",
                columns: new[] { "SessionReportId", "Kind", "ReportVersion" });

            migrationBuilder.CreateIndex(
                name: "IX_session_report_revisions_OperationId",
                schema: "training_delivery",
                table: "session_report_revisions",
                column: "OperationId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_session_report_revisions_SessionReportId_Status",
                schema: "training_delivery",
                table: "session_report_revisions",
                columns: new[] { "SessionReportId", "Status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "group_training_session_operations",
                schema: "training_delivery");

            migrationBuilder.DropTable(
                name: "group_training_session_participants",
                schema: "training_delivery");

            migrationBuilder.DropTable(
                name: "session_energy_entries",
                schema: "training_delivery");

            migrationBuilder.DropTable(
                name: "session_markers",
                schema: "training_delivery");

            migrationBuilder.DropTable(
                name: "session_report_narrative_revisions",
                schema: "training_delivery");

            migrationBuilder.DropTable(
                name: "session_report_revisions",
                schema: "training_delivery");

            migrationBuilder.DropTable(
                name: "group_training_sessions",
                schema: "training_delivery");

            migrationBuilder.DropColumn(
                name: "EndEnergyLevelPercent",
                schema: "training_delivery",
                table: "training_sessions");

            migrationBuilder.DropColumn(
                name: "StartOperationId",
                schema: "training_delivery",
                table: "training_sessions");

            migrationBuilder.DropColumn(
                name: "StartRequestFingerprint",
                schema: "training_delivery",
                table: "training_sessions");

            migrationBuilder.DropColumn(
                name: "CorrectedDeliveredDurationMinutes",
                schema: "training_delivery",
                table: "session_reports");

            migrationBuilder.DropColumn(
                name: "InternalNote",
                schema: "training_delivery",
                table: "session_reports");

            migrationBuilder.DropColumn(
                name: "LastCompletedStep",
                schema: "training_delivery",
                table: "session_reports");

            migrationBuilder.DropColumn(
                name: "LastSavedAtUtc",
                schema: "training_delivery",
                table: "session_reports");

            migrationBuilder.DropColumn(
                name: "LastSavedByUserId",
                schema: "training_delivery",
                table: "session_reports");

            migrationBuilder.DropColumn(
                name: "SharedComment",
                schema: "training_delivery",
                table: "session_reports");

            migrationBuilder.DropColumn(
                name: "Status",
                schema: "training_delivery",
                table: "session_reports");

            migrationBuilder.DropColumn(
                name: "Version",
                schema: "training_delivery",
                table: "session_reports");

            migrationBuilder.DropColumn(
                name: "Context",
                schema: "training_delivery",
                table: "session_interventions");

            migrationBuilder.DropColumn(
                name: "InternalComment",
                schema: "training_delivery",
                table: "session_interventions");

            migrationBuilder.DropColumn(
                name: "Outcome",
                schema: "training_delivery",
                table: "session_interventions");

            migrationBuilder.DropColumn(
                name: "Reason",
                schema: "training_delivery",
                table: "session_interventions");

            migrationBuilder.DropColumn(
                name: "RelatedCompetencyId",
                schema: "training_delivery",
                table: "session_interventions");

            migrationBuilder.DropColumn(
                name: "SharedExplanation",
                schema: "training_delivery",
                table: "session_interventions");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                schema: "training_delivery",
                table: "session_interventions",
                type: "character varying(3000)",
                maxLength: 3000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RelatedObjective",
                schema: "training_delivery",
                table: "session_interventions",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);
        }
    }
}
