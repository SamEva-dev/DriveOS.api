using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DriveOS.Modules.ExamsCertification.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddExamCentersAndPlaces : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "exam_centers",
                schema: "exams_certification",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    country_code = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    time_zone_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    administrative_area_code = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    address = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    external_provider_code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    external_center_id = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    last_modified_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    last_modified_by_user_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_exam_centers", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "exam_places",
                schema: "exams_certification",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: false),
                    exam_center_id = table.Column<Guid>(type: "uuid", nullable: false),
                    exam_type = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    license_category = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    starts_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ends_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    time_zone_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    source = table.Column<string>(type: "character varying(48)", maxLength: 48, nullable: false),
                    provider_code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    external_place_id = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    last_observed_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    hold_token = table.Column<Guid>(type: "uuid", nullable: true),
                    hold_expires_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    held_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    assigned_student_id = table.Column<Guid>(type: "uuid", nullable: true),
                    exam_registration_id = table.Column<Guid>(type: "uuid", nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    last_modified_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    last_modified_by_user_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_exam_places", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_exam_center_org_country_name",
                schema: "exams_certification",
                table: "exam_centers",
                columns: new[] { "organization_id", "country_code", "name" });

            migrationBuilder.CreateIndex(
                name: "ux_exam_center_external",
                schema: "exams_certification",
                table: "exam_centers",
                columns: new[] { "organization_id", "external_provider_code", "external_center_id" },
                unique: true,
                filter: "external_provider_code IS NOT NULL AND external_center_id IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_exam_place_calendar",
                schema: "exams_certification",
                table: "exam_places",
                columns: new[] { "organization_id", "starts_at_utc", "status" });

            migrationBuilder.CreateIndex(
                name: "ix_exam_place_center_slot",
                schema: "exams_certification",
                table: "exam_places",
                columns: new[] { "organization_id", "exam_center_id", "starts_at_utc", "license_category" });

            migrationBuilder.CreateIndex(
                name: "ux_exam_place_external",
                schema: "exams_certification",
                table: "exam_places",
                columns: new[] { "organization_id", "provider_code", "external_place_id" },
                unique: true,
                filter: "external_place_id IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "exam_centers",
                schema: "exams_certification");

            migrationBuilder.DropTable(
                name: "exam_places",
                schema: "exams_certification");
        }
    }
}
