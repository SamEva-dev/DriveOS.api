using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DriveOS.Modules.CRM.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class fixeError : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_commercial_offers_org_status_valid_until",
                schema: "crm",
                table: "commercial_offers");

            migrationBuilder.RenameColumn(
                name: "notes",
                schema: "crm",
                table: "commercial_offers",
                newName: "financing_notes");

            migrationBuilder.RenameIndex(
                name: "ux_commercial_offers_org_lead_version",
                schema: "crm",
                table: "commercial_offers",
                newName: "IX_commercial_offers_organization_id_lead_id_version");

            migrationBuilder.AlterColumn<string>(
                name: "status",
                schema: "crm",
                table: "commercial_offers",
                type: "character varying(24)",
                maxLength: 24,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20);

            migrationBuilder.AddColumn<int>(
                name: "assessment_revision",
                schema: "crm",
                table: "commercial_offers",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "assessment_session_id",
                schema: "crm",
                table: "commercial_offers",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<decimal>(
                name: "catalog_amount",
                schema: "crm",
                table: "commercial_offers",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "conditions",
                schema: "crm",
                table: "commercial_offers",
                type: "character varying(8000)",
                maxLength: 8000,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "discount_amount",
                schema: "crm",
                table: "commercial_offers",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "estimated_funding_amount",
                schema: "crm",
                table: "commercial_offers",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "internal_notes",
                schema: "crm",
                table: "commercial_offers",
                type: "character varying(4000)",
                maxLength: 4000,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "prospect_remaining_amount",
                schema: "crm",
                table: "commercial_offers",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "tax_amount",
                schema: "crm",
                table: "commercial_offers",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "training_code",
                schema: "crm",
                table: "commercial_offers",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "assessment_type",
                schema: "crm",
                table: "assessment_appointments",
                type: "character varying(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "delivery_mode",
                schema: "crm",
                table: "assessment_appointments",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "evaluator_user_id",
                schema: "crm",
                table: "assessment_appointments",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "location_details",
                schema: "crm",
                table: "assessment_appointments",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "location_kind",
                schema: "crm",
                table: "assessment_appointments",
                type: "character varying(24)",
                maxLength: 24,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "price_amount",
                schema: "crm",
                table: "assessment_appointments",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "price_currency",
                schema: "crm",
                table: "assessment_appointments",
                type: "character varying(3)",
                maxLength: 3,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "room_id",
                schema: "crm",
                table: "assessment_appointments",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "simulator_id",
                schema: "crm",
                table: "assessment_appointments",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "vehicle_id",
                schema: "crm",
                table: "assessment_appointments",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "assessment_sessions",
                schema: "crm",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: false),
                    appointment_id = table.Column<Guid>(type: "uuid", nullable: false),
                    lead_id = table.Column<Guid>(type: "uuid", nullable: false),
                    evaluator_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    questionnaire_code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    questionnaire_version = table.Column<int>(type: "integer", nullable: false),
                    questionnaire_snapshot = table.Column<string>(type: "jsonb", nullable: false),
                    answers = table.Column<string>(type: "jsonb", nullable: false),
                    factual_observations = table.Column<string>(type: "character varying(8000)", maxLength: 8000, nullable: true),
                    pedagogical_interpretation = table.Column<string>(type: "character varying(8000)", maxLength: 8000, nullable: true),
                    recommendation = table.Column<string>(type: "character varying(8000)", maxLength: 8000, nullable: true),
                    internal_notes = table.Column<string>(type: "character varying(8000)", maxLength: 8000, nullable: true),
                    prospect_comment = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    status = table.Column<int>(type: "integer", nullable: false),
                    revision = table.Column<int>(type: "integer", nullable: false),
                    started_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    last_saved_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    submitted_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    submitted_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    result = table.Column<string>(type: "jsonb", nullable: true),
                    ai_suggestion = table.Column<string>(type: "jsonb", nullable: true),
                    result_confidence = table.Column<int>(type: "integer", nullable: true),
                    result_status = table.Column<int>(type: "integer", nullable: false),
                    correction_reason = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    result_validated_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    result_validated_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    result_shared_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    result_shared_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    last_modified_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    last_modified_by_user_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_assessment_sessions", x => x.id);
                    table.ForeignKey(
                        name: "FK_assessment_sessions_assessment_appointments_appointment_id",
                        column: x => x.appointment_id,
                        principalSchema: "crm",
                        principalTable: "assessment_appointments",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "commercial_offer_lines",
                schema: "crm",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    offer_id = table.Column<Guid>(type: "uuid", nullable: false),
                    line_type = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    service_id = table.Column<Guid>(type: "uuid", nullable: true),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    quantity = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    unit = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    unit_price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    discount_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    tax_rate = table.Column<decimal>(type: "numeric(7,4)", precision: 7, scale: 4, nullable: false),
                    net_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    tax_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    total_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    mandatory = table.Column<bool>(type: "boolean", nullable: false),
                    price_source = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    manual_override_reason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_commercial_offer_lines", x => x.id);
                    table.ForeignKey(
                        name: "FK_commercial_offer_lines_commercial_offers_offer_id",
                        column: x => x.offer_id,
                        principalSchema: "crm",
                        principalTable: "commercial_offers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "assessment_session_revisions",
                schema: "crm",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: false),
                    session_id = table.Column<Guid>(type: "uuid", nullable: false),
                    revision = table.Column<int>(type: "integer", nullable: false),
                    answers = table.Column<string>(type: "jsonb", nullable: false),
                    factual_observations = table.Column<string>(type: "character varying(8000)", maxLength: 8000, nullable: true),
                    pedagogical_interpretation = table.Column<string>(type: "character varying(8000)", maxLength: 8000, nullable: true),
                    recommendation = table.Column<string>(type: "character varying(8000)", maxLength: 8000, nullable: true),
                    internal_notes = table.Column<string>(type: "character varying(8000)", maxLength: 8000, nullable: true),
                    prospect_comment = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    result = table.Column<string>(type: "jsonb", nullable: true),
                    ai_suggestion = table.Column<string>(type: "jsonb", nullable: true),
                    result_confidence = table.Column<int>(type: "integer", nullable: true),
                    result_status = table.Column<int>(type: "integer", nullable: false),
                    correction_reason = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    status = table.Column<int>(type: "integer", nullable: false),
                    saved_by_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    saved_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_assessment_session_revisions", x => x.id);
                    table.ForeignKey(
                        name: "FK_assessment_session_revisions_assessment_sessions_session_id",
                        column: x => x.session_id,
                        principalSchema: "crm",
                        principalTable: "assessment_sessions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_commercial_offers_organization_id_assessment_session_id",
                schema: "crm",
                table: "commercial_offers",
                columns: new[] { "organization_id", "assessment_session_id" });

            migrationBuilder.CreateIndex(
                name: "ix_assessment_appointments_org_evaluator_start",
                schema: "crm",
                table: "assessment_appointments",
                columns: new[] { "organization_id", "evaluator_user_id", "starts_at_utc" });

            migrationBuilder.CreateIndex(
                name: "ix_assessment_appointments_org_room_start",
                schema: "crm",
                table: "assessment_appointments",
                columns: new[] { "organization_id", "room_id", "starts_at_utc" });

            migrationBuilder.CreateIndex(
                name: "ix_assessment_appointments_org_simulator_start",
                schema: "crm",
                table: "assessment_appointments",
                columns: new[] { "organization_id", "simulator_id", "starts_at_utc" });

            migrationBuilder.CreateIndex(
                name: "ix_assessment_appointments_org_vehicle_start",
                schema: "crm",
                table: "assessment_appointments",
                columns: new[] { "organization_id", "vehicle_id", "starts_at_utc" });

            migrationBuilder.CreateIndex(
                name: "IX_assessment_session_revisions_organization_id_session_id_rev~",
                schema: "crm",
                table: "assessment_session_revisions",
                columns: new[] { "organization_id", "session_id", "revision" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_assessment_session_revisions_session_id",
                schema: "crm",
                table: "assessment_session_revisions",
                column: "session_id");

            migrationBuilder.CreateIndex(
                name: "IX_assessment_sessions_appointment_id",
                schema: "crm",
                table: "assessment_sessions",
                column: "appointment_id");

            migrationBuilder.CreateIndex(
                name: "IX_assessment_sessions_organization_id_appointment_id",
                schema: "crm",
                table: "assessment_sessions",
                columns: new[] { "organization_id", "appointment_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_assessment_sessions_organization_id_evaluator_user_id_status",
                schema: "crm",
                table: "assessment_sessions",
                columns: new[] { "organization_id", "evaluator_user_id", "status" });

            migrationBuilder.CreateIndex(
                name: "IX_assessment_sessions_organization_id_lead_id",
                schema: "crm",
                table: "assessment_sessions",
                columns: new[] { "organization_id", "lead_id" });

            migrationBuilder.CreateIndex(
                name: "IX_assessment_sessions_organization_id_result_status",
                schema: "crm",
                table: "assessment_sessions",
                columns: new[] { "organization_id", "result_status" });

            migrationBuilder.CreateIndex(
                name: "IX_commercial_offer_lines_offer_id_line_type",
                schema: "crm",
                table: "commercial_offer_lines",
                columns: new[] { "offer_id", "line_type" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "assessment_session_revisions",
                schema: "crm");

            migrationBuilder.DropTable(
                name: "commercial_offer_lines",
                schema: "crm");

            migrationBuilder.DropTable(
                name: "assessment_sessions",
                schema: "crm");

            migrationBuilder.DropIndex(
                name: "IX_commercial_offers_organization_id_assessment_session_id",
                schema: "crm",
                table: "commercial_offers");

            migrationBuilder.DropIndex(
                name: "ix_assessment_appointments_org_evaluator_start",
                schema: "crm",
                table: "assessment_appointments");

            migrationBuilder.DropIndex(
                name: "ix_assessment_appointments_org_room_start",
                schema: "crm",
                table: "assessment_appointments");

            migrationBuilder.DropIndex(
                name: "ix_assessment_appointments_org_simulator_start",
                schema: "crm",
                table: "assessment_appointments");

            migrationBuilder.DropIndex(
                name: "ix_assessment_appointments_org_vehicle_start",
                schema: "crm",
                table: "assessment_appointments");

            migrationBuilder.DropColumn(
                name: "assessment_revision",
                schema: "crm",
                table: "commercial_offers");

            migrationBuilder.DropColumn(
                name: "assessment_session_id",
                schema: "crm",
                table: "commercial_offers");

            migrationBuilder.DropColumn(
                name: "catalog_amount",
                schema: "crm",
                table: "commercial_offers");

            migrationBuilder.DropColumn(
                name: "conditions",
                schema: "crm",
                table: "commercial_offers");

            migrationBuilder.DropColumn(
                name: "discount_amount",
                schema: "crm",
                table: "commercial_offers");

            migrationBuilder.DropColumn(
                name: "estimated_funding_amount",
                schema: "crm",
                table: "commercial_offers");

            migrationBuilder.DropColumn(
                name: "internal_notes",
                schema: "crm",
                table: "commercial_offers");

            migrationBuilder.DropColumn(
                name: "prospect_remaining_amount",
                schema: "crm",
                table: "commercial_offers");

            migrationBuilder.DropColumn(
                name: "tax_amount",
                schema: "crm",
                table: "commercial_offers");

            migrationBuilder.DropColumn(
                name: "training_code",
                schema: "crm",
                table: "commercial_offers");

            migrationBuilder.DropColumn(
                name: "assessment_type",
                schema: "crm",
                table: "assessment_appointments");

            migrationBuilder.DropColumn(
                name: "delivery_mode",
                schema: "crm",
                table: "assessment_appointments");

            migrationBuilder.DropColumn(
                name: "evaluator_user_id",
                schema: "crm",
                table: "assessment_appointments");

            migrationBuilder.DropColumn(
                name: "location_details",
                schema: "crm",
                table: "assessment_appointments");

            migrationBuilder.DropColumn(
                name: "location_kind",
                schema: "crm",
                table: "assessment_appointments");

            migrationBuilder.DropColumn(
                name: "price_amount",
                schema: "crm",
                table: "assessment_appointments");

            migrationBuilder.DropColumn(
                name: "price_currency",
                schema: "crm",
                table: "assessment_appointments");

            migrationBuilder.DropColumn(
                name: "room_id",
                schema: "crm",
                table: "assessment_appointments");

            migrationBuilder.DropColumn(
                name: "simulator_id",
                schema: "crm",
                table: "assessment_appointments");

            migrationBuilder.DropColumn(
                name: "vehicle_id",
                schema: "crm",
                table: "assessment_appointments");

            migrationBuilder.RenameColumn(
                name: "financing_notes",
                schema: "crm",
                table: "commercial_offers",
                newName: "notes");

            migrationBuilder.RenameIndex(
                name: "IX_commercial_offers_organization_id_lead_id_version",
                schema: "crm",
                table: "commercial_offers",
                newName: "ux_commercial_offers_org_lead_version");

            migrationBuilder.AlterColumn<string>(
                name: "status",
                schema: "crm",
                table: "commercial_offers",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(24)",
                oldMaxLength: 24);

            migrationBuilder.CreateIndex(
                name: "ix_commercial_offers_org_status_valid_until",
                schema: "crm",
                table: "commercial_offers",
                columns: new[] { "organization_id", "status", "valid_until_utc" });
        }
    }
}
