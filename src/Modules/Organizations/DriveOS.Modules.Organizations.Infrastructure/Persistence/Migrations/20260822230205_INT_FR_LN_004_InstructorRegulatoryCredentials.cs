using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DriveOS.Modules.Organizations.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class INT_FR_LN_004_InstructorRegulatoryCredentials : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "instructor_regulatory_credentials",
                schema: "organization",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    InstructorUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CountryCode = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    CredentialType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Identifier = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    IssuingAuthority = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    JurisdictionCode = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    IssuedOn = table.Column<DateOnly>(type: "date", nullable: true),
                    ExpiresOn = table.Column<DateOnly>(type: "date", nullable: true),
                    Source = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    DeclaredAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    DeclaredByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    VerifiedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    VerifiedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    VerificationMethod = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    DecisionReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    SupersededAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    SupersededById = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_instructor_regulatory_credentials", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "regulatory_integration_connections",
                schema: "organization",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: false),
                    branch_id = table.Column<Guid>(type: "uuid", nullable: true),
                    scope_key = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    country_code = table.Column<string>(type: "character(2)", fixedLength: true, maxLength: 2, nullable: false),
                    provider_code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    external_account_reference = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    secret_reference = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    revision = table.Column<int>(type: "integer", nullable: false),
                    created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    last_modified_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    last_modified_by_user_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_regulatory_integration_connections", x => x.id);
                    table.ForeignKey(
                        name: "FK_regulatory_integration_connections_organizations_organizati~",
                        column: x => x.organization_id,
                        principalSchema: "organization",
                        principalTable: "organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_instructor_regulatory_credentials_OrganizationId_Instructor~",
                schema: "organization",
                table: "instructor_regulatory_credentials",
                columns: new[] { "OrganizationId", "InstructorUserId" });

            migrationBuilder.CreateIndex(
                name: "ux_instructor_regulatory_credentials_current",
                schema: "organization",
                table: "instructor_regulatory_credentials",
                columns: new[] { "OrganizationId", "InstructorUserId", "CountryCode", "CredentialType" },
                unique: true,
                filter: "\"Status\" IN (0, 1)");

            migrationBuilder.CreateIndex(
                name: "ix_regulatory_integration_connection_resolve",
                schema: "organization",
                table: "regulatory_integration_connections",
                columns: new[] { "organization_id", "country_code", "provider_code", "status" });

            migrationBuilder.CreateIndex(
                name: "ux_regulatory_integration_connection_scope_provider",
                schema: "organization",
                table: "regulatory_integration_connections",
                columns: new[] { "organization_id", "scope_key", "country_code", "provider_code" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "instructor_regulatory_credentials",
                schema: "organization");

            migrationBuilder.DropTable(
                name: "regulatory_integration_connections",
                schema: "organization");
        }
    }
}
