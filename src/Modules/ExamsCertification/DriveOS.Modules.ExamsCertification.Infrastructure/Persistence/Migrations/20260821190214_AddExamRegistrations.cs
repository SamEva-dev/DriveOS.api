using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DriveOS.Modules.ExamsCertification.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddExamRegistrations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "exam_registrations",
                schema: "exams_certification",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: false),
                    student_id = table.Column<Guid>(type: "uuid", nullable: false),
                    training_path_id = table.Column<Guid>(type: "uuid", nullable: false),
                    readiness_decision_id = table.Column<Guid>(type: "uuid", nullable: false),
                    exam_place_id = table.Column<Guid>(type: "uuid", nullable: false),
                    exam_center_id = table.Column<Guid>(type: "uuid", nullable: false),
                    exam_type = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    license_category = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    scheduled_start_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    scheduled_end_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    provider_code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    external_place_id = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    external_registration_id = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    candidate_reference = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    status = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    operation_id = table.Column<Guid>(type: "uuid", nullable: false),
                    request_fingerprint = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    last_modified_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    last_modified_by_user_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_exam_registrations", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_exam_registration_student_calendar",
                schema: "exams_certification",
                table: "exam_registrations",
                columns: new[] { "organization_id", "student_id", "scheduled_start_utc" });

            migrationBuilder.CreateIndex(
                name: "ux_exam_registration_active_student_exam",
                schema: "exams_certification",
                table: "exam_registrations",
                columns: new[] { "organization_id", "student_id", "exam_type", "license_category" },
                unique: true,
                filter: "status IN ('Draft', 'PlaceAssigned', 'PendingSubmission', 'Submitted', 'Confirmed')");

            migrationBuilder.CreateIndex(
                name: "ux_exam_registration_operation",
                schema: "exams_certification",
                table: "exam_registrations",
                columns: new[] { "organization_id", "operation_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ux_exam_registration_place",
                schema: "exams_certification",
                table: "exam_registrations",
                columns: new[] { "organization_id", "exam_place_id" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "exam_registrations",
                schema: "exams_certification");
        }
    }
}
