using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DriveOS.Modules.ExamsCertification.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialExamsCertification : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "exams_certification");

            migrationBuilder.CreateTable(
                name: "exam_readiness_decisions",
                schema: "exams_certification",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: false),
                    student_id = table.Column<Guid>(type: "uuid", nullable: false),
                    training_path_id = table.Column<Guid>(type: "uuid", nullable: false),
                    version = table.Column<int>(type: "integer", nullable: false),
                    outcome = table.Column<string>(type: "character varying(48)", maxLength: 48, nullable: false),
                    pedagogical_check = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    administrative_check = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    financial_check = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    regulatory_check = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    rationale = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    conditions = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    reviewer_id = table.Column<Guid>(type: "uuid", nullable: false),
                    decided_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    is_current = table.Column<bool>(type: "boolean", nullable: false),
                    superseded_by_decision_id = table.Column<Guid>(type: "uuid", nullable: true),
                    superseded_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    last_modified_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    last_modified_by_user_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_exam_readiness_decisions", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ux_exam_readiness_current",
                schema: "exams_certification",
                table: "exam_readiness_decisions",
                columns: new[] { "organization_id", "student_id", "training_path_id" },
                unique: true,
                filter: "is_current = TRUE");

            migrationBuilder.CreateIndex(
                name: "ux_exam_readiness_decision_version",
                schema: "exams_certification",
                table: "exam_readiness_decisions",
                columns: new[] { "organization_id", "student_id", "training_path_id", "version" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "exam_readiness_decisions",
                schema: "exams_certification");
        }
    }
}
