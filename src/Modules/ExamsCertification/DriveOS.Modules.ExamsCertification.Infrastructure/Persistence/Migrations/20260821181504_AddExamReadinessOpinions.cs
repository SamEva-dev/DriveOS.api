using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DriveOS.Modules.ExamsCertification.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddExamReadinessOpinions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "exam_readiness_opinions",
                schema: "exams_certification",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: false),
                    student_id = table.Column<Guid>(type: "uuid", nullable: false),
                    training_path_id = table.Column<Guid>(type: "uuid", nullable: false),
                    previous_opinion_id = table.Column<Guid>(type: "uuid", nullable: true),
                    version = table.Column<int>(type: "integer", nullable: false),
                    opinion = table.Column<string>(type: "character varying(48)", maxLength: 48, nullable: false),
                    observed_autonomy = table.Column<string>(type: "character varying(48)", maxLength: 48, nullable: false),
                    reservation_codes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    reservations = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    conditions = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    comment = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    progress_percent = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    required_competencies = table.Column<int>(type: "integer", nullable: false),
                    evaluated_required_competencies = table.Column<int>(type: "integer", nullable: false),
                    has_completed_pedagogical_review = table.Column<bool>(type: "boolean", nullable: false),
                    latest_pedagogical_decision = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    operation_id = table.Column<Guid>(type: "uuid", nullable: false),
                    request_fingerprint = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    author_id = table.Column<Guid>(type: "uuid", nullable: false),
                    submitted_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    last_modified_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    last_modified_by_user_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_exam_readiness_opinions", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_exam_readiness_opinion_timeline",
                schema: "exams_certification",
                table: "exam_readiness_opinions",
                columns: new[] { "organization_id", "student_id", "training_path_id", "submitted_at_utc" });

            migrationBuilder.CreateIndex(
                name: "ux_exam_readiness_opinion_author_version",
                schema: "exams_certification",
                table: "exam_readiness_opinions",
                columns: new[] { "organization_id", "student_id", "training_path_id", "author_id", "version" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ux_exam_readiness_opinion_operation",
                schema: "exams_certification",
                table: "exam_readiness_opinions",
                columns: new[] { "organization_id", "operation_id" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "exam_readiness_opinions",
                schema: "exams_certification");
        }
    }
}
