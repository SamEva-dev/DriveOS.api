using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DriveOS.Modules.CRM.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RefactorLeadConversionBoundary : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "enrollments",
                schema: "students");

            migrationBuilder.DropTable(
                name: "persons",
                schema: "students");

            migrationBuilder.DropIndex(
                name: "ix_lead_conversions_organization_person",
                schema: "crm",
                table: "lead_conversions");

            migrationBuilder.DropColumn(
                name: "enrollment_status",
                schema: "crm",
                table: "lead_conversions");

            migrationBuilder.RenameColumn(
                name: "program_code",
                schema: "crm",
                table: "lead_conversions",
                newName: "status");

            migrationBuilder.RenameColumn(
                name: "person_id",
                schema: "crm",
                table: "lead_conversions",
                newName: "responsible_user_id");

            migrationBuilder.RenameColumn(
                name: "draft_enrollment_id",
                schema: "crm",
                table: "lead_conversions",
                newName: "accepted_offer_id");

            migrationBuilder.AlterColumn<Guid>(
                name: "branch_id",
                schema: "crm",
                table: "lead_conversions",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "completed_at_utc",
                schema: "crm",
                table: "lead_conversions",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "consents_verified",
                schema: "crm",
                table: "lead_conversions",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "duplicate_check_completed",
                schema: "crm",
                table: "lead_conversions",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "guardian_summary",
                schema: "crm",
                table: "lead_conversions",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "identity_verified",
                schema: "crm",
                table: "lead_conversions",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "payer_summary",
                schema: "crm",
                table: "lead_conversions",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "required_document_codes",
                schema: "crm",
                table: "lead_conversions",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "student_enrollment_id",
                schema: "crm",
                table: "lead_conversions",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "student_person_id",
                schema: "crm",
                table: "lead_conversions",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "training_code",
                schema: "crm",
                table: "lead_conversions",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "completed_at_utc",
                schema: "crm",
                table: "lead_conversions");

            migrationBuilder.DropColumn(
                name: "consents_verified",
                schema: "crm",
                table: "lead_conversions");

            migrationBuilder.DropColumn(
                name: "duplicate_check_completed",
                schema: "crm",
                table: "lead_conversions");

            migrationBuilder.DropColumn(
                name: "guardian_summary",
                schema: "crm",
                table: "lead_conversions");

            migrationBuilder.DropColumn(
                name: "identity_verified",
                schema: "crm",
                table: "lead_conversions");

            migrationBuilder.DropColumn(
                name: "payer_summary",
                schema: "crm",
                table: "lead_conversions");

            migrationBuilder.DropColumn(
                name: "required_document_codes",
                schema: "crm",
                table: "lead_conversions");

            migrationBuilder.DropColumn(
                name: "student_enrollment_id",
                schema: "crm",
                table: "lead_conversions");

            migrationBuilder.DropColumn(
                name: "student_person_id",
                schema: "crm",
                table: "lead_conversions");

            migrationBuilder.DropColumn(
                name: "training_code",
                schema: "crm",
                table: "lead_conversions");

            migrationBuilder.EnsureSchema(
                name: "students");

            migrationBuilder.RenameColumn(
                name: "status",
                schema: "crm",
                table: "lead_conversions",
                newName: "program_code");

            migrationBuilder.RenameColumn(
                name: "responsible_user_id",
                schema: "crm",
                table: "lead_conversions",
                newName: "person_id");

            migrationBuilder.RenameColumn(
                name: "accepted_offer_id",
                schema: "crm",
                table: "lead_conversions",
                newName: "draft_enrollment_id");

            migrationBuilder.AlterColumn<Guid>(
                name: "branch_id",
                schema: "crm",
                table: "lead_conversions",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<string>(
                name: "enrollment_status",
                schema: "crm",
                table: "lead_conversions",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "enrollments",
                schema: "students",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    branch_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    last_modified_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    last_modified_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: false),
                    person_id = table.Column<Guid>(type: "uuid", nullable: false),
                    program_code = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    source_lead_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_enrollments", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "persons",
                schema: "students",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    email = table.Column<string>(type: "character varying(254)", maxLength: 254, nullable: true),
                    first_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    last_modified_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    last_modified_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    last_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: false),
                    phone = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_persons", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_lead_conversions_organization_person",
                schema: "crm",
                table: "lead_conversions",
                columns: new[] { "organization_id", "person_id" });

            migrationBuilder.CreateIndex(
                name: "ix_enrollments_organization_person",
                schema: "students",
                table: "enrollments",
                columns: new[] { "organization_id", "person_id" });

            migrationBuilder.CreateIndex(
                name: "ux_enrollments_organization_source_lead",
                schema: "students",
                table: "enrollments",
                columns: new[] { "organization_id", "source_lead_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_persons_organization_email",
                schema: "students",
                table: "persons",
                columns: new[] { "organization_id", "email" });
        }
    }
}
