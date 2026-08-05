using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DriveOS.Modules.Organizations.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddOrganizationLegalProfiles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "organization_legal_profiles",
                schema: "organization",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: false),
                    legal_form = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    registration_number = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    tax_number = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    trade_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    incorporation_date = table.Column<DateOnly>(type: "date", nullable: true),
                    registered_address_line1 = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    registered_address_line2 = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    registered_postal_code = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    registered_city = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    registered_region = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    registered_country_code = table.Column<string>(type: "character(2)", fixedLength: true, maxLength: 2, nullable: false),
                    status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    revision = table.Column<int>(type: "integer", nullable: false),
                    created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    last_modified_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    last_modified_by_user_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_organization_legal_profiles", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_organization_legal_profiles_registration_status",
                schema: "organization",
                table: "organization_legal_profiles",
                columns: new[] { "registration_number", "status" });

            migrationBuilder.CreateIndex(
                name: "ux_organization_legal_profiles_organization",
                schema: "organization",
                table: "organization_legal_profiles",
                column: "organization_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "organization_legal_profiles",
                schema: "organization");
        }
    }
}
