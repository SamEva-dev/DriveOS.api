using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DriveOS.Modules.ExamsCertification.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddExamRegistrationSubmissions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ux_exam_registration_active_student_exam",
                schema: "exams_certification",
                table: "exam_registrations");

            migrationBuilder.CreateTable(
                name: "exam_registration_submissions",
                schema: "exams_certification",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: false),
                    registration_id = table.Column<Guid>(type: "uuid", nullable: false),
                    registration_file_id = table.Column<Guid>(type: "uuid", nullable: false),
                    file_revision_id = table.Column<Guid>(type: "uuid", nullable: false),
                    file_version = table.Column<int>(type: "integer", nullable: false),
                    submission_version = table.Column<int>(type: "integer", nullable: false),
                    provider_code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    payload_json = table.Column<string>(type: "jsonb", nullable: false),
                    status = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    operation_id = table.Column<Guid>(type: "uuid", nullable: false),
                    request_fingerprint = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    external_submission_id = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    external_registration_id = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    candidate_reference = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    provider_response_code = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    provider_response_json = table.Column<string>(type: "text", nullable: true),
                    error_code = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    error_message_key = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    submitted_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    responded_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    last_modified_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    last_modified_by_user_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_exam_registration_submissions", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ux_exam_registration_active_student_exam",
                schema: "exams_certification",
                table: "exam_registrations",
                columns: new[] { "organization_id", "student_id", "exam_type", "license_category" },
                unique: true,
                filter: "status IN ('Draft', 'PlaceAssigned', 'PendingSubmission', 'Submitted', 'Confirmed', 'CorrectionRequested')");

            migrationBuilder.CreateIndex(
                name: "ix_exam_registration_submission_external_registration",
                schema: "exams_certification",
                table: "exam_registration_submissions",
                columns: new[] { "organization_id", "external_registration_id" });

            migrationBuilder.CreateIndex(
                name: "ux_exam_registration_submission_file_revision",
                schema: "exams_certification",
                table: "exam_registration_submissions",
                columns: new[] { "organization_id", "registration_id", "file_revision_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ux_exam_registration_submission_operation",
                schema: "exams_certification",
                table: "exam_registration_submissions",
                columns: new[] { "organization_id", "operation_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ux_exam_registration_submission_version",
                schema: "exams_certification",
                table: "exam_registration_submissions",
                columns: new[] { "organization_id", "registration_id", "submission_version" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "exam_registration_submissions",
                schema: "exams_certification");

            migrationBuilder.DropIndex(
                name: "ux_exam_registration_active_student_exam",
                schema: "exams_certification",
                table: "exam_registrations");

            migrationBuilder.CreateIndex(
                name: "ux_exam_registration_active_student_exam",
                schema: "exams_certification",
                table: "exam_registrations",
                columns: new[] { "organization_id", "student_id", "exam_type", "license_category" },
                unique: true,
                filter: "status IN ('Draft', 'PlaceAssigned', 'PendingSubmission', 'Submitted', 'Confirmed')");
        }
    }
}
