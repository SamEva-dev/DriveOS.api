using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DriveOS.Modules.ExamsCertification.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddExamPlaceWatcher : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "exam_place_watch_hits",
                schema: "exams_certification",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    subscription_id = table.Column<Guid>(type: "uuid", nullable: false),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: false),
                    exam_place_id = table.Column<Guid>(type: "uuid", nullable: false),
                    first_detected_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_exam_place_watch_hits", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "exam_place_watch_scans",
                schema: "exams_certification",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    subscription_id = table.Column<Guid>(type: "uuid", nullable: false),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: false),
                    started_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    completed_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    is_success = table.Column<bool>(type: "boolean", nullable: false),
                    external_slots_read = table.Column<int>(type: "integer", nullable: false),
                    new_availabilities_detected = table.Column<int>(type: "integer", nullable: false),
                    error_code = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_exam_place_watch_scans", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "exam_place_watch_subscriptions",
                schema: "exams_certification",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: false),
                    provider_code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    country_code = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    administrative_area_code = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    exam_category = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    window_from_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    window_to_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    check_interval_minutes = table.Column<int>(type: "integer", nullable: false),
                    center_external_ids = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    status = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false),
                    next_check_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    last_checked_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    last_successful_check_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    last_availability_detected_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    last_error_code = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    consecutive_failure_count = table.Column<int>(type: "integer", nullable: false),
                    processing_lease_token = table.Column<Guid>(type: "uuid", nullable: true),
                    processing_lease_until_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    last_modified_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    last_modified_by_user_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_exam_place_watch_subscriptions", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_exam_place_watch_hit_detected",
                schema: "exams_certification",
                table: "exam_place_watch_hits",
                columns: new[] { "organization_id", "first_detected_at_utc" });

            migrationBuilder.CreateIndex(
                name: "ux_exam_place_watch_hit_once",
                schema: "exams_certification",
                table: "exam_place_watch_hits",
                columns: new[] { "subscription_id", "exam_place_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_exam_place_watch_scan_history",
                schema: "exams_certification",
                table: "exam_place_watch_scans",
                columns: new[] { "organization_id", "subscription_id", "started_at_utc" });

            migrationBuilder.CreateIndex(
                name: "ix_exam_place_watch_due",
                schema: "exams_certification",
                table: "exam_place_watch_subscriptions",
                columns: new[] { "status", "next_check_at_utc" });

            migrationBuilder.CreateIndex(
                name: "ix_exam_place_watch_provider",
                schema: "exams_certification",
                table: "exam_place_watch_subscriptions",
                columns: new[] { "organization_id", "provider_code" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "exam_place_watch_hits",
                schema: "exams_certification");

            migrationBuilder.DropTable(
                name: "exam_place_watch_scans",
                schema: "exams_certification");

            migrationBuilder.DropTable(
                name: "exam_place_watch_subscriptions",
                schema: "exams_certification");
        }
    }
}
