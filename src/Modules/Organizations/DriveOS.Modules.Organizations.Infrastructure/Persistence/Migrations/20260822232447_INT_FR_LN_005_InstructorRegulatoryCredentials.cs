using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DriveOS.Modules.Organizations.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class INT_FR_LN_005_InstructorRegulatoryCredentials : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ux_instructor_regulatory_credentials_current",
                schema: "organization",
                table: "instructor_regulatory_credentials");

            migrationBuilder.CreateIndex(
                name: "ux_instructor_regulatory_credentials_current",
                schema: "organization",
                table: "instructor_regulatory_credentials",
                columns: new[] { "OrganizationId", "InstructorUserId", "CountryCode", "CredentialType" },
                unique: true,
                filter: "\"Status\" IN (0, 1)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ux_instructor_regulatory_credentials_current",
                schema: "organization",
                table: "instructor_regulatory_credentials");

            migrationBuilder.CreateIndex(
                name: "ux_instructor_regulatory_credentials_current",
                schema: "organization",
                table: "instructor_regulatory_credentials",
                columns: new[] { "OrganizationId", "InstructorUserId", "CountryCode", "CredentialType" },
                unique: true,
                filter: "status IN (0, 1)");
        }
    }
}
