using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DriveOS.Modules.ExamsCertification.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddExamRegistrationFiles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "exam_registration_files",
                schema: "exams_certification",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: false),
                    registration_id = table.Column<Guid>(type: "uuid", nullable: false),
                    student_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    current_version = table.Column<int>(type: "integer", nullable: false),
                    last_evaluated_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    last_modified_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    last_modified_by_user_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_exam_registration_files", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "exam_registration_file_revisions",
                schema: "exams_certification",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    registration_file_id = table.Column<Guid>(type: "uuid", nullable: false),
                    version = table.Column<int>(type: "integer", nullable: false),
                    candidate_reference = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    official_data_json = table.Column<string>(type: "jsonb", nullable: true),
                    created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by_user_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_exam_registration_file_revisions", x => x.id);
                    table.ForeignKey(
                        name: "FK_exam_registration_file_revisions_exam_registration_files_re~",
                        column: x => x.registration_file_id,
                        principalSchema: "exams_certification",
                        principalTable: "exam_registration_files",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "exam_registration_file_checklist_items",
                schema: "exams_certification",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    revision_id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    required = table.Column<bool>(type: "boolean", nullable: false),
                    status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    message_key = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    source = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    evidence = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_exam_registration_file_checklist_items", x => x.id);
                    table.ForeignKey(
                        name: "FK_exam_registration_file_checklist_items_exam_registration_fi~",
                        column: x => x.revision_id,
                        principalSchema: "exams_certification",
                        principalTable: "exam_registration_file_revisions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_exam_registration_file_checklist_items_revision_id_code",
                schema: "exams_certification",
                table: "exam_registration_file_checklist_items",
                columns: new[] { "revision_id", "code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_exam_registration_file_revisions_registration_file_id_versi~",
                schema: "exams_certification",
                table: "exam_registration_file_revisions",
                columns: new[] { "registration_file_id", "version" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_exam_registration_files_organization_id_registration_id",
                schema: "exams_certification",
                table: "exam_registration_files",
                columns: new[] { "organization_id", "registration_id" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "exam_registration_file_checklist_items",
                schema: "exams_certification");

            migrationBuilder.DropTable(
                name: "exam_registration_file_revisions",
                schema: "exams_certification");

            migrationBuilder.DropTable(
                name: "exam_registration_files",
                schema: "exams_certification");
        }
    }
}
