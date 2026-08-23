using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DriveOS.Modules.Students.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class INT_FR_LN_003_StudentRegulatoryIdentities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "student_regulatory_identities",
                schema: "students",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: false),
                    student_id = table.Column<Guid>(type: "uuid", nullable: false),
                    country_code = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    identifier_type = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    identifier_value = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    source = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    declared_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    declared_by_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    verified_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    verified_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    verification_method = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    decision_reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    superseded_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    superseded_by_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_student_regulatory_identities", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ux_student_regulatory_identities_current",
                schema: "students",
                table: "student_regulatory_identities",
                columns: new[] { "organization_id", "student_id", "country_code", "identifier_type" },
                unique: true,
                filter: "status IN ('Declared', 'Verified')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "student_regulatory_identities",
                schema: "students");
        }
    }
}
