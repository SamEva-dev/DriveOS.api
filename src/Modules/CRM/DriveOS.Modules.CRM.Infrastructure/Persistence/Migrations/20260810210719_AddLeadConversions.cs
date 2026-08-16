using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DriveOS.Modules.CRM.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddLeadConversions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(name: "students");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "converted_at_utc",
                schema: "crm",
                table: "leads",
                type: "timestamp with time zone",
                nullable: true
            );

            migrationBuilder.AddColumn<Guid>(
                name: "converted_person_id",
                schema: "crm",
                table: "leads",
                type: "uuid",
                nullable: true
            );

            migrationBuilder.AddColumn<Guid>(
                name: "draft_enrollment_id",
                schema: "crm",
                table: "leads",
                type: "uuid",
                nullable: true
            );

            migrationBuilder.CreateTable(
                name: "enrollments",
                schema: "students",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: false),
                    person_id = table.Column<Guid>(type: "uuid", nullable: false),
                    source_lead_id = table.Column<Guid>(type: "uuid", nullable: false),
                    branch_id = table.Column<Guid>(type: "uuid", nullable: true),
                    program_code = table.Column<string>(
                        type: "character varying(30)",
                        maxLength: 30,
                        nullable: false
                    ),
                    status = table.Column<string>(
                        type: "character varying(20)",
                        maxLength: 20,
                        nullable: false
                    ),
                    created_at_utc = table.Column<DateTimeOffset>(
                        type: "timestamp with time zone",
                        nullable: false
                    ),
                    created_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    last_modified_at_utc = table.Column<DateTimeOffset>(
                        type: "timestamp with time zone",
                        nullable: true
                    ),
                    last_modified_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_enrollments", x => x.id);
                }
            );

            migrationBuilder.CreateTable(
                name: "lead_conversions",
                schema: "crm",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: false),
                    lead_id = table.Column<Guid>(type: "uuid", nullable: false),
                    person_id = table.Column<Guid>(type: "uuid", nullable: false),
                    draft_enrollment_id = table.Column<Guid>(type: "uuid", nullable: false),
                    branch_id = table.Column<Guid>(type: "uuid", nullable: true),
                    first_name = table.Column<string>(
                        type: "character varying(100)",
                        maxLength: 100,
                        nullable: false
                    ),
                    last_name = table.Column<string>(
                        type: "character varying(100)",
                        maxLength: 100,
                        nullable: false
                    ),
                    email = table.Column<string>(
                        type: "character varying(254)",
                        maxLength: 254,
                        nullable: true
                    ),
                    phone = table.Column<string>(
                        type: "character varying(40)",
                        maxLength: 40,
                        nullable: true
                    ),
                    program_code = table.Column<string>(
                        type: "character varying(30)",
                        maxLength: 30,
                        nullable: false
                    ),
                    enrollment_status = table.Column<string>(
                        type: "character varying(20)",
                        maxLength: 20,
                        nullable: false
                    ),
                    created_at_utc = table.Column<DateTimeOffset>(
                        type: "timestamp with time zone",
                        nullable: false
                    ),
                    created_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    last_modified_at_utc = table.Column<DateTimeOffset>(
                        type: "timestamp with time zone",
                        nullable: true
                    ),
                    last_modified_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_lead_conversions", x => x.id);
                }
            );

            migrationBuilder.CreateTable(
                name: "persons",
                schema: "students",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: false),
                    first_name = table.Column<string>(
                        type: "character varying(100)",
                        maxLength: 100,
                        nullable: false
                    ),
                    last_name = table.Column<string>(
                        type: "character varying(100)",
                        maxLength: 100,
                        nullable: false
                    ),
                    email = table.Column<string>(
                        type: "character varying(254)",
                        maxLength: 254,
                        nullable: true
                    ),
                    phone = table.Column<string>(
                        type: "character varying(40)",
                        maxLength: 40,
                        nullable: true
                    ),
                    created_at_utc = table.Column<DateTimeOffset>(
                        type: "timestamp with time zone",
                        nullable: false
                    ),
                    created_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    last_modified_at_utc = table.Column<DateTimeOffset>(
                        type: "timestamp with time zone",
                        nullable: true
                    ),
                    last_modified_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_persons", x => x.id);
                }
            );

            migrationBuilder.CreateIndex(
                name: "ix_enrollments_organization_person",
                schema: "students",
                table: "enrollments",
                columns: new[] { "organization_id", "person_id" }
            );

            migrationBuilder.CreateIndex(
                name: "ux_enrollments_organization_source_lead",
                schema: "students",
                table: "enrollments",
                columns: new[] { "organization_id", "source_lead_id" },
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "ix_lead_conversions_organization_person",
                schema: "crm",
                table: "lead_conversions",
                columns: new[] { "organization_id", "person_id" }
            );

            migrationBuilder.CreateIndex(
                name: "ux_lead_conversions_organization_lead",
                schema: "crm",
                table: "lead_conversions",
                columns: new[] { "organization_id", "lead_id" },
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "ix_persons_organization_email",
                schema: "students",
                table: "persons",
                columns: new[] { "organization_id", "email" }
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "enrollments", schema: "students");

            migrationBuilder.DropTable(name: "lead_conversions", schema: "crm");

            migrationBuilder.DropTable(name: "persons", schema: "students");

            migrationBuilder.DropColumn(name: "converted_at_utc", schema: "crm", table: "leads");

            migrationBuilder.DropColumn(name: "converted_person_id", schema: "crm", table: "leads");

            migrationBuilder.DropColumn(name: "draft_enrollment_id", schema: "crm", table: "leads");
        }
    }
}
